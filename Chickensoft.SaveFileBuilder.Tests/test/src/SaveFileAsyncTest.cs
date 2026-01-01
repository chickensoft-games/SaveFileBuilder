namespace Chickensoft.SaveFileBuilder.Tests;

using System.IO.Compression;
using Chickensoft.SaveFileBuilder.Compression;
using Chickensoft.SaveFileBuilder.IO;
using Chickensoft.SaveFileBuilder.Serialization;

public class SaveFileAsyncTest
{
  private CancellationToken CancellationToken { get; }

  public Mock<IAsyncStreamIO> MockAsyncIO { get; set; }
  public Mock<IAsyncStreamSerializer> MockAsyncSerializer { get; set; }
  public Mock<IStreamCompressor> MockCompresser { get; set; }

  public Mock<ISaveChunk<string>> MockChunk { get; set; }

  public SaveFile<string> SaveFile { get; set; }

  public SaveFileAsyncTest(ITestContextAccessor testContextAccessor)
  {
    CancellationToken = testContextAccessor.Current.CancellationToken;

    MockAsyncIO = new Mock<IAsyncStreamIO>();
    MockAsyncSerializer = new Mock<IAsyncStreamSerializer>();
    MockCompresser = new Mock<IStreamCompressor>();

    MockChunk = new Mock<ISaveChunk<string>>();

    SaveFile = new SaveFile<string>(MockChunk.Object, MockAsyncIO.Object, MockAsyncSerializer.Object, MockCompresser.Object);
  }

  [Fact]
  public void CanSaveSynchronously_IsFalse() => Assert.False(SaveFile.CanSaveSynchronously);

  [Fact]
  public void Save_ThrowsInvalidOperationException() => Assert.Throws<InvalidOperationException>(() => SaveFile.Save());

  [Fact]
  public void Load_ThrowsInvalidOperationException() => Assert.Throws<InvalidOperationException>(SaveFile.Load);

  [Fact]
  public void Exists_ThrowsInvalidOperationException() => Assert.Throws<InvalidOperationException>(() => SaveFile.Exists());

  [Fact]
  public void Delete_ThrowsInvalidOperationException() => Assert.Throws<InvalidOperationException>(SaveFile.Delete);

  [Fact]
  public async Task SaveAsync_WritesCompressesAndSerializes()
  {
    // Arrange
    MemoryStream? ioStream = null;
    var compressionStream = new MemoryStream();

    MockChunk.Setup(chunk => chunk.GetSaveData()).Returns("test").Verifiable();
    MockCompresser.Setup(compresser => compresser.Compress(It.IsAny<MemoryStream>(), It.IsAny<CompressionLevel>(), true)).Callback<Stream, CompressionLevel, bool>((stream, _, _) => ioStream = (MemoryStream)stream).Returns(compressionStream).Verifiable();
    MockAsyncSerializer.Setup(serializer => serializer.SerializeAsync(compressionStream, "test", typeof(string), CancellationToken)).Verifiable();
    MockAsyncIO.Setup(io => io.WriteAsync(It.Is<Stream>(stream => ioStream == stream), CancellationToken)).Verifiable();

    // Act
    await SaveFile.SaveAsync(cancellationToken: CancellationToken);

    // Assert
    MockChunk.Verify();
    MockCompresser.Verify();
    MockAsyncSerializer.Verify();
    MockAsyncIO.Verify();
  }

  [Fact]
  public async Task SaveAsync_CompressorIsNull_WritesAndSerializesWithoutCompressing()
  {
    // Arrange
    SaveFile = new SaveFile<string>(MockChunk.Object, MockAsyncIO.Object, MockAsyncSerializer.Object, null);

    MemoryStream? ioStream = null;
    MockChunk.Setup(chunk => chunk.GetSaveData()).Returns("test").Verifiable();
    MockAsyncSerializer.Setup(serializer => serializer.SerializeAsync(It.IsAny<MemoryStream>(), "test", typeof(string), CancellationToken)).Callback<Stream, object?, Type, CancellationToken>((stream, _, _, _) => ioStream = (MemoryStream)stream).Returns(Task.CompletedTask).Verifiable();
    MockAsyncIO.Setup(io => io.WriteAsync(It.Is<Stream>(stream => ioStream == stream), CancellationToken)).Verifiable();

    // Act
    await SaveFile.SaveAsync(cancellationToken: CancellationToken);

    // Assert
    MockChunk.Verify();
    MockAsyncSerializer.Verify();
    MockAsyncIO.Verify();
  }

