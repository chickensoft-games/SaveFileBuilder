namespace Chickensoft.SaveFileBuilder.Compression;

using System;
using System.IO;
using System.IO.Compression;

/// <summary>Provides a Deflate compression and decompression stream.</summary>
public readonly struct DeflateStreamCompressor : IStreamCompressor
{
  /// <inheritdoc />
  /// <exception cref="ArgumentNullException" />
  /// <exception cref="ArgumentException" />
  public Stream Compress(Stream stream, CompressionLevel compressionLevel = default, bool leaveOpen = default) => new DeflateStream(stream, compressionLevel, leaveOpen);

  /// <inheritdoc />
  /// <exception cref="ArgumentNullException" />
  /// <exception cref="ArgumentException" />
  public Stream Decompress(Stream stream, bool leaveOpen = default) => new DeflateStream(stream, CompressionMode.Decompress, leaveOpen);
}
