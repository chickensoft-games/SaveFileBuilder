namespace Chickensoft.SaveFileBuilder;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Provides a read- and write <see cref="Stream"/> from an input / output source.</summary>
public interface IIOStreamProvider
{
  /// <summary>Returns a read-only <see cref="Stream"/> from the io source.</summary>
  /// <returns>A new read-only <see cref="Stream"/> object from the io source.</returns>
  Stream Read();

  /// <summary>Returns a write-only <see cref="Stream"/> from the io source.</summary>
  /// <returns>A new write-only <see cref="Stream"/> object from the io source.</returns>
  Stream Write();

  /// <summary>Determines whether the io source exists.</summary>
  /// <returns><see langword="true"/> if the io source exists; otherwise, <see langword="false"/>.</returns>
  bool Exists();

  /// <summary>Permanently deletes the io source.</summary>
  void Delete();
}

/// <summary>Provides a read <see cref="Stream"/> from- and requests a write <see cref="Stream"/> for an input / output source asynchronously.</summary>
public interface IAsyncIOStreamProvider
{
  /// <summary>Asynchronously reads the underlying data and returns a read-only <see cref="Stream"/> from the io source.</summary>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous read operation.</param>
  /// <returns>A task that represents the asynchronous read operation. The value of the task is a read-only <see cref="Stream"/> from the io source.</returns>
  Task<Stream> ReadAsync(CancellationToken cancellationToken = default);

  /// <summary>Requests a write stream to write data to the underlying source asynchronously.</summary>
  /// <param name="stream">The stream to write to the io source.</param>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous write operation.</param>
  /// <returns>A task that represents the asynchronous write operation.</returns>
  Task WriteAsync(Stream stream, CancellationToken cancellationToken = default);

  /// <summary>Asynchronously determines whether the io source exists.</summary>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous exists operation.</param>
  /// <returns>A task that represents the asynchronous exists operation. The value of the task is <see langword="true"/> if the io source exists; otherwise, <see langword="false"/>.</returns>
  Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

  /// <summary>Asynchronously deletes the io source.</summary>
  /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous delete operation.</param>
  /// <returns>A task that represents the asynchronous delete operation. The value of the task is <see langword="true"/> if the io source was deleted; otherwise, <see langword="false"/>.</returns>
  Task<bool> DeleteAsync(CancellationToken cancellationToken = default);
}
