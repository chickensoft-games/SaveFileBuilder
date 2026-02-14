namespace Chickensoft.SaveFileBuilder.Tests;

using System.Text.Json;
using Chickensoft.SaveFileBuilder;
using Chickensoft.SaveFileBuilder.IO;

public partial class SaveFileFactoryTest
{
  private CancellationToken CancellationToken { get; }

  private Mock<ISaveChunk<TestData>> MockChunk { get; }
  private const string FILE_PATH = "test_save.dat";

  public SaveFileFactoryTest(ITestContextAccessor testContextAccessor)
  {
    CancellationToken = testContextAccessor.Current.CancellationToken;
    MockChunk = new Mock<ISaveChunk<TestData>>();
  }

  [Fact]
  public void CreateGZipJsonFile_WithOptions_CreatesValidInstance()
  {
    // Arrange
    var options = new JsonSerializerOptions { WriteIndented = true };

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(MockChunk.Object, FILE_PATH, options);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.True(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonFile_WithNullOptions_CreatesValidInstance()
  {
    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(MockChunk.Object, FILE_PATH);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.True(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonFile_WithContext_CreatesValidInstance()
  {
    // Arrange
    var context = TestJsonContext.Default;

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(MockChunk.Object, FILE_PATH, context);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.True(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonFile_WithJsonTypeInfo_CreatesValidInstance()
  {
    // Arrange
    var typeInfo = TestJsonContext.Default.TestData;

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(MockChunk.Object, FILE_PATH, typeInfo);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.True(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndOptions_CreatesValidInstance()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var options = new JsonSerializerOptions { WriteIndented = true };

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(MockChunk.Object, mockIO.Object, options);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.True(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndNullOptions_CreatesValidInstance()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(MockChunk.Object, mockIO.Object);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.True(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndContext_CreatesValidInstance()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var context = TestJsonContext.Default;

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(MockChunk.Object, mockIO.Object, context);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.True(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndJsonTypeInfo_CreatesValidInstance()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var typeInfo = TestJsonContext.Default.TestData;

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(MockChunk.Object, mockIO.Object, typeInfo);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.True(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIAsyncStreamIOAndOptions_CreatesValidInstance()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var options = new JsonSerializerOptions { WriteIndented = true };

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(MockChunk.Object, mockAsyncIO.Object, options);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.False(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIAsyncStreamIOAndNullOptions_CreatesValidInstance()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(MockChunk.Object, mockAsyncIO.Object);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.False(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonIO_WithIAsyncStreamIOAndContext_CreatesValidInstance()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var context = TestJsonContext.Default;

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(MockChunk.Object, mockAsyncIO.Object, context);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.False(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonFIO_WithIAsyncStreamIOAndJsonTypeInfo_CreatesValidInstance()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var typeInfo = TestJsonContext.Default.TestData;

    // Act
    var saveFile = SaveFile.CreateGZipJsonFIO(MockChunk.Object, mockAsyncIO.Object, typeInfo);

    // Assert
    Assert.NotNull(saveFile);
    Assert.NotNull(saveFile.Root);
    Assert.Equal(MockChunk.Object, saveFile.Root);
    Assert.False(saveFile.CanSaveSynchronously);
  }

  [Fact]
  public void CreateGZipJsonFile_IntegrationTest_CanSaveAndLoad()
  {
    // Arrange
    var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dat");
    var data = new TestData { Name = "Hello, World!", Value = 42 };
    var chunk = new SaveChunk<TestData>(
      onSave: _ => data,
      onLoad: (_, loadedData) =>
      {
        data.Name = loadedData.Name;
        data.Value = loadedData.Value;
      }
    );

    try
    {
      var saveFile = SaveFile.CreateGZipJsonFile(chunk, tempFile);

      // Act - Save
      saveFile.Save();

      // Assert - File exists
      Assert.True(File.Exists(tempFile));

      // Act - Load
      data.Name = "Modified";
      data.Value = 0;
      saveFile.Load();

      // Assert - Data restored
      Assert.Equal("Hello, World!", data.Name);
      Assert.Equal(42, data.Value);
    }
    finally
    {
      // Cleanup
      if (File.Exists(tempFile))
      {
        File.Delete(tempFile);
      }
    }
  }

  [Fact]
  public async Task CreateGZipJsonFile_IntegrationTest_CanSaveAndLoadAsync()
  {
    // Arrange
    var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dat");
    var data = new TestData { Name = "Async Test", Value = 99 };
    var chunk = new SaveChunk<TestData>(
      onSave: _ => data,
      onLoad: (_, loadedData) =>
      {
        data.Name = loadedData.Name;
        data.Value = loadedData.Value;
      }
    );

    try
    {
      var saveFile = SaveFile.CreateGZipJsonFile(chunk, tempFile);

      // Act - Save
      await saveFile.SaveAsync(cancellationToken: CancellationToken);

      // Assert - File exists
      Assert.True(await saveFile.ExistsAsync(CancellationToken));

      // Act - Load
      data.Name = "Modified";
      data.Value = 0;
      await saveFile.LoadAsync(CancellationToken);

      // Assert - Data restored
      Assert.Equal("Async Test", data.Name);
      Assert.Equal(99, data.Value);

      // Act - Delete
      var deleted = await saveFile.DeleteAsync(CancellationToken);

      // Assert - File deleted
      Assert.True(deleted);
      Assert.False(await saveFile.ExistsAsync(CancellationToken));
    }
    finally
    {
      // Cleanup
      if (File.Exists(tempFile))
      {
        File.Delete(tempFile);
      }
    }
  }
}
