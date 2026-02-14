namespace Chickensoft.SaveFileBuilder.Tests;

public class SaveChunkTest
{
  public string ChunkData { get; set; }
  public SaveChunk<string> Chunk { get; set; }
  public string ChildChunkData { get; set; }
  public SaveChunk<string> ChildChunk { get; set; }

  public SaveChunkTest()
  {
    ChunkData = string.Empty;
    Chunk = new SaveChunk<string>(
      onSave: (chunk) => ChunkData,
      onLoad: (chunk, data) => ChunkData = data
    );
    ChildChunkData = string.Empty;
    ChildChunk = new SaveChunk<string>(
      onSave: (chunk) => ChildChunkData,
      onLoad: (chunk, data) => ChildChunkData = data
      );
  }

  [Fact]
  public void GetSaveData_ReturnsChunkData()
  {
    ChunkData = "test";
    Assert.Equal("test", Chunk.GetSaveData());
  }

  [Fact]
  public void LoadSaveData_SetsChunkData()
  {
    Chunk.LoadSaveData("test");
    Assert.Equal("test", ChunkData);
  }

  [Fact]
  public void AddChunk_DoesNotThrow()
  {
    var exception = Record.Exception(() => Chunk.AddChunk(ChildChunk));
    Assert.Null(exception);
  }

  [Fact]
  public void GetChunk_ReturnsAddedChunk()
  {
    Chunk.AddChunk(ChildChunk);
    Assert.True(ReferenceEquals(ChildChunk, Chunk.GetChunk<string>()));
  }

  [Fact]
  public void GetChunkSaveData_ReturnsChildChunkData()
  {
    Chunk.AddChunk(ChildChunk);
    ChildChunkData = "child test";
    Assert.Equal("child test", Chunk.GetChunkSaveData<string>());
  }

  [Fact]
  public void LoadChunkSaveData_SetsChildChunkData()
  {
    Chunk.AddChunk(ChildChunk);
    Chunk.LoadChunkSaveData("child test");
    Assert.Equal("child test", ChildChunkData);
  }

  [Fact]
  public void AddDuplicateChunk_ThrowsException()
  {
    Chunk.AddChunk(ChildChunk);
    var exception = Record.Exception(() => Chunk.AddChunk(It.IsAny<ISaveChunk<string>>()));
    Assert.NotNull(exception);
  }

  [Fact]
  public void OverwriteChunk_WithoutExistingChunk_AddsChunk()
  {
    Chunk.OverwriteChunk(ChildChunk);
    Assert.True(ReferenceEquals(ChildChunk, Chunk.GetChunk<string>()));
  }

  [Fact]
  public void OverwriteChunk_WithExistingChunk_UpdatesExistingChunk()
  {
    var mockChunk = It.IsAny<ISaveChunk<string>>();

    Chunk.AddChunk(ChildChunk);
    Chunk.OverwriteChunk(mockChunk);

    Assert.True(ReferenceEquals(mockChunk, Chunk.GetChunk<string>()));
  }
}
