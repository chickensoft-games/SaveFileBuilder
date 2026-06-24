namespace Chickensoft.SaveFileBuilder.Compression;

using System;
using System.IO;
using System.IO.Compression;

/// <summary>Provides a Brotli compression and decompression stream.</summary>
public readonly struct BrotliStreamCompressor : IStreamCompressor
{
  /// <inheritdoc />
  /// <exception cref="ArgumentException" />
  public Stream Compress(Stream stream, CompressionLevel compressionLevel = default, bool leaveOpen = default) => new BrotliStream(stream, compressionLevel, leaveOpen);

  /// <inheritdoc />
  public Stream Decompress(Stream stream, bool leaveOpen = default) => new BrotliStream(stream, CompressionMode.Decompress, leaveOpen);
}

