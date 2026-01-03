namespace Chickensoft.SaveFileBuilder;

using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Chickensoft.SaveFileBuilder.Compression;
using Chickensoft.SaveFileBuilder.IO;
using Chickensoft.SaveFileBuilder.Serialization;

/// <summary>Represents a save file composed of one or more save chunks.</summary>
/// <typeparam name="TData">Type of data represented by the save file.</typeparam>
public interface ISaveFile<TData> where TData : class
{
  /// <summary>Root save chunk from which the save file contents are composed.</summary>
  ISaveChunk<TData> Root { get; }

  /// <summary>Gets a value indicating whether the content can be saved using a synchronous operation.</summary>
  /// <remarks>If <see langword="false"/>, attempts to save synchronously may not be supported and could result in an exception or undefined behavior. Check this property before invoking synchronous save methods to ensure compatibility.</remarks>
  bool CanSaveSynchronously { get; }

  /// <returns></returns>
  /// <inheritdoc cref="SaveAsync(CompressionLevel, CancellationToken)" />
  void Save(CompressionLevel compressionLevel = default);

  /// <returns></returns>
  /// <inheritdoc cref="LoadAsync(CancellationToken)" />
  void Load();

  /// <returns><see langword="true"/> if the save file exists; otherwise, <see langword="false"/>.</returns>
  /// <inheritdoc cref="ExistsAsync(CancellationToken)" />
  bool Exists();

  /// <returns></returns>
  /// <inheritdoc cref="DeleteAsync(CancellationToken)" />
  void Delete();

  /// <summary>Collects save data from the <see cref="Root"/> chunk tree and saves it.</summary>
  /// <param name="compressionLevel">Compression level whether to emphasize speed or efficiency when compressing.</param>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous save operation.</param>
  /// <returns>A task that represents the asynchronous save operation.</returns>
  ValueTask SaveAsync(CompressionLevel compressionLevel = default, CancellationToken cancellationToken = default);

  /// <summary>Loads save data and restores the <see cref="Root"/> chunk tree.</summary>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous load operation.</param>
  /// <returns>A task that represents the asynchronous load operation.</returns>
  ValueTask LoadAsync(CancellationToken cancellationToken = default);

  /// <summary>Determines whether the save file exists.</summary>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous exists operation.</param>
  /// <returns>A task that represents the asynchronous exists operation. The value of the task is <see langword="true"/> if the save file exists; otherwise, <see langword="false"/>.</returns>
  ValueTask<bool> ExistsAsync(CancellationToken cancellationToken = default);

  /// <summary>Deletes the save file.</summary>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous delete operation.</param>
  /// <returns>A task that represents the asynchronous delete operation. The value of the task is <see langword="true"/> if the io source was deleted; otherwise, <see langword="false"/>.</returns>
  ValueTask<bool> DeleteAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="ISaveFile{TData}"/>
public class SaveFile<TData> : ISaveFile<TData> where TData : class
{
  /// <inheritdoc cref="ISaveFile{TData}.Root"/>
  public ISaveChunk<TData> Root { get; }

  /// <inheritdoc />
#if NET5_0_OR_GREATER
  [System.Diagnostics.CodeAnalysisMemberNotNullWhen(true, nameof(_io), nameof(_serializer))]
#endif
  public bool CanSaveSynchronously => _io is not null && _serializer is not null;

  private static InvalidOperationException SynchronousOperationNotAllowedException()
    => new($"Synchronous operation is not allowed because either the {nameof(IStreamIO)} or the {nameof(IStreamSerializer)} of the {nameof(SaveFile)} is null.");

  private readonly IStreamIO? _io;
  private readonly IAsyncStreamIO? _asyncIO;
  private readonly IStreamSerializer? _serializer;
  private readonly IAsyncStreamSerializer? _asyncSerializer;
  private readonly IStreamCompressor? _compressor;

  /// <inheritdoc cref="ISaveFile{TData}"/>
  /// <param name="root"><inheritdoc cref="ISaveFile{TData}.Root" path="/summary" /></param>
  /// <param name="io">Input/output source which the save file reads from and writes to.</param>
  /// <param name="asyncIO">Input/output source which the save file reads from and writes to asynchronously.</param>
  /// <param name="serializer">Serializer which the save file uses to serialize and deserialize data.</param>
  /// <param name="asyncSerializer">Serializer which the save file uses to serialize and deserialize data asynchronously.</param>
  /// <param name="compressor">Compressor which the save file uses to compress and decompress data.</param>
  private SaveFile(
    ISaveChunk<TData> root,
    IStreamIO? io,
    IAsyncStreamIO? asyncIO,
    IStreamSerializer? serializer,
    IAsyncStreamSerializer? asyncSerializer,
    IStreamCompressor? compressor
  )
  {
    Root = root;
    _io = io;
    _asyncIO = asyncIO;
    _serializer = serializer;
    _asyncSerializer = asyncSerializer;
    _compressor = compressor;
  }

