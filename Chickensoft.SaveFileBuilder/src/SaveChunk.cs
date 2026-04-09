namespace Chickensoft.SaveFileBuilder;

using System;
using Sync;
using Sync.Primitives;

/// <summary>Represents a save chunk for saving and loading one type of data.</summary>
/// <typeparam name="TData">Type of data associated with this save chunk.</typeparam>
/// <remarks>Save chunks can be stacked to form a tree composing different types of data.</remarks>
public interface ISaveChunk<TData> : IAutoObject<SaveChunk<TData>.Binding> where TData : new();

/// <inheritdoc cref="ISaveChunk{TData}"/>
public sealed partial class SaveChunk<TData> : ISaveChunk<TData> where TData : new()
{
  private readonly SyncSubject _subject;

  /// <inheritdoc cref="SaveChunk{TData}" />
  public SaveChunk()
  {
    _subject = new SyncSubject(this);
  }

  /// <summary>Broadcasts a save operation to all observers and returns the saved data.</summary>
  /// <returns>The data object that was broadcast and saved by observers.</returns>
  public TData Save()
  {
    var data = new TData();
    _subject.Broadcast(new SaveBroadcast(data));
    return data;
  }

  /// <summary>Broadcasts a load operation with the specified data to all observers.</summary>
  /// <param name="data">The loaded data to broadcast to observers.</param>
  public void Load(TData data) => _subject.Broadcast(new LoadBroadcast(data));

  /// <inheritdoc />
  public Binding Bind() => new(_subject);

  /// <inheritdoc />
  public void ClearBindings() => _subject.ClearBindings();

  /// <inheritdoc />
  public void Dispose() => _subject.Dispose();

  private readonly record struct SaveBroadcast(TData Value);
  private readonly record struct LoadBroadcast(TData Value);

  /// <summary>Represents a binding to a <see cref="SaveChunk{TData}"/> which can be used to register callbacks for save and load events.</summary>
  /// <param name="subject"><inheritdoc cref="SyncBinding._subject"/></param>
  public class Binding(ISyncSubject subject) : SyncBinding(subject)
  {
    /// <summary>Registers a callback to be invoked when a save operation occurs, optionally filtered by a specified condition.</summary>
    /// <param name="callback">The action to execute when a save event is broadcast. Receives the data to save as a parameter.</param>
    /// <param name="condition">An optional predicate that determines whether the callback should be invoked for a given data value. If <see langword="null"/>, the callback is always invoked.</param>
    /// <returns>The current binding instance, enabling method chaining.</returns>
    public Binding OnSave(Action<TData> callback, Predicate<TData>? condition = null)
    {
      bool predicate(TData value) => condition?.Invoke(value) ?? true;

      AddCallback<SaveBroadcast>(
        (in broadcast) => callback(broadcast.Value),
        (in broadcast) => predicate(broadcast.Value)
      );

      return this;
    }

    /// <summary>Registers a callback to be invoked when a load operation occurs, optionally filtered by a specified condition.</summary>
    /// <param name="callback">The action to execute when a load event is broadcast. Receives the loaded data as a parameter.</param>
    /// <param name="condition">An optional predicate that determines whether the callback should be invoked for a given data value. If <see langword="null"/>, the callback is always invoked.</param>
    /// <returns>The current binding instance, enabling method chaining.</returns>
    public Binding OnLoad(Action<TData> callback, Predicate<TData>? condition = null)
    {
      bool predicate(TData value) => condition?.Invoke(value) ?? true;

      AddCallback<LoadBroadcast>(
        (in broadcast) => callback(broadcast.Value),
        (in broadcast) => predicate(broadcast.Value)
      );

      return this;
    }
  }
}
