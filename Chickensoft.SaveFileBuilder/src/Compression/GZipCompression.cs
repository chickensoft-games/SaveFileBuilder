namespace Chickensoft.SaveFileBuilder.Compression;

using System;
using System.IO;
using System.IO.Compression;

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
