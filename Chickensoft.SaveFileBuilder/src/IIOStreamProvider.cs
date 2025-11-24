namespace Chickensoft.SaveFileBuilder;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Provides a read- and write <see cref="Stream"/> from an input / output source.</summary>
public interface IIOStreamProvider
{
  /// <summary>Creates a read-only <see cref="Stream"/>.</summary>
  /// <returns>A new read-only <see cref="Stream"/> object.</returns>
  Stream Read();

  /// <summary>Creates a write-only <see cref="Stream"/>.</summary>
  /// <returns>A new write-only <see cref="Stream"/> object.</returns>
  Stream Write();

  /// <summary>Determines whether the io source exists.</summary>
  /// <returns><see langword="true"/> if the io source exists; otherwise, <see langword="false"/>.</returns>
  bool Exists();

  /// <summary>Permanently deletes the io source.</summary>
  void Delete();
}

public interface IAsyncIOStreamProvider
{
  Task<Stream> ReadAsync(CancellationToken cancellationToken = default);
  Task WriteAsync(Stream stream, CancellationToken cancellationToken = default);
  Task<bool> ExistsAsync(CancellationToken cancellationToken = default);
  Task<bool> DeleteAsync(CancellationToken cancellationToken = default);
}
