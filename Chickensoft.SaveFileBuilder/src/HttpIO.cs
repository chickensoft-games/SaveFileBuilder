namespace Chickensoft.SaveFileBuilder;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public readonly record struct HttpIOConfig(
  Uri? ReadUri = null,
  Uri? WriteUri = null,
  Uri? ExistsUri = null,
  Uri? DeleteUri = null
)
{
  public HttpIOConfig(
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

  public readonly HttpIOConfig Config;

  public HttpIO(Uri? baseAddress = null, HttpIOConfig config = default)
  {
    _httpClient = new HttpClient()
    {
      BaseAddress = baseAddress
    };

    Config = config;
  }

  public async Task<Stream> ReadAsync(CancellationToken cancellationToken = default)
  {
    HttpResponseMessage response = default!;
    try
    {
      response = await _httpClient.GetAsync(Config.ReadUri, cancellationToken).ConfigureAwait(false);
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }
    catch (HttpRequestException)
    when (response.StatusCode is HttpStatusCode.NotFound)
    {
      return new MemoryStream();
    }
  }

  public async Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
  {
    stream.Position = 0;

    var content = new StreamContent(stream);
    content.Headers.ContentLength = stream.Length;

    await _httpClient.PostAsync(Config.WriteUri, content, cancellationToken).ConfigureAwait(false);
  }

  public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetAsync(Config.ExistsUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteAsync(CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.DeleteAsync(Config.DeleteUri, cancellationToken).ConfigureAwait(false);
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
        _httpClient.Dispose();
      }

      _isDisposed = true;
    }
  }
}
