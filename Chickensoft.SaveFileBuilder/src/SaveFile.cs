namespace Chickensoft.SaveFileBuilder;

using System;
using System.Threading.Tasks;

/// <summary>
/// Represents a save file composed of one or more save chunks.
/// </summary>
/// <typeparam name="TData">Type of data represented by the save file.
/// </typeparam>
public interface ISaveFile<TData> where TData : class {
  /// <summary>
  /// Root save chunk from which the save file contents are composed.
  /// </summary>
  ISaveChunk<TData> Root { get; }

  /// <summary>
  /// Collects save data from the save file chunk tree and saves it.
  /// </summary>
  /// <returns>Asynchronous task.</returns>
  Task Save();

  /// <summary>
  /// Loads save data and restores the save file chunk tree.
  /// </summary>
  /// <returns>Asynchronous task.</returns>
  Task Load();
}

/// <inheritdoc cref="ISaveFile{TData}"/>
public class SaveFile<TData> : ISaveFile<TData> where TData : class {
  /// <inheritdoc cref="ISaveFile{TData}.Root"/>
  public ISaveChunk<TData> Root { get; }

  private readonly Func<TData, Task> _onSave;
  private readonly Func<Task<TData?>> _onLoad;

  /// <inheritdoc cref="ISaveFile{TData}"/>
  /// <param name="root">
  /// <inheritdoc cref="ISaveFile{TData}.Root" path="/summary" />
  /// </param>
  /// <param name="onSave">Function that saves the data.</param>
  /// <param name="onLoad">Function that loads the data.</param>
  public SaveFile(
    SaveChunk<TData> root,
    Func<TData, Task> onSave,
    Func<Task<TData?>> onLoad
  ) {
    Root = root;
    _onSave = onSave;
    _onLoad = onLoad;
  }

  /// <inheritdoc cref="ISaveFile{TData}.Save"/>
  public Task Save() => _onSave(Root.GetSaveData());

  /// <inheritdoc cref="ISaveFile{TData}.Load"/>
  public async Task Load() {
    // Loading save data is asynchronous since it's usually coming from
    // the disk or network.
    var data = await _onLoad();

    if (data is null) {
      return;
    }

    // Actually restoring the loaded data is synchronous.
    Root.LoadSaveData(data);
  }
}
