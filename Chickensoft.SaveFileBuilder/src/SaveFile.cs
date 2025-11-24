namespace Chickensoft.SaveFileBuilder;

using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a save file composed of one or more save chunks.
/// </summary>
/// <typeparam name="TData">Type of data represented by the save file.
/// </typeparam>
public interface ISaveFile<TData> where TData : class
{
  /// <summary>
  /// Root save chunk from which the save file contents are composed.
  /// </summary>
  ISaveChunk<TData> Root { get; }

  bool CanSaveSynchronously { get; }

  void Save(CompressionLevel compressionLevel = default);

  void Load();

  bool Exists();

  void Delete();

  /// <summary>
  /// Collects save data from the save file chunk tree and saves it.
  /// </summary>
  /// <returns>Asynchronous task.</returns>
  ValueTask SaveAsync(CompressionLevel compressionLevel = default, CancellationToken cancellationToken = default);

  /// <summary>
  /// Loads save data and restores the save file chunk tree.
  /// </summary>
  /// <returns>Asynchronous task.</returns>
  ValueTask LoadAsync(CancellationToken cancellationToken = default);

  ValueTask<bool> ExistsAsync(CancellationToken cancellationToken = default);

  ValueTask<bool> DeleteAsync(CancellationToken cancellationToken = default);
}

public static class SaveFile
{
  public static SaveFile<TData> CreateGzipJsonFile<TData>(SaveChunk<TData> root, string filePath, JsonSerializerOptions? options = null) where TData : class => new(
    root: root,
    io: new FileIO(filePath),
    serializer: new JsonStreamSerializer(options),
    compression: new GZipCompression()
  );
}

/// <inheritdoc cref="ISaveFile{TData}"/>
public class SaveFile<TData> : ISaveFile<TData> where TData : class
{
  /// <inheritdoc cref="ISaveFile{TData}.Root"/>
  public ISaveChunk<TData> Root { get; }

#if NET5_0_OR_GREATER
  [System.Diagnostics.CodeAnalysisMemberNotNullWhen(true, nameof(_io), nameof(_serializer))]
#endif
  public bool CanSaveSynchronously => _io is not null && _serializer is not null;

  private static InvalidOperationException SynchronousOperationNotAllowedException()
    => new("Synchronous operation is not allowed because required IO and serializer providers are not available.");

  private readonly IIOStreamProvider? _io;
  private readonly IAsyncIOStreamProvider? _asyncIO;
  private readonly IStreamSerializer? _serializer;
  private readonly IAsyncStreamSerializer? _asyncSerializer;
  private readonly ICompressionStreamProvider? _compression;

  /// <inheritdoc cref="ISaveFile{TData}"/>
  /// <param name="root">
  /// <inheritdoc cref="ISaveFile{TData}.Root" path="/summary" />
  /// </param>
  /// <param name="onSave">Function that saves the data.</param>
  /// <param name="onLoad">Function that loads the data.</param>
  private SaveFile(
    ISaveChunk<TData> root,
    IIOStreamProvider? io,
    IAsyncIOStreamProvider? asyncIO,
    IStreamSerializer? serializer,
    IAsyncStreamSerializer? asyncSerializer,
    ICompressionStreamProvider? compression
  )
  {
    Root = root;
    _io = io;
    _asyncIO = asyncIO;
    _serializer = serializer;
    _asyncSerializer = asyncSerializer;
    _compression = compression;
  }

  public SaveFile(
    ISaveChunk<TData> root,
    IIOStreamProvider io,
    IStreamSerializer serializer,
    ICompressionStreamProvider? compression = null
  ) : this(root, io, io as IAsyncIOStreamProvider, serializer, serializer as IAsyncStreamSerializer, compression)
  { }

  public SaveFile(
    ISaveChunk<TData> root,
    IIOStreamProvider io,
    IAsyncStreamSerializer asyncSerializer,
    ICompressionStreamProvider? compression = null
  ) : this(root, io, io as IAsyncIOStreamProvider, asyncSerializer as IStreamSerializer, asyncSerializer, compression)
  { }

