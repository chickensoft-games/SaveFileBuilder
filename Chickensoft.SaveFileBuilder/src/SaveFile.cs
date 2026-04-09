namespace Chickensoft.SaveFileBuilder;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chickensoft.SaveFileBuilder.Compression;
using Chickensoft.SaveFileBuilder.IO;
using Chickensoft.SaveFileBuilder.Serialization;

/// <summary>Represents a save file that manages input / output, serialization and compression.</summary>
public interface ISaveFile
{
  /// <summary>Gets a value indicating whether file can save / load using a synchronous operation.</summary>
  /// <remarks>If <see langword="false"/>, attempts to save / load synchronously may not be supported and could result in an exception or undefined behavior. Check this property before invoking synchronous save / load methods to ensure compatibility.</remarks>
  bool CanSaveSynchronously { get; }

  /// <inheritdoc cref="SaveAsync{TData}(TData, CompressionLevel, CancellationToken)" />
  void Save<TData>(TData data, CompressionLevel compressionLevel = default);

  /// <inheritdoc cref="LoadAsync{TData}(CancellationToken)" />
  TData? Load<TData>();

  /// <inheritdoc cref="ExistsAsync(CancellationToken)" />
  bool Exists();

  /// <inheritdoc cref="DeleteAsync(CancellationToken)" />
  void Delete();

  /// <summary>Saves data to the save file.</summary>
  /// <typeparam name="TData">The type of the data to save. This can be any type that the serializer can handle.</typeparam>
  /// <param name="data">The data to save.</param>
  /// <param name="compressionLevel">Compression level whether to emphasize speed or efficiency when compressing.</param>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous save operation.</param>
  ValueTask SaveAsync<TData>(TData data, CompressionLevel compressionLevel = default, CancellationToken cancellationToken = default);

  /// <summary>Loads data from the save file.</summary>
  /// <typeparam name="TData">The type of the data to load. This can be any type that the serializer can handle.</typeparam>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous load operation.</param>
  /// <returns>The loaded data, or <see langword="null"/> if the save file does not exist or deserialization fails.</returns>
  ValueTask<TData?> LoadAsync<TData>(CancellationToken cancellationToken = default);

  /// <summary>Determines whether the save file exists.</summary>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous exists operation.</param>
  /// <returns><see langword="true"/> if the save file exists; otherwise, <see langword="false"/>.</returns>
  ValueTask<bool> ExistsAsync(CancellationToken cancellationToken = default);

  /// <summary>Deletes the save file.</summary>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous delete operation.</param>
  /// <returns><see langword="true"/> if the save file was deleted; otherwise, <see langword="false"/>.</returns>
  ValueTask<bool> DeleteAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="ISaveFile"/>
public partial class SaveFile : ISaveFile
{
  internal static class Suppressions
  {
    public static class IDE0370
    {
      public const string CATEGORY = "Style";
      public const string CHECK_ID = "IDE0370:Remove unnecessary suppression";
      public const string JUSTIFICATION = "Suppression required for compatibility with .net standard 2.1.";
    }
  }

  /// <inheritdoc />
#if NET5_0_OR_GREATER
  [MemberNotNullWhen(true, nameof(_io), nameof(_serializer))]
#endif
  public bool CanSaveSynchronously => _io is not null && _serializer is not null;

  private static InvalidOperationException SynchronousOperationNotAllowedException()
    => new($"Synchronous operation is not allowed because either the {nameof(IStreamIO)} or the {nameof(IStreamSerializer)} of the {nameof(SaveFile)} is null.");

  private readonly IStreamIO? _io;
  private readonly IAsyncStreamIO? _asyncIO;
  private readonly IStreamSerializer? _serializer;
  private readonly IAsyncStreamSerializer? _asyncSerializer;
  private readonly IStreamCompressor? _compressor;

  /// <inheritdoc cref="ISaveFile"/>
  /// <param name="io">Input/output source which the save file reads from and writes to.</param>
  /// <param name="asyncIO">Input/output source which the save file reads from and writes to asynchronously.</param>
  /// <param name="serializer">Serializer which the save file uses to serialize and deserialize data.</param>
  /// <param name="asyncSerializer">Serializer which the save file uses to serialize and deserialize data asynchronously.</param>
  /// <param name="compressor">Compressor which the save file uses to compress and decompress data.</param>
  private SaveFile(
    IStreamIO? io,
    IAsyncStreamIO? asyncIO,
    IStreamSerializer? serializer,
    IAsyncStreamSerializer? asyncSerializer,
    IStreamCompressor? compressor
  )
  {
    _io = io;
    _asyncIO = asyncIO;
    _serializer = serializer;
    _asyncSerializer = asyncSerializer;
    _compressor = compressor;
  }

