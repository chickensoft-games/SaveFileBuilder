namespace Chickensoft.SaveFileBuilder;

using System;
using Chickensoft.Collections;

/// <summary>
/// Represents a section of a save file. A chunk can contain other chunks.
/// Save chunks form a tree composing the save file contents.
/// </summary>
/// <typeparam name="TData">Type of data associated with this save chunk.
/// </typeparam>
public interface ISaveChunk<TData> where TData : class
{
  /// <summary>
  /// <para>
  /// Callback that returns save data from the scene tree.
  /// </para>
  /// <para>
  /// Only call this directly for testing. Prefer calling
  /// <see cref="GetSaveData"/> to get the save data.
  /// </para>
  /// </summary>
  Func<ISaveChunk<TData>, TData> OnSave { get; }

  /// <summary>
  /// <para>
  /// Callback that loads the data into the scene tree.
  /// </para>
  /// <para>
  /// Only call this directly for testing. Prefer calling
  /// <see cref="LoadSaveData"/> to load the save data.
  /// </para>
  /// </summary>
  Action<ISaveChunk<TData>, TData> OnLoad { get; }

  /// <summary>
  /// Gets the data associated with this save chunk.
  /// </summary>
  /// <returns>Save chunk data.</returns>
  TData GetSaveData();

  /// <summary>
  /// Loads the data associated with this save chunk.
  /// </summary>
  /// <param name="data"></param>
  void LoadSaveData(TData data);

  /// <summary>
  /// Adds a child save chunk to this chunk.
  /// </summary>
  /// <typeparam name="TDataType">Type of data associated with the child save
  /// chunk.</typeparam>
  void AddChunk<TDataType>(ISaveChunk<TDataType> child) where TDataType : class;

  /// <summary>
  /// Adds a child save chunk to or overwrites an existing chunk in this chunk, based on its compile-time type.
  /// </summary>
  /// <typeparam name="TDataType">Type of data associated with the child save
  /// chunk.</typeparam>
  void OverwriteChunk<TDataType>(ISaveChunk<TDataType> child) where TDataType : class;

  /// <summary>
  /// Gets a child save chunk.
  /// </summary>
  /// <typeparam name="TDataType">Type of data associated with the child save
  /// chunk.</typeparam>
  ISaveChunk<TDataType> GetChunk<TDataType>() where TDataType : class;

  /// <summary>
  /// Gets the data associated with a child save chunk.
  /// </summary>
  /// <typeparam name="TDataType">Type of data associated with the child save
  /// chunk.</typeparam>
  TDataType GetChunkSaveData<TDataType>() where TDataType : class;

  /// <summary>
  /// Loads the data associated with a child save chunk.
  /// </summary>
  /// <typeparam name="TDataType">Type of data associated with the child save
  /// chunk.</typeparam>
  void LoadChunkSaveData<TDataType>(TDataType data) where TDataType : class;
}

/// <inheritdoc cref="ISaveChunk{TData}"/>
public sealed class SaveChunk<TData> : ISaveChunk<TData> where TData : class
{
  /// <inheritdoc cref="ISaveChunk{TData}.OnSave"/>
  public Func<ISaveChunk<TData>, TData> OnSave { get; }

  /// <inheritdoc cref="ISaveChunk{TData}.OnLoad"/>
  public Action<ISaveChunk<TData>, TData> OnLoad { get; }
  private readonly Blackboard _children = new();

  /// <summary>
  /// Creates a new save chunk.
  /// </summary>
  /// <param name="onSave">Function to get the save data associated
  /// with this chunk. Should receive the current chunk as a parameter and
  /// return save data.</param>
  /// <param name="onLoad">Function to load the save data associated
  /// with this chunk. Should receive the current chunk and the data as
  /// parameters.</param>
  public SaveChunk(
    Func<ISaveChunk<TData>, TData> onSave,
    Action<ISaveChunk<TData>, TData> onLoad
  )
  {
    OnSave = onSave;
    OnLoad = onLoad;
  }

  /// <inheritdoc />
  public TData GetSaveData() => OnSave(this);

  /// <inheritdoc />
  public void LoadSaveData(TData data) => OnLoad(this, data);

  /// <inheritdoc />
  public void AddChunk<TDataType>(ISaveChunk<TDataType> child)
    where TDataType : class => _children.Set(child);

  /// <inheritdoc />
  public void OverwriteChunk<TDataType>(ISaveChunk<TDataType> child)
    where TDataType : class => _children.Overwrite(child);

  /// <inheritdoc />
  public ISaveChunk<TDataType> GetChunk<TDataType>() where TDataType : class =>
    _children.Get<ISaveChunk<TDataType>>();

  /// <inheritdoc />
  public TDataType GetChunkSaveData<TDataType>() where TDataType : class =>
    GetChunk<TDataType>().GetSaveData();

  /// <inheritdoc />
  public void LoadChunkSaveData<TDataType>(TDataType data)
    where TDataType : class => GetChunk<TDataType>().LoadSaveData(data);
}
