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

    compressedStream.ShouldNotBeNull();
    compressedStream.ShouldBeOfType<GZipStream>();
    compressedStream.CanWrite.ShouldBeTrue();
  }

  [Fact]
  public void Compress_WithOptimalCompression_ReturnsGZipStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.Optimal);

    compressedStream.ShouldNotBeNull();
    compressedStream.ShouldBeOfType<GZipStream>();
    compressedStream.CanWrite.ShouldBeTrue();
  }

  [Fact]
  public void Compress_WithFastestCompression_ReturnsGZipStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.Fastest);

    compressedStream.ShouldNotBeNull();
    compressedStream.ShouldBeOfType<GZipStream>();
    compressedStream.CanWrite.ShouldBeTrue();
  }

  [Fact]
  public void Compress_WithSmallestSizeCompression_ReturnsGZipStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.SmallestSize);

    compressedStream.ShouldNotBeNull();
    compressedStream.ShouldBeOfType<GZipStream>();
    compressedStream.CanWrite.ShouldBeTrue();
  }

  [Fact]
  public void Compress_WithNoCompression_ReturnsGZipStream()
  {
    using var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, CompressionLevel.NoCompression);

    compressedStream.ShouldNotBeNull();
    compressedStream.ShouldBeOfType<GZipStream>();
    compressedStream.CanWrite.ShouldBeTrue();
  }

  [Fact]
  public void Compress_WithLeaveOpenTrue_KeepsBaseStreamOpen()
  {
    var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, leaveOpen: true);

    compressedStream.Dispose();

    // BaseStream should still be accessible if leaveOpen was true
    baseStream.CanRead.ShouldBeTrue();
    baseStream.Dispose();
  }

  [Fact]
  public void Compress_WithLeaveOpenFalse_ClosesBaseStream()
  {
    var baseStream = new MemoryStream();
    var compressedStream = _compressor.Compress(baseStream, leaveOpen: false);

    compressedStream.Dispose();

    // BaseStream should be closed if leaveOpen was false
    baseStream.CanRead.ShouldBeFalse();
  }

  [Fact]
  public void Compress_WithNullStream_ThrowsArgumentNullException()
  {
    var exception = Record.Exception(() => _compressor.Compress(null!));

    exception.ShouldNotBeNull();
    exception.ShouldBeOfType<ArgumentNullException>();
  }

  [Fact]
  public void Compress_WithNonWritableStream_ThrowsArgumentException()
  {
    using var readOnlyStream = new MemoryStream(new byte[10], writable: false);
    var exception = Record.Exception(() => _compressor.Compress(readOnlyStream));

    exception.ShouldNotBeNull();
    exception.ShouldBeOfType<ArgumentException>();
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

    decompressedStream.ShouldNotBeNull();
    decompressedStream.ShouldBeOfType<GZipStream>();
    decompressedStream.CanRead.ShouldBeTrue();
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
    compressedData.CanRead.ShouldBeTrue();
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
    compressedData.CanRead.ShouldBeFalse();
  }

  [Fact]
  public void Decompress_WithNullStream_ThrowsArgumentNullException()
  {
    var exception = Record.Exception(() => _compressor.Decompress(null!));

    exception.ShouldNotBeNull();
    exception.ShouldBeOfType<ArgumentNullException>();
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
    decompressedData.ShouldBe(originalData);
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
    compressedStream.Length.ShouldBeLessThan(originalBytes.Length);

    // Decompress
    compressedStream.Position = 0;
    using var decompressedStream = new MemoryStream();
    using (var gzipDecompress = _compressor.Decompress(compressedStream, leaveOpen: true))
    {
      gzipDecompress.CopyTo(decompressedStream);
    }

    // Verify
    var decompressedData = Encoding.UTF8.GetString(decompressedStream.ToArray());
    decompressedData.ShouldBe(originalData);
    decompressedStream.Length.ShouldBe(originalBytes.Length);
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
    decompressedStream.ToArray().ShouldBeEmpty();
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
    smallestStream.Length.ShouldBeLessThanOrEqualTo(fastestStream.Length);
  }
}
