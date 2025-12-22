namespace Chickensoft.SaveFileBuilder.Compression;

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