  public SaveFile(
    ISaveChunk<TData> root,
    IAsyncIOStreamProvider asyncIO,
    IStreamSerializer serializer,
    ICompressionStreamProvider? compression = null
  ) : this(root, asyncIO as IIOStreamProvider, asyncIO, serializer, serializer as IAsyncStreamSerializer, compression)
  { }

  public SaveFile(
    ISaveChunk<TData> root,
    IAsyncIOStreamProvider asyncIO,
    IAsyncStreamSerializer asyncSerializer,
    ICompressionStreamProvider? compression = null
  ) : this(root, asyncIO as IIOStreamProvider, asyncIO, asyncSerializer as IStreamSerializer, asyncSerializer, compression)
  { }

  public void Save(CompressionLevel compressionLevel = default)
  {
    if (!CanSaveSynchronously)
    {
      throw SynchronousOperationNotAllowedException();
    }

    using var ioStream = _io.Write();
    using var compressionStream = _compression?.CompressionStream(ioStream, compressionLevel);
    _serializer.Serialize(compressionStream ?? ioStream, Root.GetSaveData());
  }

  public void Load()
  {
    if (!CanSaveSynchronously)
    {
      throw SynchronousOperationNotAllowedException();
    }

    using var ioStream = _io.Read();
    using var decompressionStream = _compression?.DecompressionStream(ioStream);
    var data = _serializer.Deserialize<TData>(decompressionStream ?? ioStream);
    if (data is null)
    {
      return;
    }

    Root.LoadSaveData(data);
  }

  public bool Exists() => _io is not null ? _io.Exists() : throw SynchronousOperationNotAllowedException();

  public void Delete()
  {
    if (_io is null)
    {
      throw SynchronousOperationNotAllowedException();
    }

    _io.Delete();
  }

  public async ValueTask SaveAsync(CompressionLevel compressionLevel = default, CancellationToken cancellationToken = default)
  {
    await using var ioStream = _asyncIO is not null
      ? new MemoryStream()
      : _io!.Write();

    await using var compressionStream = _compression?.CompressionStream(ioStream, compressionLevel);

    if (_asyncSerializer is not null)
    {
      await _asyncSerializer.SerializeAsync(compressionStream ?? ioStream, Root.GetSaveData(), cancellationToken).ConfigureAwait(false);
    }
    else
    {
      _serializer!.Serialize(compressionStream ?? ioStream, Root.GetSaveData());
    }

    if (_asyncIO is not null)
    {
      await _asyncIO.WriteAsync(compressionStream ?? ioStream, cancellationToken).ConfigureAwait(false);
    }
  }

  public async ValueTask LoadAsync(CancellationToken cancellationToken = default)
  {
    await using var ioStream = _asyncIO is not null
      ? await _asyncIO.ReadAsync(cancellationToken).ConfigureAwait(false)
      : _io!.Read();

    await using var decompressionStream = _compression?.DecompressionStream(ioStream);

    var data = _asyncSerializer is not null
      ? await _asyncSerializer.DeserializeAsync<TData>(decompressionStream ?? ioStream, cancellationToken).ConfigureAwait(false)
      : _serializer!.Deserialize<TData>(decompressionStream ?? ioStream);

    if (data is null)
    {
      return;
    }

    Root.LoadSaveData(data);
  }

  public async ValueTask<bool> ExistsAsync(CancellationToken cancellationToken = default)
  {
    if (_asyncIO is not null)
    {
      return await _asyncIO.ExistsAsync(cancellationToken).ConfigureAwait(false);
    }

    return _io!.Exists();
  }

  public async ValueTask<bool> DeleteAsync(CancellationToken cancellationToken = default)
  {
    if (_asyncIO is not null)
    {
      return await _asyncIO.DeleteAsync(cancellationToken).ConfigureAwait(false);
    }

    _io!.Delete();
    return true;
  }
}