  /// <inheritdoc cref="SaveFile{TData}.SaveFile(ISaveChunk{TData}, IStreamIO?, IAsyncStreamIO?, IStreamSerializer?, IAsyncStreamSerializer?, IStreamCompressor?)" />
  public SaveFile(
    ISaveChunk<TData> root,
    IStreamIO io,
    IStreamSerializer serializer,
    IStreamCompressor? compressor = null
  ) : this(root, io, io as IAsyncStreamIO, serializer, serializer as IAsyncStreamSerializer, compressor)
  { }

  /// <inheritdoc cref="SaveFile{TData}.SaveFile(ISaveChunk{TData}, IStreamIO?, IAsyncStreamIO?, IStreamSerializer?, IAsyncStreamSerializer?, IStreamCompressor?)" />
  public SaveFile(
    ISaveChunk<TData> root,
    IStreamIO io,
    IAsyncStreamSerializer asyncSerializer,
    IStreamCompressor? compressor = null
  ) : this(root, io, io as IAsyncStreamIO, asyncSerializer as IStreamSerializer, asyncSerializer, compressor)
  { }

  /// <inheritdoc cref="SaveFile{TData}.SaveFile(ISaveChunk{TData}, IStreamIO?, IAsyncStreamIO?, IStreamSerializer?, IAsyncStreamSerializer?, IStreamCompressor?)" />
  public SaveFile(
    ISaveChunk<TData> root,
    IAsyncStreamIO asyncIO,
    IStreamSerializer serializer,
    IStreamCompressor? compressor = null
  ) : this(root, asyncIO as IStreamIO, asyncIO, serializer, serializer as IAsyncStreamSerializer, compressor)
  { }

  /// <inheritdoc cref="SaveFile{TData}.SaveFile(ISaveChunk{TData}, IStreamIO?, IAsyncStreamIO?, IStreamSerializer?, IAsyncStreamSerializer?, IStreamCompressor?)" />
  public SaveFile(
    ISaveChunk<TData> root,
    IAsyncStreamIO asyncIO,
    IAsyncStreamSerializer asyncSerializer,
    IStreamCompressor? compressor = null
  ) : this(root, asyncIO as IStreamIO, asyncIO, asyncSerializer as IStreamSerializer, asyncSerializer, compressor)
  { }

  /// <inheritdoc />
  public void Save(CompressionLevel compressionLevel = default)
  {
    if (!CanSaveSynchronously)
    {
      throw SynchronousOperationNotAllowedException();
    }

    using var ioStream = _io!.Write();
    using var compressionStream = _compressor?.Compress(ioStream, compressionLevel);
    _serializer!.Serialize(compressionStream ?? ioStream, Root.GetSaveData());
  }

  /// <inheritdoc />
  public void Load()
  {
    if (!CanSaveSynchronously)
    {
      throw SynchronousOperationNotAllowedException();
    }

    using var ioStream = _io!.Read();
    using var decompressionStream = _compressor?.Decompress(ioStream);
    var data = _serializer!.Deserialize<TData>(decompressionStream ?? ioStream);
    if (data is null)
    {
      return;
    }

    Root.LoadSaveData(data);
  }

  /// <inheritdoc />
  public bool Exists() => _io is not null ? _io.Exists() : throw SynchronousOperationNotAllowedException();

  /// <inheritdoc />
  public void Delete()
  {
    if (_io is null)
    {
      throw SynchronousOperationNotAllowedException();
    }

    _io.Delete();
  }

  /// <inheritdoc />
  public async ValueTask SaveAsync(CompressionLevel compressionLevel = default, CancellationToken cancellationToken = default)
  {
    if (_asyncIO is null)
    {
      await using var ioStream = _io!.Write();
      await using var compressionStream = _compressor?.Compress(ioStream, compressionLevel);
      await serialize(compressionStream ?? ioStream);
    }
    else
    {
      await using var writeStream = new MemoryStream();
      await using (var compressionStream = _compressor?.Compress(writeStream, compressionLevel, true))
      {
        await serialize(compressionStream ?? writeStream);
      }
      writeStream.Position = 0;

      await _asyncIO.WriteAsync(writeStream, cancellationToken);
    }

    async Task serialize(Stream stream)
    {
      if (_asyncSerializer is not null)
      {
        await _asyncSerializer.SerializeAsync(stream, Root.GetSaveData(), cancellationToken);
      }
      else
      {
        _serializer!.Serialize(stream, Root.GetSaveData());
      }
    }
  }

