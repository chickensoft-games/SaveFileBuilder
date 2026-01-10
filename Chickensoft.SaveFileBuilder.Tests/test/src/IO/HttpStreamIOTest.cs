namespace Chickensoft.SaveFileBuilder.Tests.IO;

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Chickensoft.SaveFileBuilder.IO;

public class HttpStreamIOTest : IDisposable
{
  private CancellationToken CancellationToken { get; }

  private readonly MockHttpMessageHandler _mockHandler;
  private readonly HttpClient _httpClient;

  public HttpStreamIOTest(ITestContextAccessor testContextAccessor)
  {
    CancellationToken = testContextAccessor.Current.CancellationToken;

    _mockHandler = new MockHttpMessageHandler();
    _httpClient = new HttpClient(_mockHandler)
    {
      BaseAddress = new Uri("http://localhost:8080")
    };
  }

  public void Dispose()
  {
    _httpClient?.Dispose();
    GC.SuppressFinalize(this);
  }

  #region Constructor Tests

  [Fact]
  public async Task Constructor_WithHttpClientDisposeTrue_DisposesClientOnDispose()
  {
    // Arrange
    var handler = new MockHttpMessageHandler();
    var client = new HttpClient(handler);
    var streamIO = new HttpStreamIO(client, disposeClient: true);

    // Act
    streamIO.Dispose();

    // Assert - Verify client is disposed by trying to send a request
    await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
      await client.GetAsync("http://test.com", CancellationToken)
    );
  }

  [Fact]
  public void Constructor_WithHttpClientDisposeFalse_DoesNotDisposeClientOnDispose()
  {
    // Arrange
    var handler = new MockHttpMessageHandler();
    var client = new HttpClient(handler);
    var streamIO = new HttpStreamIO(client, disposeClient: false);

    // Act
    streamIO.Dispose();

    // Assert - client should still be usable
    var exception = Record.Exception(() => _ = client.BaseAddress);
    Assert.Null(exception);
    client.Dispose();
  }

  #endregion

  #region Headers Tests

  [Fact]
  public void ReadHeaders_CanAddHeaders()
  {
    // Arrange
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false);
    streamIO.ReadHeaders.Add("X-Custom-Header", "test-value");

    // Act
    var headerExists = streamIO.ReadHeaders.Contains("X-Custom-Header");

    // Assert
    Assert.True(headerExists);
  }

  [Fact]
  public void WriteHeaders_CanAddHeaders()
  {
    // Arrange
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false);
    streamIO.WriteHeaders.Add("X-Custom-Header", "test-value");

    // Act
    var headerExists = streamIO.WriteHeaders.Contains("X-Custom-Header");

    // Assert
    Assert.True(headerExists);
  }

  [Fact]
  public void WriteHeaders_CanSetContentLength()
  {
    // Arrange
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false);

    // Act
    streamIO.WriteHeaders.ContentLength = 1024;

    // Assert
    Assert.Equal(1024, streamIO.WriteHeaders.ContentLength);
  }

  #endregion

  #region ReadAsync Tests

  [Fact]
  public async Task ReadAsync_SuccessfulResponse_ReturnsStreamWithContent()
  {
    // Arrange
    var expectedContent = "test data";
    _mockHandler.SetupResponse(HttpStatusCode.OK, expectedContent);
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ReadUri: new Uri("api/read", UriKind.Relative))
    };

    // Act
    using var stream = await streamIO.ReadAsync(CancellationToken);

    // Assert
    Assert.NotNull(stream);
    stream.Position = 0;
    using var reader = new StreamReader(stream);
    var actualContent = await reader.ReadToEndAsync(CancellationToken);
    Assert.Equal(expectedContent, actualContent);
  }

  [Fact]
  public async Task ReadAsync_NotFoundResponse_ReturnsEmptyStream()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.NotFound, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ReadUri: new Uri("api/read", UriKind.Relative))
    };

    // Act
    using var stream = await streamIO.ReadAsync(CancellationToken);

    // Assert
    Assert.NotNull(stream);
    Assert.Equal(0, stream.Length);
  }

  [Fact]
  public async Task ReadAsync_CancelledToken_ThrowsOperationCanceledException()
  {
    // Arrange
    var cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
    cts.Cancel();
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ReadUri: new Uri("api/read", UriKind.Relative))
    };

    // Act & Assert
    await Assert.ThrowsAsync<TaskCanceledException>(
      async () => await streamIO.ReadAsync(cts.Token)
    );
  }

  [Fact]
  public async Task ReadAsync_ServerError_ThrowsHttpRequestException()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.InternalServerError, "Server Error");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ReadUri: new Uri("api/read", UriKind.Relative))
    };

    // Act & Assert
    await Assert.ThrowsAsync<HttpRequestException>(
      async () => await streamIO.ReadAsync(CancellationToken)
    );
  }

  #endregion

  #region WriteAsync Tests

  [Fact]
  public async Task WriteAsync_ValidStream_PostsDataSuccessfully()
  {
    // Arrange
    var testData = "test write data";
    var stream = new MemoryStream(Encoding.UTF8.GetBytes(testData));
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(WriteUri: new Uri("api/write", UriKind.Relative))
    };

    // Act
    await streamIO.WriteAsync(stream, CancellationToken);

    // Assert
    Assert.True(_mockHandler.RequestReceived);
    Assert.Equal(HttpMethod.Post, _mockHandler.LastRequest?.Method);
  }

  [Fact]
  public async Task WriteAsync_UsesStreamLength_WhenContentLengthIsNull()
  {
    // Arrange
    var testData = "test data";
    var stream = new MemoryStream(Encoding.UTF8.GetBytes(testData));
    var streamLength = stream.Length;
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(WriteUri: new Uri("api/write", UriKind.Relative))
    };
    streamIO.WriteHeaders.ContentLength = null;

    // Act
    await streamIO.WriteAsync(stream, CancellationToken);

    // Assert
    Assert.Equal(streamLength, _mockHandler.LastRequest?.Content?.Headers.ContentLength);

  }

  [Fact]
  public async Task WriteAsync_UsesCustomContentLength_WhenSet()
  {
    // Arrange
    var testData = "test data";
    var stream = new MemoryStream(Encoding.UTF8.GetBytes(testData));
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(WriteUri: new Uri("api/write", UriKind.Relative))
    };
    streamIO.WriteHeaders.ContentLength = 5;

    // Act
    await streamIO.WriteAsync(stream, CancellationToken);

    // Assert
    Assert.Equal(5, _mockHandler.LastRequest?.Content?.Headers.ContentLength);
  }

  [Fact]
  public async Task WriteAsync_CopiesAllHeadersExceptContentLength()
  {
    // Arrange
    var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(WriteUri: new Uri("api/write", UriKind.Relative))
    };
    streamIO.WriteHeaders.Add("X-Custom-Header", "custom-value");
    streamIO.WriteHeaders.ContentType = new MediaTypeHeaderValue("application/json");

    // Act
    await streamIO.WriteAsync(stream, CancellationToken);

    // Assert
    IEnumerable<string>? contentTypeValues = [];
    Assert.True(_mockHandler.LastRequest?.Content?.Headers.TryGetValues("X-Custom-Header", out contentTypeValues));
    Assert.NotNull(contentTypeValues);
    Assert.Single(contentTypeValues);
    Assert.Equal("custom-value", contentTypeValues.First());

    Assert.Equal("application/json", _mockHandler.LastRequest?.Content?.Headers.ContentType?.MediaType);
  }

  [Fact]
  public async Task WriteAsync_CancelledToken_ThrowsOperationCanceledException()
  {
    // Arrange
    var cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
    cts.Cancel();
    var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(WriteUri: new Uri("api/write", UriKind.Relative))
    };

    // Act & Assert
    await Assert.ThrowsAsync<TaskCanceledException>(
      async () => await streamIO.WriteAsync(stream, cts.Token)
    );
  }

  #endregion

  #region ExistsAsync Tests

  [Fact]
  public async Task ExistsAsync_SuccessStatusCode_ReturnsTrue()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ExistsUri: new Uri("api/exists", UriKind.Relative))
    };

    // Act
    var exists = await streamIO.ExistsAsync(CancellationToken);

    // Assert
    Assert.True(exists);
  }

  [Fact]
  public async Task ExistsAsync_NotFoundStatusCode_ReturnsFalse()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.NotFound, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ExistsUri: new Uri("api/exists", UriKind.Relative))
    };

    // Act
    var exists = await streamIO.ExistsAsync(CancellationToken);

    // Assert
    Assert.False(exists);
  }

  [Fact]
  public async Task ExistsAsync_OtherErrorStatusCode_ReturnsFalse()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.InternalServerError, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ExistsUri: new Uri("api/exists", UriKind.Relative))
    };

    // Act
    var exists = await streamIO.ExistsAsync(CancellationToken);

    // Assert
    Assert.False(exists);
  }

  [Fact]
  public async Task ExistsAsync_CancelledToken_ThrowsOperationCanceledException()
  {
    // Arrange
    var cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
    cts.Cancel();
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ExistsUri: new Uri("api/exists", UriKind.Relative))
    };

    // Act & Assert
    await Assert.ThrowsAsync<TaskCanceledException>(
      async () => await streamIO.ExistsAsync(cts.Token)
    );
  }

  #endregion

  #region DeleteAsync Tests

  [Fact]
  public async Task DeleteAsync_SuccessStatusCode_ReturnsTrue()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(DeleteUri: new Uri("api/delete", UriKind.Relative))
    };

    // Act
    var deleted = await streamIO.DeleteAsync(CancellationToken);

    // Assert
    Assert.True(deleted);
  }

  [Fact]
  public async Task DeleteAsync_NoContentStatusCode_ReturnsTrue()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.NoContent, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(DeleteUri: new Uri("api/delete", UriKind.Relative))
    };

    // Act
    var deleted = await streamIO.DeleteAsync(CancellationToken);

    // Assert
    Assert.True(deleted);
  }

  [Fact]
  public async Task DeleteAsync_NotFoundStatusCode_ReturnsFalse()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.NotFound, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(DeleteUri: new Uri("api/delete", UriKind.Relative))
    };

    // Act
    var deleted = await streamIO.DeleteAsync(CancellationToken);

    // Assert
    Assert.False(deleted);
  }

  [Fact]
  public async Task DeleteAsync_ErrorStatusCode_ReturnsFalse()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.InternalServerError, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(DeleteUri: new Uri("api/delete", UriKind.Relative))
    };

    // Act
    var deleted = await streamIO.DeleteAsync(CancellationToken);

    // Assert
    Assert.False(deleted);
  }

  [Fact]
  public async Task DeleteAsync_CancelledToken_ThrowsOperationCanceledException()
  {
    // Arrange
    var cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
    cts.Cancel();
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(DeleteUri: new Uri("api/delete", UriKind.Relative))
    };

    // Act & Assert
    await Assert.ThrowsAsync<TaskCanceledException>(
      async () => await streamIO.DeleteAsync(cts.Token)
    );
  }

  [Fact]
  public async Task DeleteAsync_UsesDeleteHttpMethod()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(DeleteUri: new Uri("api/delete", UriKind.Relative))
    };

    // Act
    await streamIO.DeleteAsync(CancellationToken);

    // Assert
    Assert.True(_mockHandler.RequestReceived);
    Assert.Equal(HttpMethod.Delete, _mockHandler.LastRequest?.Method);
  }

  #endregion

  #region Dispose Tests

  [Fact]
  public void Dispose_CalledMultipleTimes_DoesNotThrow()
  {
    // Arrange
    var handler = new MockHttpMessageHandler();
    var client = new HttpClient(handler);
    var streamIO = new HttpStreamIO(client, disposeClient: true);

    // Act & Assert
    streamIO.Dispose();
    var exception = Record.Exception(streamIO.Dispose);
    Assert.Null(exception);
  }

  [Fact]
  public void Dispose_DisposeClientFalse_DoesNotDisposeHttpClient()
  {
    // Arrange
    var handler = new MockHttpMessageHandler();
    var client = new HttpClient(handler);
    var streamIO = new HttpStreamIO(client, disposeClient: false);

    // Act
    streamIO.Dispose();

    // Assert - client should still be usable
    var exception = Record.Exception(() => _ = client.BaseAddress);
    Assert.Null(exception);
    client.Dispose();
  }

  #endregion
}

/// <summary>
/// Mock HttpMessageHandler for testing HTTP requests without actual network calls.
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
  private HttpStatusCode _statusCode = HttpStatusCode.OK;
  private string _content = "";

  [MemberNotNullWhen(true, nameof(LastRequest))]
  public bool RequestReceived { get; private set; }

  public HttpRequestMessage? LastRequest { get; private set; }

  public void SetupResponse(HttpStatusCode statusCode, string content)
  {
    _statusCode = statusCode;
    _content = content;
  }

  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    RequestReceived = true;
    LastRequest = request;

    var response = new HttpResponseMessage(_statusCode)
    {
      Content = new StringContent(_content, Encoding.UTF8, "application/json")
    };

    return Task.FromResult(response);
  }
}
