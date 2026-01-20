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

  #region HttpIORequestUris Tests

  [Fact]
  public void HttpIORequestUris_DefaultConstructor_AllUrisAreNull()
  {
    // Arrange & Act
    var uris = new HttpIORequestUris();

    // Assert
    Assert.Null(uris.ReadUri);
    Assert.Null(uris.WriteUri);
    Assert.Null(uris.ExistsUri);
    Assert.Null(uris.DeleteUri);
  }

  [Fact]
  public void HttpIORequestUris_WithUriParameters_SetsUrisCorrectly()
  {
    // Arrange
    var readUri = new Uri("api/read", UriKind.Relative);
    var writeUri = new Uri("api/write", UriKind.Relative);
    var existsUri = new Uri("api/exists", UriKind.Relative);
    var deleteUri = new Uri("api/delete", UriKind.Relative);

    // Act
    var uris = new HttpIORequestUris(readUri, writeUri, existsUri, deleteUri);

    // Assert
    Assert.Equal(readUri, uris.ReadUri);
    Assert.Equal(writeUri, uris.WriteUri);
    Assert.Equal(existsUri, uris.ExistsUri);
    Assert.Equal(deleteUri, uris.DeleteUri);
  }

  [Fact]
  public void HttpIORequestUris_WithStringParameters_SetsUrisCorrectly()
  {
    // Arrange & Act
    var uris = new HttpIORequestUris(
      readUri: "api/read",
      writeUri: "api/write",
      existsUri: "api/exists",
      deleteUri: "api/delete"
    );

    // Assert
    Assert.NotNull(uris.ReadUri);
    Assert.Equal("api/read", uris.ReadUri.ToString());
    Assert.NotNull(uris.WriteUri);
    Assert.Equal("api/write", uris.WriteUri.ToString());
    Assert.NotNull(uris.ExistsUri);
    Assert.Equal("api/exists", uris.ExistsUri.ToString());
    Assert.NotNull(uris.DeleteUri);
    Assert.Equal("api/delete", uris.DeleteUri.ToString());
  }

  [Fact]
  public void HttpIORequestUris_WithNullStringParameters_SetsUrisToNull()
  {
    // Arrange & Act
    var uris = new HttpIORequestUris(
      readUri: null,
      writeUri: null,
      existsUri: null,
      deleteUri: null
    );

    // Assert
    Assert.Null(uris.ReadUri);
    Assert.Null(uris.WriteUri);
    Assert.Null(uris.ExistsUri);
    Assert.Null(uris.DeleteUri);
  }

  [Fact]
  public void HttpIORequestUris_WithPartialStringParameters_SetsSomeUrisCorrectly()
  {
    // Arrange & Act
    var uris = new HttpIORequestUris(
      readUri: "api/read",
      writeUri: null,
      existsUri: "api/exists",
      deleteUri: null
    );

    // Assert
    Assert.NotNull(uris.ReadUri);
    Assert.Equal("api/read", uris.ReadUri.ToString());
    Assert.Null(uris.WriteUri);
    Assert.NotNull(uris.ExistsUri);
    Assert.Equal("api/exists", uris.ExistsUri.ToString());
    Assert.Null(uris.DeleteUri);
  }

  [Fact]
  public void HttpIORequestUris_CanBeSet_UsingInitSyntax()
  {
    // Arrange
    var readUri = new Uri("api/read", UriKind.Relative);
    var writeUri = new Uri("api/write", UriKind.Relative);
    var existsUri = new Uri("api/exists", UriKind.Relative);
    var deleteUri = new Uri("api/delete", UriKind.Relative);
    // Act
    var uris = new HttpIORequestUris(
      readUri: "api/ignored",
      writeUri: "api/ignored",
      existsUri: "api/ignored",
      deleteUri: "api/ignored"
    )
    {
      ReadUri = readUri,
      WriteUri = writeUri,
      ExistsUri = existsUri,
      DeleteUri = deleteUri
    };
    // Assert
    Assert.Equal(readUri, uris.ReadUri);
    Assert.Equal(writeUri, uris.WriteUri);
    Assert.Equal(existsUri, uris.ExistsUri);
    Assert.Equal(deleteUri, uris.DeleteUri);
  }

  #endregion

  #region Constructor Tests

  [Fact]
  public void Constructor_Default_CreatesInstanceSuccessfully()
  {
    // Arrange & Act
    using var streamIO = new HttpStreamIO();

    // Assert
    Assert.NotNull(streamIO);
    Assert.NotNull(streamIO.ReadHeaders);
    Assert.NotNull(streamIO.WriteHeaders);
  }

  [Fact]
  public void Constructor_WithTimeout_SetsTimeoutCorrectly()
  {
    // Arrange
    var timeout = TimeSpan.FromSeconds(30);

    // Act
    using var streamIO = new HttpStreamIO(timeout);

    // Assert
    Assert.NotNull(streamIO);
  }

  [Fact]
  public void Constructor_WithUriBaseAddress_SetsBaseAddressCorrectly()
  {
    // Arrange
    var baseAddress = new Uri("http://example.com");

    // Act
    using var streamIO = new HttpStreamIO(baseAddress);

    // Assert
    Assert.NotNull(streamIO);
  }

  [Fact]
  public void Constructor_WithUriBaseAddressAndTimeout_SetsPropertiesCorrectly()
  {
    // Arrange
    var baseAddress = new Uri("http://example.com");
    var timeout = TimeSpan.FromSeconds(45);

    // Act
    using var streamIO = new HttpStreamIO(baseAddress, timeout);

    // Assert
    Assert.NotNull(streamIO);
  }

  [Fact]
  public void Constructor_WithStringBaseAddress_SetsBaseAddressCorrectly()
  {
    // Arrange
    var baseAddress = "http://example.com";

    // Act
    using var streamIO = new HttpStreamIO(baseAddress);

    // Assert
    Assert.NotNull(streamIO);
  }

  [Fact]
  public void Constructor_WithStringBaseAddressAndTimeout_SetsPropertiesCorrectly()
  {
    // Arrange
    var baseAddress = "http://example.com";
    var timeout = TimeSpan.FromSeconds(60);

    // Act
    using var streamIO = new HttpStreamIO(baseAddress, timeout);

    // Assert
    Assert.NotNull(streamIO);
  }

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

  [Fact]
  public void RequestUris_CanBeSet_UsingInitSyntax()
  {
    // Arrange
    var readUri = new Uri("api/custom-read", UriKind.Relative);
    var writeUri = new Uri("api/custom-write", UriKind.Relative);

    // Act
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ReadUri: readUri, WriteUri: writeUri)
    };

    // Assert
    Assert.Equal(readUri, streamIO.RequestUris.ReadUri);
    Assert.Equal(writeUri, streamIO.RequestUris.WriteUri);
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

  [Fact]
  public void WriteHeaders_WithMultipleCustomHeaders_AllHeadersAreSet()
  {
    // Arrange
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false);

    // Act
    streamIO.WriteHeaders.Add("X-Header-1", "value1");
    streamIO.WriteHeaders.Add("X-Header-2", "value2");
    streamIO.WriteHeaders.Add("X-Header-3", "value3");

    // Assert
    Assert.True(streamIO.WriteHeaders.Contains("X-Header-1"));
    Assert.True(streamIO.WriteHeaders.Contains("X-Header-2"));
    Assert.True(streamIO.WriteHeaders.Contains("X-Header-3"));
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

  [Fact]
  public async Task ReadAsync_WithEmptyResponse_ReturnsEmptyStream()
  {
    // Arrange
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ReadUri: new Uri("api/read", UriKind.Relative))
    };

    // Act
    using var stream = await streamIO.ReadAsync(CancellationToken);

    // Assert
    Assert.Equal(0, stream.Length);
  }

  [Fact]
  public async Task ReadAsync_WithLargeResponse_ReturnsFullContent()
  {
    // Arrange
    var largeContent = new string('X', 1024 * 1024); // 1MB of data
    _mockHandler.SetupResponse(HttpStatusCode.OK, largeContent);
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(ReadUri: new Uri("api/read", UriKind.Relative))
    };

    // Act
    using var stream = await streamIO.ReadAsync(CancellationToken);

    // Assert
    Assert.Equal(largeContent.Length, stream.Length);
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

  [Fact]
  public async Task WriteAsync_WithEmptyStream_PostsSuccessfully()
  {
    // Arrange
    var stream = new MemoryStream();
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(WriteUri: new Uri("api/write", UriKind.Relative))
    };

    // Act
    await streamIO.WriteAsync(stream, CancellationToken);

    // Assert
    Assert.Equal(0, _mockHandler.LastRequest?.Content?.Headers.ContentLength);
  }

  [Fact]
  public async Task WriteAsync_WithLargeStream_PostsSuccessfully()
  {
    // Arrange
    var largeData = new byte[1024 * 1024]; // 1MB
    Array.Fill(largeData, (byte)'A');
    var stream = new MemoryStream(largeData);
    _mockHandler.SetupResponse(HttpStatusCode.OK, "");
    using var streamIO = new HttpStreamIO(_httpClient, disposeClient: false)
    {
      RequestUris = new HttpIORequestUris(WriteUri: new Uri("api/write", UriKind.Relative))
    };

    // Act
    await streamIO.WriteAsync(stream, CancellationToken);

    // Assert
    Assert.Equal(largeData.Length, _mockHandler.LastRequest?.Content?.Headers.ContentLength);
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
internal sealed class MockHttpMessageHandler : HttpMessageHandler
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
