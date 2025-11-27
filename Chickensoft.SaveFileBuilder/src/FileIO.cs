namespace Chickensoft.SaveFileBuilder;

using System.IO;

/// <summary>Provides a read- and write <see cref="Stream"/> from a file.</summary>
public class FileIO : IIOStreamProvider
{
  /// <summary>The <see cref="System.IO.FileInfo"/> of the file.</summary>
  public FileInfo FileInfo { get; }

  /// <summary>Initializes a new instance of the <see cref="FileIO"/> class.</summary>
  /// <param name="fileInfo">The <see cref="System.IO.FileInfo"/> of the file.</param>
  public FileIO(FileInfo fileInfo)
  {
    FileInfo = fileInfo;
  }

  /// <summary>Initializes a new instance of the <see cref="FileIO"/> class.</summary>
  /// <param name="fileName">The filename of the file.</param>
  public FileIO(string fileName)
  {
    FileInfo = new FileInfo(fileName);
  }

  /// <inheritdoc />
  public Stream Read() => FileInfo.Open(FileMode.Open, FileAccess.Read);

  /// <inheritdoc />
  public Stream Write()
  {
    FileInfo.Refresh();
    if (FileInfo.DirectoryName == null)
    {
      throw new DirectoryNotFoundException("The directory of the file does not exist.");
    }

    Directory.CreateDirectory(FileInfo.DirectoryName);
    return FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write);
  }

  /// <inheritdoc />
  public bool Exists()
  {
    FileInfo.Refresh();
    return FileInfo.Exists;
  }

  /// <inheritdoc />
  public void Delete() => FileInfo.Delete();
}
