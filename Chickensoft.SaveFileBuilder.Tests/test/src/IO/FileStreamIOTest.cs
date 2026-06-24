namespace Chickensoft.SaveFileBuilder.Tests.IO;

using System.IO;
using System.Text;
using Chickensoft.SaveFileBuilder.IO;

public class FileStreamIOTest : IDisposable
{
  private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), nameof(FileStreamIOTest));
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
    streamIO.FileInfo.FullName.ShouldBe(fileInfo.FullName);
  }

  [Fact]
  public void Constructor_WithFileName_SetsFileInfo()
  {
    // Arrange
    var fileName = GetTestFilePath();

    // Act
    var streamIO = new FileStreamIO(fileName);

    // Assert
    streamIO.FileInfo.FullName.ShouldBe(fileName);
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
    stream.ShouldNotBeNull();
    stream.CanRead.ShouldBeTrue();
    using var reader = new StreamReader(stream);
    var content = reader.ReadToEnd();
    content.ShouldBe("test content");
  }

  [Fact]
  public void Read_NonExistingFile_ThrowsFileNotFoundException()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);

    // Act & Assert
    Should.Throw<FileNotFoundException>(streamIO.Read);
  }

  [Fact]
  public void Write_NonExistingFile_CreatesFileWithCorrectContent()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);
    var expectedContent = "test write content";

    // Act
    using var data = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));
    streamIO.Write(data);

    // Assert
    File.Exists(filePath).ShouldBeTrue();
    File.ReadAllText(filePath).ShouldBe(expectedContent);
  }

  [Fact]
  public void Write_ExistingFile_ReplacesContent()
  {
    // Arrange
    var filePath = GetTestFilePath();
    File.WriteAllText(filePath, "old content");
    var streamIO = new FileStreamIO(filePath);
    var newContent = "new content";

    // Act
    using var data = new MemoryStream(Encoding.UTF8.GetBytes(newContent));
    streamIO.Write(data);

    // Assert
    File.ReadAllText(filePath).ShouldBe(newContent);
  }

  [Fact]
  public void Write_NonExistingDirectory_CreatesDirectoryAndWritesContent()
  {
    // Arrange
    var subdirectory = Path.Combine(_testDirectory, "subdir1", "subdir2");
    var filePath = Path.Combine(subdirectory, _testFileName);
    var streamIO = new FileStreamIO(filePath);
    var expectedContent = "directory creation test";

    // Act
    using var data = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));
    streamIO.Write(data);

    // Assert
    Directory.Exists(subdirectory).ShouldBeTrue();
    File.ReadAllText(filePath).ShouldBe(expectedContent);
  }

  [Fact]
  public void Write_WhenCopyFails_OriginalFileIsPreserved()
  {
    // Arrange
    var filePath = GetTestFilePath();
    File.WriteAllText(filePath, "original content");
    var streamIO = new FileStreamIO(filePath);

    // Act
    Should.Throw<IOException>(() => streamIO.Write(new ThrowingStream()));

    // Assert
    File.ReadAllText(filePath).ShouldBe("original content");
  }

  [Fact]
  public void Write_WhenCopyFails_NoTempFileIsLeft()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);

    // Act
    Should.Throw<IOException>(() => streamIO.Write(new ThrowingStream()));

    // Assert
    Directory.GetFiles(_testDirectory).ShouldBeEmpty();
  }

  private sealed class ThrowingStream : Stream
  {
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }
    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) => throw new IOException("Simulated read failure");
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
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
    exists.ShouldBeTrue();
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
    exists.ShouldBeFalse();
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
    existsBefore.ShouldBeFalse();
    existsAfter.ShouldBeTrue();
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
    File.Exists(filePath).ShouldBeFalse();
  }

  [Fact]
  public void Delete_NonExistingFile_DoesNotThrow()
  {
    // Arrange
    var filePath = GetTestFilePath();
    var streamIO = new FileStreamIO(filePath);

    // Act & Assert
    var exception = Record.Exception(streamIO.Delete);
    exception.ShouldBeNull();
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
    exists.ShouldBeFalse();
  }
}
