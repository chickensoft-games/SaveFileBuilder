namespace Chickensoft.SaveFileBuilder.IO;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Defines the relative <see cref="Uri"/>'s used for specific HTTP requests by the <see cref="HttpStreamIO"/>.</summary>
/// <param name="ReadUri">The relative <see cref="Uri"/> used for read requests.</param>
/// <param name="WriteUri">The relative <see cref="Uri"/> used for write requests.</param>
/// <param name="ExistsUri">The relative <see cref="Uri"/> used for exists requests.</param>
/// <param name="DeleteUri">The relative <see cref="Uri"/> used for delete requests.</param>
public readonly record struct HttpIORequestUris(
  Uri? ReadUri = null,
  Uri? WriteUri = null,
  Uri? ExistsUri = null,
  Uri? DeleteUri = null
)
{
  /// <inheritdoc cref="HttpIORequestUris" />
  /// <param name="readUri">The relative address used for read requests.</param>
  /// <param name="writeUri">The relative address used for write requests.</param>
  /// <param name="existsUri">The relative address used for exists requests.</param>
  /// <param name="deleteUri">The relative address used for delete requests.</param>
  public HttpIORequestUris(
    string? readUri = null,
    string? writeUri = null,
    string? existsUri = null,
    string? deleteUri = null
  ) : this(
    readUri is not null ? new Uri(readUri) : null,
    writeUri is not null ? new Uri(writeUri) : null,
    existsUri is not null ? new Uri(existsUri) : null,
    deleteUri is not null ? new Uri(deleteUri) : null
  )
  { }
}

/// <summary>Provides a read <see cref="Stream"/> from- and requests a write <see cref="Stream"/> for an Http address.</summary>
public class HttpStreamIO : IAsyncStreamIO, IDisposable
{
  private bool _isDisposed;

  private readonly HttpClient _httpClient;
  private readonly bool _disposeClient;
  private readonly HttpContent _emptyContent = new ByteArrayContent([])
  {
    Headers = { ContentLength = null }
  };

  /// <summary>Gets the relative <see cref="Uri"/>'s used for specific requests.</summary>
  /// <returns>The relative <see cref="Uri"/>'s used for specific requests.</returns>
  public HttpIORequestUris RequestUris { get; init; }

  /// <summary>Gets the <see cref="HttpContentHeaders"/> to be sent when reading data.</summary>
  /// <returns>The <see cref="HttpContentHeaders"/> to be sent when reading data.</returns>
  public HttpRequestHeaders ReadHeaders => _httpClient.DefaultRequestHeaders;

  /// <summary>Gets the <see cref="HttpContentHeaders"/>, as defined in RFC 2616, to be sent when writing data.</summary>
  /// <returns>The <see cref="HttpContentHeaders"/>, as defined in RFC 2616, to be sent when writing data.</returns>
  /// <remarks>If the <see cref="HttpContentHeaders.ContentLength"/> is left null, it will be set to the length of the stream being written. In most cases, this is the desired behavior.</remarks>
  public HttpContentHeaders WriteHeaders => _emptyContent.Headers;

  /// <summary>Initializes a new instance of the <see cref="HttpStreamIO"/> class.</summary>
  public HttpStreamIO()
    : this(new HttpClient())
  { }

  /// <summary>Initializes a new instance of the <see cref="HttpStreamIO"/> class with the specified timeout.</summary>
  /// <inheritdoc cref="HttpStreamIO(string, TimeSpan)" path="/param[@name='timeout']"/>
  public HttpStreamIO(TimeSpan timeout)
    : this(new HttpClient()
    {
      Timeout = timeout
    })
  { }

  /// <inheritdoc cref="HttpStreamIO(string)" />
  public HttpStreamIO(Uri baseAddress)
    : this(new HttpClient()
    {
      BaseAddress = baseAddress,
    })
  { }

  /// <inheritdoc cref="HttpStreamIO(string, TimeSpan)" />
  public HttpStreamIO(Uri baseAddress, TimeSpan timeout)
    : this(new HttpClient()
    {
      BaseAddress = baseAddress,
      Timeout = timeout
    })
  { }

  /// <summary>Initializes a new instance of the <see cref="HttpStreamIO"/> class with the specified address.</summary>
  /// <inheritdoc cref="HttpStreamIO(string, TimeSpan)" path="/param[@name='baseAddress']"/>
  public HttpStreamIO(string baseAddress)
    : this(new Uri(baseAddress))
  { }

  /// <summary>Initializes a new instance of the <see cref="HttpStreamIO"/> class with the specified address and timeout.</summary>
  /// <param name="baseAddress">The base address used when sending requests.</param>
  /// <param name="timeout">The time to wait before a request times out.</param>
  public HttpStreamIO(string baseAddress, TimeSpan timeout)
    : this(new Uri(baseAddress), timeout)
  { }

  /// <summary>Initializes a new instance of the <see cref="HttpStreamIO"/> class with the specified client, and specifies whether that client should be disposed when this instance is disposed.</summary>
  /// <param name="client">The <see cref="HttpClient"/> to use for requests.</param>
  /// <param name="disposeClient"><see langword="true"/> if the inner client should be disposed of by <see cref="Dispose()"/>; <see langword="false"/> if you intend to reuse the client.</param>
  public HttpStreamIO(HttpClient client, bool disposeClient = true)
  {
    _httpClient = client;
    _disposeClient = disposeClient;
  }

  /// <inheritdoc />
  public async Task<Stream> ReadAsync(CancellationToken cancellationToken = default)
  {
    using var response = await _httpClient.GetAsync(RequestUris.ReadUri, cancellationToken);

    try
    {
      response.EnsureSuccessStatusCode();
    }
    catch (HttpRequestException)
    when (response.StatusCode is HttpStatusCode.NotFound)
    {
      return new MemoryStream();
    }

    await using var contentStream = await response.Content.ReadAsStreamAsync();

    var readStream = new MemoryStream();
    await contentStream.CopyToAsync(readStream, cancellationToken);
    readStream.Position = 0;
    return readStream;
  }

  /// <inheritdoc />
  public async Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
  {
    using var content = new StreamContent(stream)
    {
      Headers = { ContentLength = WriteHeaders.ContentLength ?? stream.Length }
    };
    foreach (var header in WriteHeaders)
    {
      if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      content.Headers.Add(header.Key, header.Value);
    }

    await _httpClient.PostAsync(RequestUris.WriteUri, content, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
  {
    using var response = await _httpClient.GetAsync(RequestUris.ExistsUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    return response.IsSuccessStatusCode;
  }

  /// <inheritdoc />
  public async Task<bool> DeleteAsync(CancellationToken cancellationToken = default)
  {
    using var response = await _httpClient.DeleteAsync(RequestUris.DeleteUri, cancellationToken);
    return response.IsSuccessStatusCode;
  }

  /// <inheritdoc />
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <inheritdoc cref="IDisposable.Dispose" />
  protected virtual void Dispose(bool disposing)
  {
    if (!_isDisposed)
    {
      if (disposing)
      {
        if (_disposeClient)
        {
          _httpClient.Dispose();
        }
        _emptyContent.Dispose();
      }

      _isDisposed = true;
    }
  }
}
