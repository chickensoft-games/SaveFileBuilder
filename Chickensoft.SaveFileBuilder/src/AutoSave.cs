namespace Chickensoft.SaveFileBuilder;

using System;
using Sync;
using Sync.Primitives;

public interface IAutoSave<TData> : IAutoObject<AutoSave<TData>.Binding>
  where TData : new();

/// <inheritdoc cref="ISaveChunk{TData}"/>
public sealed class AutoSave<TData> : IAutoSave<TData> where TData : new()
{

  // Broadcasts
  private readonly record struct SaveBroadcast(TData Value);
  private readonly record struct LoadBroadcast(TData Value);

  public class Binding : SyncBinding
  {
    internal Binding(ISyncSubject subject) : base(subject) { }

    public Binding OnSave(
      Action<TData> callback, Func<TData, bool>? condition = null
    )
    {
      bool predicate(TData value) => condition?.Invoke(value) ?? true;

      AddCallback(
        (in SaveBroadcast broadcast) => callback(broadcast.Value),
        (in SaveBroadcast broadcast) => predicate(broadcast.Value)
      );

      return this;
    }

    public Binding OnLoad(
      Action<TData> callback, Func<TData, bool>? condition = null
    )
    {
      bool predicate(TData value) => condition?.Invoke(value) ?? true;

      AddCallback(
        (in LoadBroadcast broadcast) => callback(broadcast.Value),
        (in LoadBroadcast broadcast) => predicate(broadcast.Value)
      );

      return this;
    }
  }

  public TData GetSaveData()
  {
    var data = new TData();
    _subject.Broadcast(new SaveBroadcast(data));
    return data;
  }

  public void LoadSaveData(TData data) => _subject.Broadcast(new LoadBroadcast(data));

  private readonly SyncSubject _subject;

  public AutoSave()
  {
    _subject = new SyncSubject(this);
  }

  /// <inheritdoc />
  public Binding Bind() => new(_subject);

  /// <inheritdoc />
  public void ClearBindings() => _subject.ClearBindings();

  /// <inheritdoc />
  public void Dispose() => _subject.Dispose();
}