  /// <inheritdoc cref="SaveFile(IStreamIO?, IAsyncStreamIO?, IStreamSerializer?, IAsyncStreamSerializer?, IStreamCompressor?)" />
  public SaveFile(
    IStreamIO io,
    IStreamSerializer serializer,
    IStreamCompressor? compressor = null
  ) : this(io, io as IAsyncStreamIO, serializer, serializer as IAsyncStreamSerializer, compressor)
  { }

  /// <inheritdoc cref="SaveFile(IStreamIO?, IAsyncStreamIO?, IStreamSerializer?, IAsyncStreamSerializer?, IStreamCompressor?)" />
  public SaveFile(
    IStreamIO io,
    IAsyncStreamSerializer asyncSerializer,
    IStreamCompressor? compressor = null
  ) : this(io, io as IAsyncStreamIO, asyncSerializer as IStreamSerializer, asyncSerializer, compressor)
  { }

  /// <inheritdoc cref="SaveFile(IStreamIO?, IAsyncStreamIO?, IStreamSerializer?, IAsyncStreamSerializer?, IStreamCompressor?)" />
  public SaveFile(
    IAsyncStreamIO asyncIO,
    IStreamSerializer serializer,
    IStreamCompressor? compressor = null
  ) : this(asyncIO as IStreamIO, asyncIO, serializer, serializer as IAsyncStreamSerializer, compressor)
  { }

  /// <inheritdoc cref="SaveFile(IStreamIO?, IAsyncStreamIO?, IStreamSerializer?, IAsyncStreamSerializer?, IStreamCompressor?)" />
  public SaveFile(
    IAsyncStreamIO asyncIO,
    IAsyncStreamSerializer asyncSerializer,
    IStreamCompressor? compressor = null
  ) : this(asyncIO as IStreamIO, asyncIO, asyncSerializer as IStreamSerializer, asyncSerializer, compressor)
  { }

  /// <inheritdoc />
  [SuppressMessage(Suppressions.IDE0370.CATEGORY, Suppressions.IDE0370.CHECK_ID, Justification = Suppressions.IDE0370.JUSTIFICATION)]
  public void Save<TData>(TData data, CompressionLevel compressionLevel = default)
  {
    if (!CanSaveSynchronously)
    {
      throw SynchronousOperationNotAllowedException();
    }

    using var ioStream = _io!.Write();
    using var compressionStream = _compressor?.Compress(ioStream, compressionLevel);
    _serializer!.Serialize(compressionStream ?? ioStream, data);
  }

  /// <inheritdoc />
  [SuppressMessage(Suppressions.IDE0370.CATEGORY, Suppressions.IDE0370.CHECK_ID, Justification = Suppressions.IDE0370.JUSTIFICATION)]
  public TData? Load<TData>()
  {
    if (!CanSaveSynchronously)
    {
      throw SynchronousOperationNotAllowedException();
    }

    using var ioStream = _io!.Read();
    using var decompressionStream = _compressor?.Decompress(ioStream);
    return _serializer!.Deserialize<TData>(decompressionStream ?? ioStream);
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
  public async ValueTask SaveAsync<TData>(TData data, CompressionLevel compressionLevel = default, CancellationToken cancellationToken = default)
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
        await _asyncSerializer.SerializeAsync(stream, data, cancellationToken);
      }
      else
      {
        _serializer!.Serialize(stream, data);
      }
    }
  }

  /// <inheritdoc />
  public async ValueTask<TData?> LoadAsync<TData>(CancellationToken cancellationToken = default)
  {
    await using var ioStream = _asyncIO is not null
      ? await _asyncIO.ReadAsync(cancellationToken)
      : _io!.Read();

    await using var decompressionStream = _compressor?.Decompress(ioStream);

    return _asyncSerializer is not null
      ? await _asyncSerializer.DeserializeAsync<TData>(decompressionStream ?? ioStream, cancellationToken)
      : _serializer!.Deserialize<TData>(decompressionStream ?? ioStream);
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
