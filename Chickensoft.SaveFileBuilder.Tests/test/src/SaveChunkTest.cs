namespace Chickensoft.SaveFileBuilder.Tests;

public class SaveChunkTest
{
  public SaveChunk<TestData> Chunk { get; set; }

  public SaveChunkTest()
  {
    Chunk = new SaveChunk<TestData>();
  }

  [Fact]
  public void Constructor_CreatesInstance()
  {
    var chunk = new SaveChunk<TestData>();
    Assert.NotNull(chunk);
  }

  [Fact]
  public void Save_ReturnsNewDataInstance()
  {
    var data = Chunk.Save();
    Assert.NotNull(data);
    Assert.IsType<TestData>(data);
  }

  [Fact]
  public void Save_InvokesOnSaveCallback()
  {
    // Arrange
    TestData? savedData = null;
    Chunk.Bind().OnSave(data => savedData = data);

    // Act
    var result = Chunk.Save();

    // Assert
    Assert.NotNull(savedData);
    Assert.Same(result, savedData);
  }

  [Fact]
  public void Save_DataModifiedInCallback_ReflectedInReturnValue()
  {
    // Arrange
    Chunk.Bind().OnSave(data => data.Name = "modified");

    // Act
    var result = Chunk.Save();

    // Assert
    Assert.Equal("modified", result.Name);
  }

  [Fact]
  public void Load_InvokesOnLoadCallback()
  {
    // Arrange
    TestData? loadedData = null;
    Chunk.Bind().OnLoad(data => loadedData = data);

    var dataToLoad = new TestData { Name = "test", Value = 42 };

    // Act
    Chunk.Load(dataToLoad);

    // Assert
    Assert.NotNull(loadedData);
    Assert.Same(dataToLoad, loadedData);
  }

  [Fact]
  public void Bind_ReturnsBinding()
  {
    var binding = Chunk.Bind();
    Assert.NotNull(binding);
  }

  [Fact]
  public void ClearBindings_StopsOnSaveCallbackFromBeingInvoked()
  {
    // Arrange
    var callCount = 0;
    Chunk.Bind().OnSave(_ => callCount++);

    // Act
    Chunk.ClearBindings();
    Chunk.Save();

    // Assert
    Assert.Equal(0, callCount);
  }

  [Fact]
  public void ClearBindings_StopsOnLoadCallbackFromBeingInvoked()
  {
    // Arrange
    var callCount = 0;
    Chunk.Bind().OnLoad(_ => callCount++);

    // Act
    Chunk.ClearBindings();
    Chunk.Load(new TestData());

    // Assert
    Assert.Equal(0, callCount);
  }

  [Fact]
  public void Dispose_DoesNotThrow()
  {
    var exception = Record.Exception(Chunk.Dispose);
    Assert.Null(exception);
  }

  [Fact]
  public void OnSave_WithTrueCondition_InvokesCallback()
  {
    // Arrange
    var callCount = 0;
    Chunk.Bind().OnSave(_ => callCount++, _ => true);

    // Act
    Chunk.Save();

    // Assert
    Assert.Equal(1, callCount);
  }

  [Fact]
  public void OnSave_WithFalseCondition_DoesNotInvokeCallback()
  {
    // Arrange
    var callCount = 0;
    Chunk.Bind().OnSave(_ => callCount++, _ => false);

    // Act
    Chunk.Save();

    // Assert
    Assert.Equal(0, callCount);
  }

  [Fact]
  public void OnLoad_WithTrueCondition_InvokesCallback()
  {
    // Arrange
    var callCount = 0;
    Chunk.Bind().OnLoad(_ => callCount++, _ => true);

    // Act
    Chunk.Load(new TestData());

    // Assert
    Assert.Equal(1, callCount);
  }

  [Fact]
  public void OnLoad_WithFalseCondition_DoesNotInvokeCallback()
  {
    // Arrange
    var callCount = 0;
    Chunk.Bind().OnLoad(_ => callCount++, _ => false);

    // Act
    Chunk.Load(new TestData());

    // Assert
    Assert.Equal(0, callCount);
  }

  [Fact]
  public void OnSave_ReturnsBinding_ForMethodChaining()
  {
    // Arrange
    var binding = Chunk.Bind();

    // Act
    var result = binding.OnSave(_ => { });

    // Assert
    Assert.Same(binding, result);
  }

  [Fact]
  public void OnLoad_ReturnsBinding_ForMethodChaining()
  {
    // Arrange
    var binding = Chunk.Bind();

    // Act
    var result = binding.OnLoad(_ => { });

    // Assert
    Assert.Same(binding, result);
  }

  [Fact]
  public void MultipleBindings_AllOnSaveCallbacksInvoked()
  {
    // Arrange
    var callCount = 0;
    Chunk.Bind().OnSave(_ => callCount++);
    Chunk.Bind().OnSave(_ => callCount++);

    // Act
    Chunk.Save();

    // Assert
    Assert.Equal(2, callCount);
  }

  [Fact]
  public void MultipleBindings_AllOnLoadCallbacksInvoked()
  {
    // Arrange
    var callCount = 0;
    Chunk.Bind().OnLoad(_ => callCount++);
    Chunk.Bind().OnLoad(_ => callCount++);

    // Act
    Chunk.Load(new TestData());

    // Assert
    Assert.Equal(2, callCount);
  }
}
