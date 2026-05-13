namespace Chickensoft.SaveFileBuilder.Tests;

public class ISaveableTest
{
  private sealed class SaveableObject : ISaveable<TestData>
  {
    public string Name { get; private set; } = string.Empty;
    public int Value { get; private set; }

    public TestData Save() => new() { Name = Name, Value = Value };

    public void Load(in TestData data)
    {
      Name = data.Name;
      Value = data.Value;
    }
  }

  [Fact]
  public void Save_ReturnsCurrentState()
  {
    var obj = new SaveableObject();
    obj.Load(new TestData { Name = "Test", Value = 42 });

    var data = obj.Save();

    Assert.Equal("Test", data.Name);
    Assert.Equal(42, data.Value);
  }

  [Fact]
  public void Load_RestoresState()
  {
    var obj = new SaveableObject();
    var data = new TestData { Name = "Hello", Value = 7 };

    obj.Load(in data);

    Assert.Equal("Hello", obj.Name);
    Assert.Equal(7, obj.Value);
  }

  [Fact]
  public void SaveThenLoad_RoundTripsState()
  {
    var original = new SaveableObject();
    original.Load(new TestData { Name = "RoundTrip", Value = 99 });

    var saved = original.Save();

    var restored = new SaveableObject();
    restored.Load(in saved);

    Assert.Equal(original.Name, restored.Name);
    Assert.Equal(original.Value, restored.Value);
  }

  [Fact]
  public void Load_OverwritesPreviousState()
  {
    var obj = new SaveableObject();
    obj.Load(new TestData { Name = "First", Value = 1 });
    obj.Load(new TestData { Name = "Second", Value = 2 });

    Assert.Equal("Second", obj.Name);
    Assert.Equal(2, obj.Value);
  }
}
