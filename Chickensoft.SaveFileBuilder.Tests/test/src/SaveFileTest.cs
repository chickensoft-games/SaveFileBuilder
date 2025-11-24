namespace Chickensoft.SaveFileBuilder.Tests;

using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Godot;

public class SaveFileTest(Node testScene) : TestClass(testScene)
{
  private sealed record SaveData { }

  [Test]
  public async Task SavesAndLoads()
  {
    //var onSave = Task.CompletedTask;
    //var data = new SaveData();

    //var saveFile = new SaveFile<SaveData>(
    //  root: new SaveChunk<SaveData>(
    //    onSave: (chunk) => new SaveData(),
    //    onLoad: (chunk, data) => { }
    //  ),
    //  onSave: _ => onSave,
    //  onLoad: () => Task.FromResult<SaveData?>(data)
    //);

    //await Should.NotThrowAsync(async () =>
    //{
    //  await saveFile.Load();
    //  await saveFile.Save();
    //});
  }

  [Test]
  public async Task DoesNotLoadIfNull()
  {
    //var onSave = Task.CompletedTask;
    //var data = new SaveData();

    //var saveFile = new SaveFile<SaveData>(
    //  root: new SaveChunk<SaveData>(
    //    onSave: (chunk) => new SaveData(),
    //    onLoad: (chunk, data) => { }
    //  ),
    //  onSave: _ => onSave,
    //  onLoad: () => Task.FromResult<SaveData?>(null)
    //);

    //await Should.NotThrowAsync(saveFile.Load);
  }
}
