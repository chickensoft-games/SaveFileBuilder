namespace Chickensoft.SaveFileBuilder.Tests.Compression;

using System.IO.Compression;
using System.Text;
using Chickensoft.SaveFileBuilder.Compression;

public class DeflateStreamCompressorTest
{
  private readonly DeflateStreamCompressor _compressor;

  public DeflateStreamCompressorTest()
  {
    _compressor = new DeflateStreamCompressor();
  }

  [Fact]
  public void Compress_WithDefaultParameters_ReturnsDeflateStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream);

    Assert.NotNull(compressedStream);
    Assert.IsType<DeflateStream>(compressedStream);
    Assert.True(compressedStream.CanWrite);
  }

  [Fact]
  public void Compress_WithOptimalCompression_ReturnsDeflateStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.Optimal);

    Assert.NotNull(compressedStream);
    Assert.IsType<DeflateStream>(compressedStream);
  }

  [Fact]
  public void Compress_WithFastestCompression_ReturnsDeflateStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.Fastest);

    Assert.NotNull(compressedStream);
    Assert.IsType<DeflateStream>(compressedStream);
  }

  [Fact]
  public void Compress_WithSmallestSizeCompression_ReturnsDeflateStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.SmallestSize);

    Assert.NotNull(compressedStream);
    Assert.IsType<DeflateStream>(compressedStream);
  }

  [Fact]
  public void Compress_WithNoCompression_ReturnsDeflateStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.NoCompression);

    Assert.NotNull(compressedStream);
    Assert.IsType<DeflateStream>(compressedStream);
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
  public void Decompress_WithValidStream_ReturnsDeflateStream()
  {
    // Create a compressed stream first
    using var compressedData = new MemoryStream();
    using (var deflateStream = new DeflateStream(compressedData, CompressionMode.Compress, leaveOpen: true))
    {
      var data = Encoding.UTF8.GetBytes("test data");
      deflateStream.Write(data, 0, data.Length);
    }
    compressedData.Position = 0;

    var decompressedStream = _compressor.Decompress(compressedData);

    Assert.NotNull(decompressedStream);
    Assert.IsType<DeflateStream>(decompressedStream);
    Assert.True(decompressedStream.CanRead);
  }

  [Fact]
  public void Decompress_WithLeaveOpenTrue_KeepsBaseStreamOpen()
  {
    // Create a compressed stream first
    var compressedData = new MemoryStream();
    using (var deflateStream = new DeflateStream(compressedData, CompressionMode.Compress, leaveOpen: true))
    {
      var data = Encoding.UTF8.GetBytes("test data");
      deflateStream.Write(data, 0, data.Length);
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
    using (var deflateStream = new DeflateStream(compressedData, CompressionMode.Compress, leaveOpen: true))
    {
      var data = Encoding.UTF8.GetBytes("test data");
      deflateStream.Write(data, 0, data.Length);
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
    using (var deflateCompress = _compressor.Compress(compressedStream, leaveOpen: true))
    {
      deflateCompress.Write(originalBytes, 0, originalBytes.Length);
    }

    // Decompress
    compressedStream.Position = 0;
    using var decompressedStream = new MemoryStream();
    using (var deflateDecompress = _compressor.Decompress(compressedStream, leaveOpen: true))
    {
      deflateDecompress.CopyTo(decompressedStream);
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
    using (var deflateCompress = _compressor.Compress(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
    {
      deflateCompress.Write(originalBytes, 0, originalBytes.Length);
    }

    // Verify compression actually occurred
    Assert.True(compressedStream.Length < originalBytes.Length);

    // Decompress
    compressedStream.Position = 0;
    using var decompressedStream = new MemoryStream();
    using (var deflateDecompress = _compressor.Decompress(compressedStream, leaveOpen: true))
    {
      deflateDecompress.CopyTo(decompressedStream);
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
    using (var deflateCompress = _compressor.Compress(compressedStream, leaveOpen: true))
    {
      deflateCompress.Write(originalBytes, 0, originalBytes.Length);
    }

    // Decompress
    compressedStream.Position = 0;
    using var decompressedStream = new MemoryStream();
    using (var deflateDecompress = _compressor.Decompress(compressedStream, leaveOpen: true))
    {
      deflateDecompress.CopyTo(decompressedStream);
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
    using (var deflateFastest = _compressor.Compress(fastestStream, CompressionLevel.Fastest, leaveOpen: true))
    {
      deflateFastest.Write(testBytes, 0, testBytes.Length);
    }

    // Compress with SmallestSize
    using var smallestStream = new MemoryStream();
    using (var deflateSmallest = _compressor.Compress(smallestStream, CompressionLevel.SmallestSize, leaveOpen: true))
    {
      deflateSmallest.Write(testBytes, 0, testBytes.Length);
    }

    // SmallestSize should produce smaller or equal output than Fastest
    Assert.True(smallestStream.Length <= fastestStream.Length);
  }
}
