namespace Chickensoft.SaveFileBuilder.Tests;

using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class SaveChunkTest(Node testScene) : TestClass(testScene)
{
  private sealed record SaveData { }

  [Test]
  public void SavesAndLoads()
  {
    var onSave = Task.CompletedTask;
    var data = new SaveData();

    var loaded = false;

    var saveChunk = new SaveChunk<SaveData>(
      onSave: (chunk) => data,
      onLoad: (chunk, data) => loaded = true
    );

    saveChunk.ShouldNotBeNull();

    saveChunk.GetSaveData().ShouldBeSameAs(data);
    saveChunk.LoadSaveData(data);
    loaded.ShouldBeTrue();
  }

  [Test]
  public void AddsAndGetsChunk()
  {
    var onSave = Task.CompletedTask;
    var data = new SaveData();

    var saveChunk = new SaveChunk<SaveData>(
      onSave: (chunk) => data,
      onLoad: (chunk, data) => { }
    );

    var childLoaded = false;
    var childData = new SaveData();
    var child = new SaveChunk<SaveData>(
      onSave: (chunk) => childData,
      onLoad: (chunk, data) => childLoaded = true
    );

    saveChunk.AddChunk(child);

    var childChunk = saveChunk.GetChunk<SaveData>();

    childChunk.ShouldBeSameAs(child);

    saveChunk.GetChunkSaveData<SaveData>().ShouldBeSameAs(childData);
    saveChunk.LoadChunkSaveData(childData);
    childLoaded.ShouldBeTrue();
  }

  [Test]
  public void OverwritesAndGetsChunk()
  {
    var onSave = Task.CompletedTask;
    var data = new SaveData();

    var saveChunk = new SaveChunk<SaveData>(
      onSave: (chunk) => data,
      onLoad: (chunk, data) => { }
    );

    var childData = new SaveData();
    var child = new SaveChunk<SaveData>(
      onSave: (chunk) => childData,
      onLoad: (chunk, data) => { }
    );

    var otherChildData = new SaveData();
    var otherChild = new SaveChunk<SaveData>(
      onSave: (chunk) => otherChildData,
      onLoad: (chunk, data) => { }
    );

    saveChunk.AddChunk(child);
    saveChunk.OverwriteChunk(otherChild);

    var childChunk = saveChunk.GetChunk<SaveData>();

    childChunk.ShouldNotBeSameAs(child);
    childChunk.ShouldBeSameAs(otherChild);

    saveChunk.GetChunkSaveData<SaveData>().ShouldNotBeSameAs(childData);
    saveChunk.GetChunkSaveData<SaveData>().ShouldBeSameAs(otherChildData);
  }
}