  /// <inheritdoc />
  public async ValueTask LoadAsync(CancellationToken cancellationToken = default)
  {
    await using var ioStream = _asyncIO is not null
      ? await _asyncIO.ReadAsync(cancellationToken)
      : _io!.Read();

    await using var decompressionStream = _compressor?.Decompress(ioStream);

    var data = _asyncSerializer is not null
      ? await _asyncSerializer.DeserializeAsync<TData>(decompressionStream ?? ioStream, cancellationToken)
      : _serializer!.Deserialize<TData>(decompressionStream ?? ioStream);

    if (data is null)
    {
      return;
    }

    Root.LoadSaveData(data);
  }

  /// <inheritdoc />
  public async ValueTask<bool> ExistsAsync(CancellationToken cancellationToken = default)
  {
    if (_asyncIO is not null)
    {
      return await _asyncIO.ExistsAsync(cancellationToken);
    }

    return _io!.Exists();
  }

  /// <inheritdoc />
  public async ValueTask<bool> DeleteAsync(CancellationToken cancellationToken = default)
  {
    if (_asyncIO is not null)
    {
      return await _asyncIO.DeleteAsync(cancellationToken);
    }

    _io!.Delete();
    return true;
  }
}

/// <summary>Provides factory methods for creating common save file configurations.</summary>
public static class SaveFile
{
  /// <summary>Creates a new <see cref="SaveFile{TData}"/> that uses JSON serialization and GZip compression.</summary>
  public static SaveFile<TData> CreateGZipJsonFile<TData>(ISaveChunk<TData> root, string filePath, JsonSerializerOptions? options = null) where TData : class => new(
    root: root,
    io: new FileStreamIO(filePath),
    serializer: new JsonStreamSerializer(options),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonFile{TData}(ISaveChunk{TData}, string, JsonSerializerOptions?)" />
  public static SaveFile<TData> CreateGZipJsonFile<TData>(ISaveChunk<TData> root, string filePath, JsonSerializerContext context) where TData : class => new(
    root: root,
    io: new FileStreamIO(filePath),
    serializer: new JsonStreamSerializer(context),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonFile{TData}(ISaveChunk{TData}, string, JsonSerializerOptions?)" />
  public static SaveFile<TData> CreateGZipJsonFile<TData>(ISaveChunk<TData> root, string filePath, JsonTypeInfo jsonTypeInfo) where TData : class => new(
    root: root,
    io: new FileStreamIO(filePath),
    serializer: new JsonStreamSerializer(jsonTypeInfo),
    compressor: new GZipStreamCompressor()
  );

  /// <summary>Creates a new <see cref="SaveFile{TData}"/> that uses the specified io, JSON serialization and GZip compression.</summary>
  public static SaveFile<TData> CreateGZipJsonFile<TData>(ISaveChunk<TData> root, IStreamIO io, JsonSerializerOptions? options = null) where TData : class => new(
    root: root,
    io: io,
    serializer: new JsonStreamSerializer(options),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonFile{TData}(ISaveChunk{TData}, IStreamIO, JsonSerializerOptions?)" />
  public static SaveFile<TData> CreateGZipJsonFile<TData>(ISaveChunk<TData> root, IStreamIO io, JsonSerializerContext context) where TData : class => new(
    root: root,
    io: io,
    serializer: new JsonStreamSerializer(context),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonFile{TData}(ISaveChunk{TData}, IStreamIO, JsonSerializerOptions?)" />
  public static SaveFile<TData> CreateGZipJsonFile<TData>(ISaveChunk<TData> root, IStreamIO io, JsonTypeInfo jsonTypeInfo) where TData : class => new(
    root: root,
    io: io,
    serializer: new JsonStreamSerializer(jsonTypeInfo),
    compressor: new GZipStreamCompressor()
  );

  /// <summary>Creates a new <see cref="SaveFile{TData}"/> that uses the specified io, JSON serialization and GZip compression.</summary>
  public static SaveFile<TData> CreateGZipJsonFile<TData>(ISaveChunk<TData> root, IAsyncStreamIO asyncIO, JsonSerializerOptions? options = null) where TData : class => new(
    root: root,
    asyncIO: asyncIO,
    serializer: new JsonStreamSerializer(options),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonFile{TData}(ISaveChunk{TData}, IAsyncStreamIO, JsonSerializerOptions?)" />
  public static SaveFile<TData> CreateGZipJsonFile<TData>(ISaveChunk<TData> root, IAsyncStreamIO asyncIO, JsonSerializerContext context) where TData : class => new(
    root: root,
    asyncIO: asyncIO,
    serializer: new JsonStreamSerializer(context),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonFile{TData}(ISaveChunk{TData}, IAsyncStreamIO, JsonSerializerOptions?)" />
  public static SaveFile<TData> CreateGZipJsonFile<TData>(ISaveChunk<TData> root, IAsyncStreamIO asyncIO, JsonTypeInfo jsonTypeInfo) where TData : class => new(
    root: root,
    asyncIO: asyncIO,
    serializer: new JsonStreamSerializer(jsonTypeInfo),
    compressor: new GZipStreamCompressor()
  );
}
