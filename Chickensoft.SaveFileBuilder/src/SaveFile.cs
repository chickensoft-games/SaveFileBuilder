namespace Chickensoft.SaveFileBuilder;

using System;
using System.Threading.Tasks;

/// <summary>
/// Represents a save file composed of one or more save chunks.
/// </summary>
/// <typeparam name="TData">Type of data represented by the save file.
/// </typeparam>
public interface ISaveFile<TData> where TData : class
{
  /// <summary>
  /// Callback that saves the data to the save file.
  /// </summary>
  Func<TData, Task> OnSave { get; }

  /// <summary>
  /// Callback that loads the data from the save file.
  /// </summary>
  /// <returns>Save data.</returns>
  Func<Task<TData?>> OnLoad { get; }

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
public class SaveFile<TData> : ISaveFile<TData> where TData : class
{
  /// <inheritdoc cref="ISaveFile{TData}.Root"/>
  public ISaveChunk<TData> Root { get; }

  /// <inheritdoc cref="ISaveFile{TData}.OnSave"/>
  public Func<TData, Task> OnSave { get; }

  /// <inheritdoc cref="ISaveFile{TData}.OnLoad"/>
  public Func<Task<TData?>> OnLoad { get; }

  /// <inheritdoc cref="ISaveFile{TData}"/>
  /// <param name="root">
  /// <inheritdoc cref="ISaveFile{TData}.Root" path="/summary" />
  /// </param>
  /// <param name="onSave">Function that saves the data.</param>
  /// <param name="onLoad">Function that loads the data.</param>
  public SaveFile(
    ISaveChunk<TData> root,
    Func<TData, Task> onSave,
    Func<Task<TData?>> onLoad
  )
  {
    Root = root;
    OnSave = onSave;
    OnLoad = onLoad;
  }

  /// <inheritdoc cref="ISaveFile{TData}.Save"/>
  public Task Save() => OnSave(Root.GetSaveData());

  /// <inheritdoc cref="ISaveFile{TData}.Load"/>
  public async Task Load()
  {
    // Loading save data is asynchronous since it's usually coming from
    // the disk or network.
    var data = await OnLoad();

    if (data is null)
    {
      return;
    }

    // Actually restoring the loaded data is synchronous.
    Root.LoadSaveData(data);
  }
}
