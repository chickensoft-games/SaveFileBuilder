namespace Chickensoft.SaveFileBuilder;

using System;
using System.IO;
using System.IO.Compression;

/// <summary>Provides a compression- and decompression <see cref="Stream"/> based on the base <see cref="Stream"/> that can be written to or -read from.</summary>
public interface ICompressionStreamProvider
{
  /// <summary>Provide a compression stream using the compression level, and optionally leaves the base stream open.</summary>
  /// <param name="stream">The base stream.</param>
  /// <param name="compressionLevel">Compression level whether to emphasize speed or efficiency.</param>
  /// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="stream"/> open after disposing the compression stream; otherwise <see langword="false"/>.</param>
  /// <returns>The compression stream.</returns>
  Stream CompressionStream(Stream stream, CompressionLevel compressionLevel = default, bool leaveOpen = default);

  /// <summary>Provide a decompression stream, and optionally leaves the base stream open.</summary>
  /// <param name="stream">The base stream.</param>
  /// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="stream"/> open after disposing the decompression stream; otherwise <see langword="false"/>.</param>
  /// <returns>The decompressed stream.</returns>
  Stream DecompressionStream(Stream stream, bool leaveOpen = default);
}

/// <summary>Provides a Brotli compression and decompression stream.</summary>
public readonly struct BrotliCompression : ICompressionStreamProvider
{
  /// <inheritdoc />
  /// <exception cref="ArgumentException" />
  public Stream CompressionStream(Stream stream, CompressionLevel compressionLevel = default, bool leaveOpen = default) => new BrotliStream(stream, compressionLevel, leaveOpen);

  /// <inheritdoc />
  public Stream DecompressionStream(Stream stream, bool leaveOpen = default) => new BrotliStream(stream, CompressionMode.Decompress, leaveOpen);
}

/// <summary>Provides a Deflate compression and decompression stream.</summary>
public readonly struct DeflateCompression : ICompressionStreamProvider
{
  /// <inheritdoc />
  /// <exception cref="ArgumentNullException" />
  /// <exception cref="ArgumentException" />
  public Stream CompressionStream(Stream stream, CompressionLevel compressionLevel = default, bool leaveOpen = default) => new DeflateStream(stream, compressionLevel, leaveOpen);

  /// <inheritdoc />
  /// <exception cref="ArgumentNullException" />
  /// <exception cref="ArgumentException" />
  public Stream DecompressionStream(Stream stream, bool leaveOpen = default) => new DeflateStream(stream, CompressionMode.Decompress, leaveOpen);
}

/// <summary>Provides a GZip compression and decompression stream.</summary>
public readonly struct GZipCompression : ICompressionStreamProvider
{
  /// <inheritdoc />
  /// <exception cref="ArgumentNullException" />
  /// <exception cref="ArgumentException" />
  public Stream CompressionStream(Stream stream, CompressionLevel compressionLevel = default, bool leaveOpen = default) => new GZipStream(stream, compressionLevel, leaveOpen);

  /// <inheritdoc />
  /// <exception cref="ArgumentNullException" />
  /// <exception cref="ArgumentException" />
  public Stream DecompressionStream(Stream stream, bool leaveOpen = default) => new GZipStream(stream, CompressionMode.Decompress, leaveOpen);
}

#if NET8_0_OR_GREATER
/// <summary>Provides a ZLib compression and decompression stream.</summary>
public readonly struct ZLibCompression : ICompressionStreamProvider
{
  /// <inheritdoc />
  public Stream CompressionStream(Stream stream, CompressionLevel compressionLevel = default, bool leaveOpen = default) => new ZLibStream(stream, compressionLevel, leaveOpen);

  /// <inheritdoc />
  public Stream DecompressionStream(Stream stream, bool leaveOpen = default) => new ZLibStream(stream, CompressionMode.Decompress, leaveOpen);
}
#endif
