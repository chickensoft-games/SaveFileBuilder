namespace Chickensoft.SaveFileBuilder.IO;

using System.Diagnostics.CodeAnalysis;
using System.IO;

/// <summary>Provides a read- and write <see cref="Stream"/> from a file.</summary>
public class FileStreamIO : IStreamIO
{
  /// <summary>The <see cref="System.IO.FileInfo"/> of the file.</summary>
  public FileInfo FileInfo { get; }

  /// <summary>Initializes a new instance of the <see cref="FileStreamIO"/> class.</summary>
  /// <param name="fileInfo">The <see cref="System.IO.FileInfo"/> of the file.</param>
  public FileStreamIO(FileInfo fileInfo)
  {
    FileInfo = fileInfo;
  }

  /// <summary>Initializes a new instance of the <see cref="FileStreamIO"/> class.</summary>
  /// <param name="fileName">The filename of the file.</param>
  public FileStreamIO(string fileName)
  {
    FileInfo = new FileInfo(fileName);
  }

  /// <inheritdoc />
  public Stream Read() => FileInfo.Open(FileMode.Open, FileAccess.Read);

  /// <inheritdoc />
  public Stream Write()
  {
    FileInfo.Refresh();
    var directoryName = GetDirectoryNameOrThrowIfNull();
    Directory.CreateDirectory(directoryName);
    return FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write);
  }

  // Note: Testing the DirectoryName is null scenario is not feasible because:
  // 1. FileInfo is a sealed class and cannot be mocked
  // 2. In practice, FileInfo.DirectoryName is virtually never null
  // 3. Any FileInfo created with a path will resolve to an absolute path with a directory
  // The defensive null check in FileStreamIO.Write() remains as good practice.
  [ExcludeFromCodeCoverage]
  private string GetDirectoryNameOrThrowIfNull() => FileInfo.DirectoryName ?? throw new DirectoryNotFoundException("The directory of the file does not exist.");

  /// <inheritdoc />
  public bool Exists()
  {
    FileInfo.Refresh();
    return FileInfo.Exists;
  }

  /// <inheritdoc />
  public void Delete() => FileInfo.Delete();
}
