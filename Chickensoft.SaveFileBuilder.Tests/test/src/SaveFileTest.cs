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
  public void CanSaveSynchronously_IsTrue() => SaveFile.CanSaveSynchronously.ShouldBeTrue();

  [Fact]
  public void Save_WritesCompressesAndSerializes()
  {
    // Arrange
    var compressionStream = new MemoryStream();

    MockCompressor.Setup(compressor => compressor.Compress(It.IsAny<MemoryStream>(), default, true)).Returns(compressionStream).Verifiable();
    MockSerializer.Setup(serializer => serializer.Serialize(compressionStream, "test", typeof(string))).Verifiable();
    MockIO.Setup(io => io.Write(It.IsAny<MemoryStream>())).Verifiable();

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

    MockSerializer.Setup(serializer => serializer.Serialize(It.IsAny<MemoryStream>(), "test", typeof(string))).Verifiable();
    MockIO.Setup(io => io.Write(It.IsAny<MemoryStream>())).Verifiable();

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
    MockCompressor.Setup(compressor => compressor.Compress(It.IsAny<MemoryStream>(), CompressionLevel.Fastest, true)).Verifiable();

    // Act
    SaveFile.Save("test", CompressionLevel.Fastest);

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
    data.ShouldBe("test");
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
    data.ShouldBe("test");
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
    data.ShouldBeNull();
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
    result.ShouldBeTrue();
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
    task.IsCompletedSuccessfully.ShouldBeTrue();
  }

  [Fact]
  public void SaveAsync_CompressorIsNull_CompletedSynchronously()
  {
    SaveFile = new SaveFile(MockIO.Object, MockSerializer.Object, null);
    var task = SaveFile.SaveAsync("test", cancellationToken: TestContext.Current.CancellationToken);
    task.IsCompletedSuccessfully.ShouldBeTrue();
  }

  [Fact]
  public void LoadAsync_CompletedSynchronously()
  {
    var task = SaveFile.LoadAsync<string>(TestContext.Current.CancellationToken);
    task.IsCompletedSuccessfully.ShouldBeTrue();
  }

  [Fact]
  public void ExistsAsync_CompletedSynchronously()
  {
    var task = SaveFile.ExistsAsync(TestContext.Current.CancellationToken);
    task.IsCompletedSuccessfully.ShouldBeTrue();
  }

  [Fact]
  public void DeleteAsync_CompletedSynchronously()
  {
    var task = SaveFile.DeleteAsync(TestContext.Current.CancellationToken);
    task.IsCompletedSuccessfully.ShouldBeTrue();
  }

  [Fact]
  public void Save_WhenSerializationFails_WriteIsNeverCalled()
  {
    // Arrange
    MockSerializer.Setup(serializer => serializer.Serialize(It.IsAny<Stream>(), It.IsAny<object>(), It.IsAny<Type>()))
      .Throws<InvalidOperationException>();

    // Act
    Should.Throw<InvalidOperationException>(() => SaveFile.Save("test"));

    // Assert
    MockIO.Verify(io => io.Write(It.IsAny<Stream>()), Times.Never);
  }

  [Fact]
  public void Save_WritesBufferedDataToIO()
  {
    // Arrange
    var expectedBytes = System.Text.Encoding.UTF8.GetBytes("test-data");
    MockSerializer.Setup(serializer => serializer.Serialize(It.IsAny<MemoryStream>(), "test", typeof(string)))
      .Callback<Stream, object?, Type>((stream, _, _) => stream.Write(expectedBytes, 0, expectedBytes.Length));

    byte[]? capturedBytes = null;
    MockIO.Setup(io => io.Write(It.IsAny<MemoryStream>()))
      .Callback<Stream>(s => capturedBytes = ((MemoryStream)s).ToArray());

    // Act
    SaveFile.Save("test");

    // Assert
    capturedBytes.ShouldNotBeNull();
    capturedBytes.ShouldBe(expectedBytes);
  }

  [Fact]
  public async Task SaveAsync_WhenSerializationFails_WriteIsNeverCalled()
  {
    // Arrange
    MockSerializer.Setup(serializer => serializer.Serialize(It.IsAny<Stream>(), It.IsAny<object>(), It.IsAny<Type>()))
      .Throws<InvalidOperationException>();

    // Act
    await Should.ThrowAsync<InvalidOperationException>(
      () => SaveFile.SaveAsync("test", cancellationToken: TestContext.Current.CancellationToken).AsTask()
    );

    // Assert
    MockIO.Verify(io => io.Write(It.IsAny<Stream>()), Times.Never);
  }
}
