namespace Chickensoft.SaveFileBuilder.Tests;

using System.Text.Json;
using Chickensoft.SaveFileBuilder;
using Chickensoft.SaveFileBuilder.IO;

public partial class SaveFileFactoryTest
{
  private CancellationToken CancellationToken { get; }

  private const string FILE_PATH = "test_save.dat";

  public SaveFileFactoryTest(ITestContextAccessor testContextAccessor)
  {
    CancellationToken = testContextAccessor.Current.CancellationToken;
  }

  [Fact]
  public void CreateGZipJsonFile_WithOptions_CreatesValidInstance()
  {
    // Arrange
    var options = new JsonSerializerOptions { WriteIndented = true };

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(FILE_PATH, options);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeTrue();
  }

  [Fact]
  public void CreateGZipJsonFile_WithNullOptions_CreatesValidInstance()
  {
    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(FILE_PATH);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeTrue();
  }

  [Fact]
  public void CreateGZipJsonFile_WithContext_CreatesValidInstance()
  {
    // Arrange
    var context = TestJsonContext.Default;

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(FILE_PATH, context);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeTrue();
  }

  [Fact]
  public void CreateGZipJsonFile_WithJsonTypeInfo_CreatesValidInstance()
  {
    // Arrange
    var typeInfo = TestJsonContext.Default.TestData;

    // Act
    var saveFile = SaveFile.CreateGZipJsonFile(FILE_PATH, typeInfo);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeTrue();
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndOptions_CreatesValidInstance()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var options = new JsonSerializerOptions { WriteIndented = true };

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object, options);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeTrue();
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndNullOptions_CreatesValidInstance()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeTrue();
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndContext_CreatesValidInstance()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var context = TestJsonContext.Default;

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object, context);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeTrue();
  }

  [Fact]
  public void CreateGZipJsonIO_WithIStreamIOAndJsonTypeInfo_CreatesValidInstance()
  {
    // Arrange
    var mockIO = new Mock<IStreamIO>();
    var typeInfo = TestJsonContext.Default.TestData;

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockIO.Object, typeInfo);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeTrue();
  }

  [Fact]
  public void CreateGZipJsonIO_WithIAsyncStreamIOAndOptions_CreatesValidInstance()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var options = new JsonSerializerOptions { WriteIndented = true };

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object, options);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeFalse();
  }

  [Fact]
  public void CreateGZipJsonIO_WithIAsyncStreamIOAndNullOptions_CreatesValidInstance()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeFalse();
  }

  [Fact]
  public void CreateGZipJsonIO_WithIAsyncStreamIOAndContext_CreatesValidInstance()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var context = TestJsonContext.Default;

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object, context);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeFalse();
  }

  [Fact]
  public void CreateGZipJsonIO_WithIAsyncStreamIOAndJsonTypeInfo_CreatesValidInstance()
  {
    // Arrange
    var mockAsyncIO = new Mock<IAsyncStreamIO>();
    var typeInfo = TestJsonContext.Default.TestData;

    // Act
    var saveFile = SaveFile.CreateGZipJsonIO(mockAsyncIO.Object, typeInfo);

    // Assert
    saveFile.ShouldNotBeNull();
    saveFile.CanSaveSynchronously.ShouldBeFalse();
  }

  [Fact]
  public void CreateGZipJsonFile_IntegrationTest_CanSaveAndLoad()
  {
    // Arrange
    var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dat");
    var data = new TestData { Name = "Hello, World!", Value = 42 };

    try
    {
      var saveFile = SaveFile.CreateGZipJsonFile(tempFile);

      // Act - Save
      saveFile.Save(data);

      // Assert - File exists
      File.Exists(tempFile).ShouldBeTrue();

      // Act - Load
      data = new TestData { Name = "Modified", Value = 0 };
      data = saveFile.Load<TestData>();

      // Assert - Data restored
      data.ShouldNotBeNull();
      data.Name.ShouldBe("Hello, World!");
      data.Value.ShouldBe(42);
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

    try
    {
      var saveFile = SaveFile.CreateGZipJsonFile(tempFile);

      // Act - Save
      await saveFile.SaveAsync(data, cancellationToken: CancellationToken);

      // Assert - File exists
      (await saveFile.ExistsAsync(CancellationToken)).ShouldBeTrue();

      // Act - Load
      data = new TestData { Name = "Modified", Value = 0 };
      data = await saveFile.LoadAsync<TestData>(CancellationToken);

      // Assert - Data restored
      data.ShouldNotBeNull();
      data.Name.ShouldBe("Async Test");
      data.Value.ShouldBe(99);

      // Act - Delete
      var deleted = await saveFile.DeleteAsync(CancellationToken);

      // Assert - File deleted
      deleted.ShouldBeTrue();
      (await saveFile.ExistsAsync(CancellationToken)).ShouldBeFalse();
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
