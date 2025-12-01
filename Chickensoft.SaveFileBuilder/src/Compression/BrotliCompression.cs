namespace Chickensoft.SaveFileBuilder.Compression;

using System;
using System.IO;
using System.IO.Compression;

/// <summary>Provides a Brotli compression and decompression stream.</summary>
public readonly struct BrotliCompression : ICompressionStreamProvider
{
  /// <inheritdoc />
  /// <exception cref="ArgumentException" />
  public Stream CompressionStream(Stream stream, CompressionLevel compressionLevel = default, bool leaveOpen = default) => new BrotliStream(stream, compressionLevel, leaveOpen);

  /// <inheritdoc />
  public Stream DecompressionStream(Stream stream, bool leaveOpen = default) => new BrotliStream(stream, CompressionMode.Decompress, leaveOpen);
}

