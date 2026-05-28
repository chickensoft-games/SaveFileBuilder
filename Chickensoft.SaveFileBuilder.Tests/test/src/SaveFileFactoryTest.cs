namespace Chickensoft.SaveFileBuilder.Tests;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Chickensoft.SaveFileBuilder;
using Chickensoft.SaveFileBuilder.IO;

public partial class SaveFileFactoryTest : IDisposable
{
  private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), nameof(SaveFileFactoryTest));
  private readonly string _testFileName = "test.json.gz";

  private CancellationToken CancellationToken { get; }

  public SaveFileFactoryTest(ITestContextAccessor testContextAccessor)
  {
    CancellationToken = testContextAccessor.Current.CancellationToken;

    Directory.CreateDirectory(_testDirectory);
  }

  public void Dispose()
  {
    // Clean up test directory after tests
    if (Directory.Exists(_testDirectory))
    {
      Directory.Delete(_testDirectory, true);
    }
    GC.SuppressFinalize(this);
  }

  #region Helper Methods and Classes
  private string GetTestFilePath() => Path.Combine(_testDirectory, _testFileName);

  private class MockStream : MemoryStream
  {
    // Do nothing to prevent the stream from being disposed during tests
    [SuppressMessage("Usage", "CA2215:Dispose methods should call base class dispose", Justification = "This is intentional for testing purposes.")]
    protected override void Dispose(bool disposing) { }

    [SuppressMessage("Usage", "CA2215:Dispose methods should call base class dispose", Justification = "This is intentional for testing purposes.")]
    public override ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void DisposeForReal() => base.Dispose(true);
    public ValueTask DisposeAsyncForReal() => base.DisposeAsync();
  }
  #endregion

  #region CreateGZipJsonFile
  [Fact]
  public void CreateGZipJsonFile_WithOptions_SavesToFileUsingGZipAndJson()
  {
    // Arrange
    var options = new JsonSerializerOptions { WriteIndented = true };
    var testData = new TestData { Name = "Test", Value = 123 };

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(GetTestFilePath(), options);
    saveFile.Save(testData);

    var fileInfo = new FileInfo(GetTestFilePath());
    using var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read);
    using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
    var data = JsonSerializer.Deserialize<TestData>(gzipStream, options);

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonFile_WithOptions_LoadsFromFileUsingGZipAndJson()
  {
    // Arrange
    var options = new JsonSerializerOptions { WriteIndented = true };
    var testData = new TestData { Name = "Test", Value = 123 };

    // Act
    var fileInfo = new FileInfo(GetTestFilePath());
    fileInfo.Refresh();
    using (var fileStream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write))
    {
      using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
      JsonSerializer.Serialize(gzipStream, testData, options);
    }

    var saveFile = SaveFile.CreateGZipJsonFile(GetTestFilePath(), options);
    var data = saveFile.Load<TestData>();

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonFile_WithContext_SavesToFileUsingGZipAndJson()
  {
    // Arrange
    var context = TestJsonContext.Default;
    var testData = new TestData { Name = "Test", Value = 123 };

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(GetTestFilePath(), context);
    saveFile.Save(testData);

    var fileInfo = new FileInfo(GetTestFilePath());
    using var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read);
    using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
    var data = JsonSerializer.Deserialize(gzipStream, typeof(TestData), context) as TestData;

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonFile_WithContext_LoadsFromFileUsingGZipAndJson()
  {
    // Arrange
    var context = TestJsonContext.Default;
    var testData = new TestData { Name = "Test", Value = 123 };

    var fileInfo = new FileInfo(GetTestFilePath());
    using (var fileStream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write))
    {
      using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
      JsonSerializer.Serialize(gzipStream, testData, typeof(TestData), context);
    }

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(GetTestFilePath(), context);
    var data = saveFile.Load<TestData>();

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonFile_WithJsonTypeInfo_SavesToFileUsingGZipAndJson()
  {
    // Arrange
    var typeInfo = TestJsonContext.Default.TestData;
    var testData = new TestData { Name = "Test", Value = 123 };

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(GetTestFilePath(), typeInfo);
    saveFile.Save(testData);

    var fileInfo = new FileInfo(GetTestFilePath());
    using var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read);
    using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
    var data = JsonSerializer.Deserialize(gzipStream, typeInfo);

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonFile_WithJsonTypeInfo_LoadsFromFileUsingGZipAndJson()
  {
    // Arrange
    var typeInfo = TestJsonContext.Default.TestData;
    var testData = new TestData { Name = "Test", Value = 123 };

    var fileInfo = new FileInfo(GetTestFilePath());
    using (var fileStream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write))
    {
      using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
      JsonSerializer.Serialize(gzipStream, testData, typeInfo);
    }

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(GetTestFilePath(), typeInfo);
    var data = saveFile.Load<TestData>();

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }
  #endregion

  #region CreateGZipJsonIO IStreamIO
  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndOptions_SavesUsingGZipAndJson()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var options = new JsonSerializerOptions { WriteIndented = true };
    var testData = new TestData { Name = "Test", Value = 123 };
    var mockStream = new MockStream();
    mockIO.Setup(io => io.Write()).Returns(mockStream);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object, options);
    saveFile.Save(testData);

    mockStream.Seek(0, SeekOrigin.Begin);
    using var gzipStream = new GZipStream(mockStream, CompressionMode.Decompress);
    var data = JsonSerializer.Deserialize<TestData>(gzipStream, options);
    mockStream.DisposeForReal();

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndOptions_LoadsUsingGZipAndJson()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var options = new JsonSerializerOptions { WriteIndented = true };
    var testData = new TestData { Name = "Test", Value = 123 };
    using var memoryStream = new MemoryStream();
    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
    {
      JsonSerializer.Serialize(gzipStream, testData, options);
    }
    memoryStream.Seek(0, SeekOrigin.Begin);
    mockIO.Setup(io => io.Read()).Returns(memoryStream);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object, options);
    var data = saveFile.Load<TestData>();

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndContext_SavesUsingGZipAndJson()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var context = TestJsonContext.Default;
    var testData = new TestData { Name = "Test", Value = 123 };
    var mockStream = new MockStream();
    mockIO.Setup(io => io.Write()).Returns(mockStream);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object, context);
    saveFile.Save(testData);

    mockStream.Seek(0, SeekOrigin.Begin);
    using var gzipStream = new GZipStream(mockStream, CompressionMode.Decompress);
    var data = JsonSerializer.Deserialize(gzipStream, typeof(TestData), context) as TestData;
    mockStream.DisposeForReal();

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndContext_LoadsUsingGZipAndJson()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var context = TestJsonContext.Default;
    var testData = new TestData { Name = "Test", Value = 123 };
    using var memoryStream = new MemoryStream();
    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
    {
      JsonSerializer.Serialize(gzipStream, testData, typeof(TestData), context);
    }
    memoryStream.Seek(0, SeekOrigin.Begin);
    mockIO.Setup(io => io.Read()).Returns(memoryStream);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object, context);
    var data = saveFile.Load<TestData>();

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndJsonTypeInfo_SavesUsingGZipAndJson()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var typeInfo = TestJsonContext.Default.TestData;
    var testData = new TestData { Name = "Test", Value = 123 };
    var mockStream = new MockStream();
    mockIO.Setup(io => io.Write()).Returns(mockStream);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object, typeInfo);
    saveFile.Save(testData);

    mockStream.Seek(0, SeekOrigin.Begin);
    using var gzipStream = new GZipStream(mockStream, CompressionMode.Decompress);
    var data = JsonSerializer.Deserialize(gzipStream, typeInfo);
    mockStream.DisposeForReal();

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndJsonTypeInfo_LoadsUsingGZipAndJson()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var typeInfo = TestJsonContext.Default.TestData;
    var testData = new TestData { Name = "Test", Value = 123 };
    using var memoryStream = new MemoryStream();
    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
    {
      JsonSerializer.Serialize(gzipStream, testData, typeInfo);
    }
    memoryStream.Seek(0, SeekOrigin.Begin);
    mockIO.Setup(io => io.Read()).Returns(memoryStream);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object, typeInfo);
    var data = saveFile.Load<TestData>();

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }
  #endregion

  #region CreateGZipJsonIO IAsyncStreamIO

  [Fact]
  public async Task CreateGZipJsonIO_WithIAsyncStreamIOAndOptions_SavesUsingGZipAndJson()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var options = new JsonSerializerOptions { WriteIndented = true };
    var testData = new TestData { Name = "Test", Value = 123 };
    byte[]? capturedBytes = null;
    mockAsyncIO
      .Setup(io => io.WriteAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
      .Callback<Stream, CancellationToken>((stream, _) => capturedBytes = ((MemoryStream)stream).ToArray())
      .Returns(Task.CompletedTask);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object, options);
    await saveFile.SaveAsync(testData, cancellationToken: CancellationToken);

    capturedBytes.ShouldNotBeNull();
    using var capturedStream = new MemoryStream(capturedBytes);
    using var gzipStream = new GZipStream(capturedStream, CompressionMode.Decompress);
    var data = JsonSerializer.Deserialize<TestData>(gzipStream, options);

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public async Task CreateGZipJsonIO_WithIAsyncStreamIOAndOptions_LoadsUsingGZipAndJson()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var options = new JsonSerializerOptions { WriteIndented = true };
    var testData = new TestData { Name = "Test", Value = 123 };
    var memoryStream = new MemoryStream();
    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
    {
      JsonSerializer.Serialize(gzipStream, testData, options);
    }
    memoryStream.Seek(0, SeekOrigin.Begin);
    mockAsyncIO.Setup(io => io.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(memoryStream);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object, options);
    var data = await saveFile.LoadAsync<TestData>(CancellationToken);

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public async Task CreateGZipJsonIO_WithIAsyncStreamIOAndContext_SavesUsingGZipAndJson()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var context = TestJsonContext.Default;
    var testData = new TestData { Name = "Test", Value = 123 };
    byte[]? capturedBytes = null;
    mockAsyncIO
      .Setup(io => io.WriteAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
      .Callback<Stream, CancellationToken>((stream, _) => capturedBytes = ((MemoryStream)stream).ToArray())
      .Returns(Task.CompletedTask);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object, context);
    await saveFile.SaveAsync(testData, cancellationToken: CancellationToken);

    capturedBytes.ShouldNotBeNull();
    using var capturedStream = new MemoryStream(capturedBytes);
    using var gzipStream = new GZipStream(capturedStream, CompressionMode.Decompress);
    var data = JsonSerializer.Deserialize(gzipStream, typeof(TestData), context) as TestData;

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public async Task CreateGZipJsonIO_WithIAsyncStreamIOAndContext_LoadsUsingGZipAndJson()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var context = TestJsonContext.Default;
    var testData = new TestData { Name = "Test", Value = 123 };
    var memoryStream = new MemoryStream();
    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
    {
      JsonSerializer.Serialize(gzipStream, testData, typeof(TestData), context);
    }
    memoryStream.Seek(0, SeekOrigin.Begin);
    mockAsyncIO.Setup(io => io.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(memoryStream);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object, context);
    var data = await saveFile.LoadAsync<TestData>(CancellationToken);

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public async Task CreateGZipJsonIO_WithIAsyncStreamIOAndJsonTypeInfo_SavesUsingGZipAndJson()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var typeInfo = TestJsonContext.Default.TestData;
    var testData = new TestData { Name = "Test", Value = 123 };
    byte[]? capturedBytes = null;
    mockAsyncIO
      .Setup(io => io.WriteAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
      .Callback<Stream, CancellationToken>((stream, _) => capturedBytes = ((MemoryStream)stream).ToArray())
      .Returns(Task.CompletedTask);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object, typeInfo);
    await saveFile.SaveAsync(testData, cancellationToken: CancellationToken);

    capturedBytes.ShouldNotBeNull();
    using var capturedStream = new MemoryStream(capturedBytes);
    using var gzipStream = new GZipStream(capturedStream, CompressionMode.Decompress);
    var data = JsonSerializer.Deserialize(gzipStream, typeInfo);

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }

  [Fact]
  public async Task CreateGZipJsonIO_WithIAsyncStreamIOAndJsonTypeInfo_LoadsUsingGZipAndJson()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var typeInfo = TestJsonContext.Default.TestData;
    var testData = new TestData { Name = "Test", Value = 123 };
    var memoryStream = new MemoryStream();
    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
    {
      JsonSerializer.Serialize(gzipStream, testData, typeInfo);
    }
    memoryStream.Seek(0, SeekOrigin.Begin);
    mockAsyncIO.Setup(io => io.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(memoryStream);

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object, typeInfo);
    var data = await saveFile.LoadAsync<TestData>(CancellationToken);

    // Assert
    data.ShouldNotBeNull();
    data.Name.ShouldBe("Test");
    data.Value.ShouldBe(123);
  }
  #endregion
}
