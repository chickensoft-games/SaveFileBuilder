namespace Chickensoft.SaveFileBuilder;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public readonly record struct HttpIORequestUris(
  Uri? ReadUri = null,
  Uri? WriteUri = null,
  Uri? ExistsUri = null,
  Uri? DeleteUri = null
)
{
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

public class HttpIO : IAsyncIOStreamProvider, IDisposable
{
  private bool _isDisposed;

  private readonly HttpClient _httpClient;
  private readonly bool _disposeClient;
  private readonly HttpContent _emptyContent = new ByteArrayContent([]);

  public HttpIORequestUris RequestUris { get; init; }

  public HttpRequestHeaders ReadHeaders => _httpClient.DefaultRequestHeaders;
  public HttpContentHeaders WriteHeaders => _emptyContent.Headers;

  public HttpIO()
    : this(new HttpClient())
  { }

  public HttpIO(Uri? baseAddress)
    : this(new HttpClient()
    {
      BaseAddress = baseAddress,
    })
  { }

  public HttpIO(Uri? baseAddress, TimeSpan timeout)
    : this(new HttpClient()
    {
      BaseAddress = baseAddress,
      Timeout = timeout
    })
  { }

  public HttpIO(string? baseAddress)
  : this(baseAddress is not null ? new Uri(baseAddress) : null)
  { }

  public HttpIO(string? baseAddress, TimeSpan timeout)
    : this(baseAddress is not null ? new Uri(baseAddress) : null, timeout)
  { }

  public HttpIO(HttpClient client, bool disposeClient = true)
  {
    _httpClient = client;
    _disposeClient = disposeClient;
  }

  public async Task<Stream> ReadAsync(CancellationToken cancellationToken = default)
  {
    using var response = await _httpClient.GetAsync(RequestUris.ReadUri, cancellationToken).ConfigureAwait(false);

    try
    {
      response.EnsureSuccessStatusCode();
    }
    catch (HttpRequestException)
    when (response.StatusCode is HttpStatusCode.NotFound)
    {
      return new MemoryStream();
    }
    catch
    {
      throw;
    }

    await using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

    var memoryStream = new MemoryStream();
    await contentStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
    memoryStream.Position = 0;
    return memoryStream;
  }

  public async Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
  {
    stream.Position = 0;

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

    await _httpClient.PostAsync(RequestUris.WriteUri, content, cancellationToken).ConfigureAwait(false);
  }

  public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
  {
    using var response = await _httpClient.GetAsync(RequestUris.ExistsUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteAsync(CancellationToken cancellationToken = default)
  {
    using var response = await _httpClient.DeleteAsync(RequestUris.DeleteUri, cancellationToken).ConfigureAwait(false);
    return response.IsSuccessStatusCode;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

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
