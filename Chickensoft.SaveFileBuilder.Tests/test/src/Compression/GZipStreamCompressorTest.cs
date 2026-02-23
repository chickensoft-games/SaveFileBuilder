namespace Chickensoft.SaveFileBuilder.Tests.Compression;

using System.IO.Compression;
using System.Text;
using Chickensoft.SaveFileBuilder.Compression;

public class GZipStreamCompressorTest
{
  private readonly GZipStreamCompressor _compressor;

  public GZipStreamCompressorTest()
  {
    _compressor = new GZipStreamCompressor();
  }

  [Fact]
  public void Compress_WithDefaultParameters_ReturnsGZipStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream);

    Assert.NotNull(compressedStream);
    Assert.IsType<GZipStream>(compressedStream);
    Assert.True(compressedStream.CanWrite);
  }

  [Fact]
  public void Compress_WithOptimalCompression_ReturnsGZipStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.Optimal);

    Assert.NotNull(compressedStream);
    Assert.IsType<GZipStream>(compressedStream);
  }

  [Fact]
  public void Compress_WithFastestCompression_ReturnsGZipStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.Fastest);

    Assert.NotNull(compressedStream);
    Assert.IsType<GZipStream>(compressedStream);
  }

  [Fact]
  public void Compress_WithSmallestSizeCompression_ReturnsGZipStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.SmallestSize);

    Assert.NotNull(compressedStream);
    Assert.IsType<GZipStream>(compressedStream);
  }

  [Fact]
  public void Compress_WithNoCompression_ReturnsGZipStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.NoCompression);

    Assert.NotNull(compressedStream);
    Assert.IsType<GZipStream>(compressedStream);
  }

  [Fact]
  public void Compress_WithLeaveOpenTrue_KeepsBaseStreamOpen()
  {
    var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, leaveOpen: true);

    compressedStream.Dispose();

    // BaseStream should still be accessible if leaveOpen was true
    Assert.True(baseStream.CanRead);
    baseStream.Dispose();
  }

  [Fact]
  public void Compress_WithLeaveOpenFalse_ClosesBaseStream()
  {
    var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, leaveOpen: false);

    compressedStream.Dispose();

    // BaseStream should be closed if leaveOpen was false
    Assert.False(baseStream.CanRead);
  }

  [Fact]
  public void Compress_WithNullStream_ThrowsArgumentNullException()
  {
    var exception = Record.Exception(() => _compressor.Compress(null!));

    Assert.NotNull(exception);
    Assert.IsType<ArgumentNullException>(exception);
  }

  [Fact]
  public void Compress_WithNonWritableStream_ThrowsArgumentException()
  {
    using var readOnlyStream = new MemoryStream(new byte[10], writable: false);
    var exception = Record.Exception(() => _compressor.Compress(readOnlyStream));

    Assert.NotNull(exception);
    Assert.IsType<ArgumentException>(exception);
  }

  [Fact]
  public void Decompress_WithValidStream_ReturnsGZipStream()
  {
    // Create a compressed stream first
    using var compressedData = new MemoryStream();
    using (var gzipStream = new GZipStream(compressedData, CompressionMode.Compress, leaveOpen: true))
    {
      var data = Encoding.UTF8.GetBytes("test data");
      gzipStream.Write(data, 0, data.Length);
    }
    compressedData.Position = 0;

    var decompressedStream = _compressor.Decompress(compressedData);

    Assert.NotNull(decompressedStream);
    Assert.IsType<GZipStream>(decompressedStream);
    Assert.True(decompressedStream.CanRead);
  }

  [Fact]
  public void Decompress_WithLeaveOpenTrue_KeepsBaseStreamOpen()
  {
    // Create a compressed stream first
    var compressedData = new MemoryStream();
    using (var gzipStream = new GZipStream(compressedData, CompressionMode.Compress, leaveOpen: true))
    {
      var data = Encoding.UTF8.GetBytes("test data");
      gzipStream.Write(data, 0, data.Length);
    }
    compressedData.Position = 0;

    var decompressedStream = _compressor.Decompress(compressedData, leaveOpen: true);
    decompressedStream.Dispose();

    // BaseStream should still be accessible if leaveOpen was true
    Assert.True(compressedData.CanRead);
    compressedData.Dispose();
  }

  [Fact]
  public void Decompress_WithLeaveOpenFalse_ClosesBaseStream()
  {
    // Create a compressed stream first
    var compressedData = new MemoryStream();
    using (var gzipStream = new GZipStream(compressedData, CompressionMode.Compress, leaveOpen: true))
    {
      var data = Encoding.UTF8.GetBytes("test data");
      gzipStream.Write(data, 0, data.Length);
    }
    compressedData.Position = 0;

    var decompressedStream = _compressor.Decompress(compressedData, leaveOpen: false);
    decompressedStream.Dispose();

    // BaseStream should be closed if leaveOpen was false
    Assert.False(compressedData.CanRead);
  }

  [Fact]
  public void Decompress_WithNullStream_ThrowsArgumentNullException()
  {
    var exception = Record.Exception(() => _compressor.Decompress(null!));

    Assert.NotNull(exception);
    Assert.IsType<ArgumentNullException>(exception);
  }

  [Fact]
  public void CompressAndDecompress_RoundTrip_PreservesData()
  {
    var originalData = "This is test data for compression and decompression round trip!";
    var originalBytes = Encoding.UTF8.GetBytes(originalData);

    // Compress
    using var compressedStream = new MemoryStream();
    using (var gzipCompress = _compressor.Compress(compressedStream, leaveOpen: true))
    {
      gzipCompress.Write(originalBytes, 0, originalBytes.Length);
    }

    // Decompress
    compressedStream.Position = 0;
    using var decompressedStream = new MemoryStream();
    using (var gzipDecompress = _compressor.Decompress(compressedStream, leaveOpen: true))
    {
      gzipDecompress.CopyTo(decompressedStream);
    }

    // Verify
    var decompressedData = Encoding.UTF8.GetString(decompressedStream.ToArray());
    Assert.Equal(originalData, decompressedData);
  }

  [Fact]
  public void CompressAndDecompress_WithLargeData_PreservesData()
  {
    // Create a larger test data set
    var originalData = string.Join("", Enumerable.Repeat("Large test data for compression! ", 1000));
    var originalBytes = Encoding.UTF8.GetBytes(originalData);

    // Compress
    using var compressedStream = new MemoryStream();
    using (var gzipCompress = _compressor.Compress(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
    {
      gzipCompress.Write(originalBytes, 0, originalBytes.Length);
    }

    // Verify compression actually occurred
    Assert.True(compressedStream.Length < originalBytes.Length);

    // Decompress
    compressedStream.Position = 0;
    using var decompressedStream = new MemoryStream();
    using (var gzipDecompress = _compressor.Decompress(compressedStream, leaveOpen: true))
    {
      gzipDecompress.CopyTo(decompressedStream);
    }

    // Verify
    var decompressedData = Encoding.UTF8.GetString(decompressedStream.ToArray());
    Assert.Equal(originalData, decompressedData);
    Assert.Equal(originalBytes.Length, decompressedStream.Length);
  }

  [Fact]
  public void CompressAndDecompress_WithEmptyData_PreservesEmptyData()
  {
    var originalBytes = Array.Empty<byte>();

    // Compress
    using var compressedStream = new MemoryStream();
    using (var gzipCompress = _compressor.Compress(compressedStream, leaveOpen: true))
    {
      gzipCompress.Write(originalBytes, 0, originalBytes.Length);
    }

    // Decompress
    compressedStream.Position = 0;
    using var decompressedStream = new MemoryStream();
    using (var gzipDecompress = _compressor.Decompress(compressedStream, leaveOpen: true))
    {
      gzipDecompress.CopyTo(decompressedStream);
    }

    // Verify
    Assert.Empty(decompressedStream.ToArray());
  }

  [Fact]
  public void Compress_WithDifferentCompressionLevels_ProducesDifferentSizes()
  {
    var testData = string.Join("", Enumerable.Repeat("Compression test data! ", 100));
    var testBytes = Encoding.UTF8.GetBytes(testData);

    // Compress with Fastest
    using var fastestStream = new MemoryStream();
    using (var gzipFastest = _compressor.Compress(fastestStream, CompressionLevel.Fastest, leaveOpen: true))
    {
      gzipFastest.Write(testBytes, 0, testBytes.Length);
    }

    // Compress with SmallestSize
    using var smallestStream = new MemoryStream();
    using (var gzipSmallest = _compressor.Compress(smallestStream, CompressionLevel.SmallestSize, leaveOpen: true))
    {
      gzipSmallest.Write(testBytes, 0, testBytes.Length);
    }

    // SmallestSize should produce smaller or equal output than Fastest
    Assert.True(smallestStream.Length <= fastestStream.Length);
  }
}
