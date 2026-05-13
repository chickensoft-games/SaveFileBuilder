namespace Chickensoft.SaveFileBuilder.Tests.IO;

using System.IO;
using Chickensoft.SaveFileBuilder.IO;

public class FileStreamIOTest : IDisposable
{
  private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), "FileStreamIOTest");
  private readonly string _testFileName = "test.txt";

  public FileStreamIOTest()
  {
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

  private string GetTestFilePath() => Path.Combine(_testDirectory, _testFileName);

  [Fact]
  public void Constructor_WithFileInfo_SetsFileInfo()
  {
    // Arrange
    var fileInfo = new FileInfo(GetTestFilePath());

    // Act
    var streamIO = new FileStreamIO(fileInfo);

    // Assert
    Assert.Equal(fileInfo.FullName, streamIO.FileInfo.FullName);
  }

  [Fact]
  public void Constructor_WithFileName_SetsFileInfo()
  {
    // Arrange
    var fileName = GetTestFilePath();

    // Act
    var streamIO = new FileStreamIO(fileName);

    // Assert
    Assert.Equal(fileName, streamIO.FileInfo.FullName);
  }

  [Fact]
  public void Read_ExistingFile_ReturnsReadableStream()
  {
    // Arrange
    var filePath = GetTestFilePath();
    File.WriteAllText(filePath, "test content");
    var streamIO = new FileStreamIO(filePath);

    // Act
    using var stream = streamIO.Read();

    // Assert
    Assert.NotNull(stream);
    Assert.True(stream.CanRead);
    using var reader = new StreamReader(stream);
    var content = reader.ReadToEnd();
    Assert.Equal("test content", content);
  }

  [Fact]
  public void Read_NonExistingFile_ThrowsFileNotFoundException()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);

    // Act & Assert
    Assert.Throws<FileNotFoundException>(streamIO.Read);
  }

  [Fact]
  public void Write_NonExistingFile_CreatesFileAndReturnsWritableStream()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);

    // Act
    using var stream = streamIO.Write();

    // Assert
    Assert.NotNull(stream);
    Assert.True(stream.CanWrite);
    Assert.True(File.Exists(filePath));
  }

  [Fact]
  public void Write_ExistingFile_ReturnsWritableStream()
  {
    // Arrange
    var filePath = GetTestFilePath();
    File.WriteAllText(filePath, "existing content");
    var streamIO = new FileStreamIO(filePath);

    // Act
    using var stream = streamIO.Write();

    // Assert
    Assert.NotNull(stream);
    Assert.True(stream.CanWrite);
  }

  [Fact]
  public void Write_NonExistingDirectory_CreatesDirectoryAndReturnsStream()
  {
    // Arrange
    var subdirectory = Path.Combine(_testDirectory, "subdir1", "subdir2");
    var filePath = Path.Combine(subdirectory, _testFileName);
    var streamIO = new FileStreamIO(filePath);

    // Act
    using var stream = streamIO.Write();

    // Assert
    Assert.NotNull(stream);
    Assert.True(stream.CanWrite);
    Assert.True(Directory.Exists(subdirectory));
  }

  [Fact]
  public void Write_AllowsWritingContent()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);
    var testContent = "test write content";

    // Act
    using (var stream = streamIO.Write())
    using (var writer = new StreamWriter(stream))
    {
      writer.Write(testContent);
    }

    // Assert
    var actualContent = File.ReadAllText(filePath);
    Assert.Equal(testContent, actualContent);
  }

  [Fact]
  public void Exists_ExistingFile_ReturnsTrue()
  {
    // Arrange
    var filePath = GetTestFilePath();
    File.WriteAllText(filePath, "content");
    var streamIO = new FileStreamIO(filePath);

    // Act
    var exists = streamIO.Exists();

    // Assert
    Assert.True(exists);
  }

  [Fact]
  public void Exists_NonExistingFile_ReturnsFalse()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);

    // Act
    var exists = streamIO.Exists();

    // Assert
    Assert.False(exists);
  }

  [Fact]
  public void Exists_AfterFileCreation_ReturnsTrue()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);

    // Act - Initially doesn't exist
    var existsBefore = streamIO.Exists();

    // Create the file
    File.WriteAllText(filePath, "content");

    // Act - Check after creation
    var existsAfter = streamIO.Exists();

    // Assert
    Assert.False(existsBefore);
    Assert.True(existsAfter);
  }

  [Fact]
  public void Delete_ExistingFile_DeletesFile()
  {
    // Arrange
    var filePath = GetTestFilePath();
    File.WriteAllText(filePath, "content");
    var streamIO = new FileStreamIO(filePath);

    // Act
    streamIO.Delete();

    // Assert
    Assert.False(File.Exists(filePath));
  }

  [Fact]
  public void Delete_NonExistingFile_DoesNotThrow()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);

    // Act & Assert
    var exception = Record.Exception(streamIO.Delete);
    Assert.Null(exception);
  }

  [Fact]
  public void Delete_AfterDeletion_ExistsReturnsFalse()
  {
    // Arrange
    var filePath = GetTestFilePath();
    File.WriteAllText(filePath, "content");
    var streamIO = new FileStreamIO(filePath);

    // Act
    streamIO.Delete();
    var exists = streamIO.Exists();

    // Assert
    Assert.False(exists);
  }
}