  [Fact]
  public async Task SaveAsync_CompressionLevel_UsedByCompressor()
  {
    // Arrange
    MockCompresser.Setup(compressor => compressor.Compress(It.IsAny<Stream>(), CompressionLevel.Fastest, true)).Verifiable();

    // Act
    await SaveFile.SaveAsync(CompressionLevel.Fastest, CancellationToken);

    // Assert
    MockCompresser.Verify();
  }

  [Fact]
  public async Task LoadAsync_ReadsDecompressesAndDeserializes()
  {
    // Arrange
    var ioStream = new MemoryStream();
    var decompressionStream = new MemoryStream();

    MockAsyncIO.Setup(io => io.ReadAsync(CancellationToken)).ReturnsAsync(ioStream).Verifiable();
    MockCompresser.Setup(compresser => compresser.Decompress(ioStream)).Returns(decompressionStream).Verifiable();
    MockAsyncSerializer.Setup(serializer => serializer.DeserializeAsync(decompressionStream, typeof(string), CancellationToken)).ReturnsAsync("test").Verifiable();
    MockChunk.Setup(chunk => chunk.LoadSaveData("test")).Verifiable();

    // Act
    await SaveFile.LoadAsync(CancellationToken);

    // Assert
    MockAsyncIO.Verify();
    MockCompresser.Verify();
    MockAsyncSerializer.Verify();
    MockChunk.Verify();
  }

  [Fact]
  public async Task LoadAsync_CompressorIsNull_ReadsAndDeserializesWithoutDecompressing()
  {
    // Arrange
    SaveFile = new SaveFile<string>(MockChunk.Object, MockAsyncIO.Object, MockAsyncSerializer.Object, null);

    var ioStream = new MemoryStream();
    MockAsyncIO.Setup(io => io.ReadAsync(CancellationToken)).ReturnsAsync(ioStream).Verifiable();
    MockAsyncSerializer.Setup(serializer => serializer.DeserializeAsync(ioStream, typeof(string), CancellationToken)).ReturnsAsync("test").Verifiable();
    MockChunk.Setup(chunk => chunk.LoadSaveData("test")).Verifiable();

    // Act
    await SaveFile.LoadAsync(CancellationToken);

    // Assert
    MockAsyncIO.Verify();
    MockAsyncSerializer.Verify();
    MockChunk.Verify();
  }

  [Fact]
  public async Task LoadAsync_DataIsNull_DoesNotSetChunkData()
  {
    // Arrange
    MockAsyncSerializer.Setup(serializer => serializer.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<Type>(), CancellationToken)).ReturnsAsync((string?)null).Verifiable();
    MockChunk.Setup(chunk => chunk.LoadSaveData(It.IsAny<string>())).Verifiable(Times.Never);

    // Act
    await SaveFile.LoadAsync(CancellationToken);

    // Assert
    MockAsyncSerializer.Verify();
    MockChunk.Verify();
  }

  [Fact]
  public async Task ExistsAsync_ReturnsIOStreamExistsAsyncResult()
  {
    // Arrange
    MockAsyncIO.Setup(io => io.ExistsAsync(CancellationToken)).ReturnsAsync(true).Verifiable();

    // Act
    var result = await SaveFile.ExistsAsync(cancellationToken: CancellationToken);

    // Assert
    MockAsyncIO.Verify();
    Assert.True(result);
  }

  [Fact]
  public async Task DeleteAsync_CallsIOStreamDeleteAsync()
  {
    // Arrange
    MockAsyncIO.Setup(io => io.DeleteAsync(CancellationToken)).ReturnsAsync(true).Verifiable();

    // Act
    var result = await SaveFile.DeleteAsync(cancellationToken: CancellationToken);

    // Assert
    MockAsyncIO.Verify();
    Assert.True(result);
  }
}
