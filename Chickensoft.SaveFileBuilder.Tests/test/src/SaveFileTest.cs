namespace Chickensoft.SaveFileBuilder.Tests;

using System.IO.Compression;
using Chickensoft.SaveFileBuilder.Compression;
using Chickensoft.SaveFileBuilder.IO;
using Chickensoft.SaveFileBuilder.Serialization;

public class SaveFileTest
{
  public Mock<IStreamIO> MockIO { get; set; }
  public Mock<IStreamSerializer> MockSerializer { get; set; }
  public Mock<IStreamCompressor> MockCompressor { get; set; }

  public SaveFile SaveFile { get; set; }

  public SaveFileTest()
  {
    MockIO = new Mock<IStreamIO>();
    MockSerializer = new Mock<IStreamSerializer>();
    MockCompressor = new Mock<IStreamCompressor>();

    SaveFile = new SaveFile(MockIO.Object, MockSerializer.Object, MockCompressor.Object);
  }

  [Fact]
  public void CanSaveSynchronously_IsTrue() => Assert.True(SaveFile.CanSaveSynchronously);

  [Fact]
  public void Save_WritesCompressesAndSerializes()
  {
    // Arrange
    var io = new MemoryStream();
    var compressionStream = new MemoryStream();

    MockIO.Setup(io => io.Write()).Returns(io).Verifiable();
    MockCompressor.Setup(compressor => compressor.Compress(io, default, false)).Returns(compressionStream).Verifiable();
    MockSerializer.Setup(serializer => serializer.Serialize(compressionStream, "test", typeof(string))).Verifiable();

    // Act
    SaveFile.Save("test");

    // Assert
    MockIO.Verify();
    MockCompressor.Verify();
    MockSerializer.Verify();
  }

  [Fact]
  public void Save_CompressorIsNull_WritesAndSerializesWithoutCompressing()
  {
    // Arrange
    SaveFile = new SaveFile(MockIO.Object, MockSerializer.Object, null);

    var ioStream = new MemoryStream();
    MockIO.Setup(io => io.Write()).Returns(ioStream).Verifiable();
    MockSerializer.Setup(serializer => serializer.Serialize(ioStream, "test", typeof(string))).Verifiable();

    // Act
    SaveFile.Save("test");

    // Assert
    MockIO.Verify();
    MockSerializer.Verify();
  }

  [Fact]
  public void Save_CompressionLevel_UsedByCompressor()
  {
    // Arrange
    MockCompressor.Setup(compressor => compressor.Compress(It.IsAny<Stream>(), CompressionLevel.Fastest, false)).Verifiable();

    // Act
    SaveFile.Save(CompressionLevel.Fastest);

    // Assert
    MockCompressor.Verify();
  }

  [Fact]
  public void Load_ReadsDecompressesAndDeserializes()
  {
    // Arrange
    var ioStream = new MemoryStream();
    var compressionStream = new MemoryStream();

    MockIO.Setup(io => io.Read()).Returns(ioStream).Verifiable();
    MockCompressor.Setup(compressor => compressor.Decompress(ioStream, false)).Returns(compressionStream).Verifiable();
    MockSerializer.Setup(serializer => serializer.Deserialize(compressionStream, typeof(string))).Returns("test").Verifiable();

    // Act
    var data = SaveFile.Load<string>();

    // Assert
    MockIO.Verify();
    MockCompressor.Verify();
    MockSerializer.Verify();
    Assert.Equal("test", data);
  }

  [Fact]
  public void Load_CompressorIsNull_ReadsAndDeserializesWithoutDecompressing()
  {
    // Arrange
    SaveFile = new SaveFile(MockIO.Object, MockSerializer.Object, null);

    var ioStream = new MemoryStream();
    MockIO.Setup(io => io.Read()).Returns(ioStream).Verifiable();
    MockSerializer.Setup(serializer => serializer.Deserialize(ioStream, typeof(string))).Returns("test").Verifiable();

    // Act
    var data = SaveFile.Load<string>();

    // Assert
    MockIO.Verify();
    MockSerializer.Verify();
    Assert.Equal("test", data);
  }

  [Fact]
  public void Load_DataIsNull_DoesNotSetChunkData()
  {
    // Arrange
    MockSerializer.Setup(serializer => serializer.Deserialize(It.IsAny<Stream>(), It.IsAny<Type>())).Returns((object?)null).Verifiable();

    // Act
    var data = SaveFile.Load<string>();

    // Assert
    MockSerializer.Verify();
    Assert.Null(data);
  }

  [Fact]
  public void Exists_ReturnsIOExists()
  {
    // Arrange
    MockIO.Setup(io => io.Exists()).Returns(true).Verifiable();

    // Act
    var result = SaveFile.Exists();

    // Assert
    MockIO.Verify();
    Assert.True(result);
  }

  [Fact]
  public void Delete_CallsIODelete()
  {
    // Arrange
    MockIO.Setup(io => io.Delete()).Verifiable();

    // Act
    SaveFile.Delete();

    // Assert
    MockIO.Verify();
  }

  [Fact]
  public void SaveAsync_CompletedSynchronously()
  {
    var task = SaveFile.SaveAsync("test", cancellationToken: TestContext.Current.CancellationToken);
    Assert.True(task.IsCompletedSuccessfully);
  }

  [Fact]
  public void SaveAsync_CompressorIsNull_CompletedSynchronously()
  {
    SaveFile = new SaveFile(MockIO.Object, MockSerializer.Object, null);
    var task = SaveFile.SaveAsync("test", cancellationToken: TestContext.Current.CancellationToken);
    Assert.True(task.IsCompletedSuccessfully);
  }

  [Fact]
  public void LoadAsync_CompletedSynchronously()
  {
    var task = SaveFile.LoadAsync<string>(TestContext.Current.CancellationToken);
    Assert.True(task.IsCompletedSuccessfully);
  }

  [Fact]
  public void ExistsAsync_CompletedSynchronously()
  {
    var task = SaveFile.ExistsAsync(TestContext.Current.CancellationToken);
    Assert.True(task.IsCompletedSuccessfully);
  }

  [Fact]
  public void DeleteAsync_CompletedSynchronously()
  {
    var task = SaveFile.DeleteAsync(TestContext.Current.CancellationToken);
    Assert.True(task.IsCompletedSuccessfully);
  }
}
