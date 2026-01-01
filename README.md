# 👽 SaveFileBuilder

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord][discord-badge]][discord] [![Read the docs][read-the-docs-badge]][docs] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

Compose chunks of save data into a single data type by creating loosely coupled save chunks at various points in your application.

<p align="center">
<img alt="Chickensoft.SaveFileBuilder" src="Chickensoft.SaveFileBuilder/icon.png" width="200">
</p>

## 🥚 Installation

Find the latest version of [`Chickensoft.SaveFileBuilder`][nuget] on nuget.

```sh
dotnet add package Chickensoft.SaveFileBuilder
```

## :hatching_chick: Quick Start

```csharp
// Define your (serializable!) save data
public class UserData
{
  public string Name { get; set; }
  public DateTime Birthday { get; set; }
}

// Define your class responsible for saving and loading.
public class User
{
  public string Name { get; set; }
  public string Birthday { get; set; }

  public SaveFile<UserData> SaveFile { get; }
  public ISaveChunk<UserData> SaveChunk { get; }

  public User()
  {
    // Define your saving and loading behavior at the start, and never again!
    SaveChunk = new SaveChunk<UserData>(
      onSave: (chunk) => new UserData()
      {
        Name = Name,
        Birthday = Birthday
      },
      onLoad: (chunk, data) =>
      {
        Name = data.Name;
        Birthday = data.Birthday;
      }
    );

    // Let SaveFile take care of the rest.
    SaveFile = SaveFile.CreateGZipJsonFile(SaveChunk, "savefile.json.gz");
  }

  public Task OnSave() => SaveFile.SaveAsync();
  public Task OnLoad() => SaveFile.LoadAsync();
}
```

> [!TIP]
> You can define easily serializable types with [Chickensoft.Serialization].

## 🍪 Save Chunks & Modularity

SaveChunks are smaller pieces of save data that are composed together into the overall save file.

```csharp
// User data contains preferences data separately.
public class UserData
{
  public string Name { get; set; }
  public DateTime Birthday { get; set; }
  public PreferencesData Preferences { get; set; }
}

// This allows us to keep our save data and -logic modular.
public class PreferencesData
{
  public bool IsDarkMode { get; set; }
  public string Language { get; set; }
}
```

This modularity allows us to separate concerns when saving and loading data. The `User` class is only concerned with user data, while the `UserPreferences` class is only concerned with preferences data.

We can link our save chunks together using:
- `GetChunkSaveData` to retrieve child chunk data during save.
- `LoadChunkSaveData` to load child chunk data during load.
- `AddChunk` to compose our save data.

```csharp
// Handle user logic.
public class User
{
  public string Name { get; set; }
  public DateTime Birthday { get; set; }

  public ISaveChunk<UserData> SaveChunk { get; }

  public User()
  {
    // Define our user chunk with a nested preferences chunk.
    SaveChunk = new SaveChunk<UserData>(
      onSave: (chunk) => new UserData()
      {
        Name = Name,
        Birthday = Birthday,
        Preferences = chunk.GetChunkSaveData<PreferencesData>()
      },
      onLoad: (chunk, data) =>
      {
        Name = data.Name;
        Birthday = data.Birthday;
        chunk.LoadChunkSaveData(data.Preferences);
      }
    );
  }
}

// Handle preferences logic.
public class UserPreferences
{
  public bool IsDarkMode { get; set; }
  public string Language { get; set; }

  public ISaveChunk<PreferencesData> SaveChunk { get; }

  public UserPreferences(User user)
  {
    // Define our preferences chunk.
    SaveChunk = new SaveChunk<PreferencesData>(
      onSave: (chunk) => new PreferencesData()
      {
        IsDarkMode = IsDarkMode,
        Language = Language
      },
      onLoad: (chunk, data) =>
      {
        IsDarkMode = data.IsDarkMode;
        Language = data.Language;
      }
    );

    // Add our preferences chunk as a child of the user chunk.
    user.SaveChunk.AddChunk(SaveChunk);
  }
}
```

## :floppy_disk: SaveFile & Flexibility

> [!TIP]
> If you just want to save some data to a file, call the following: `SaveFile.CreateGZipJsonFile(Root, "savefile.json.gz");`

Saving a file involves 2 to 3 steps:
- input / output (io)
- (preferably) compression
- serialization

SaveFile handles these steps for you, and optimally at that! By using [Streams] under the hood, SaveFile can efficiently save and load data without unnecessary memory allocations.

But the :zap: REAL POWER :zap: of SaveFile comes from its flexibility. You can define your own IO providers, compression algorithms, and serialization formats by implementing the relevant interfaces:
- IStreamIO / IAsyncStreamIO for io
- IStreamCompressor for compression
- IStreamSerializer / IAsyncStreamSerializer for serialization

```csharp
public class AzureStreamIO : IAsyncIOStreamProvider
{
  public Stream ReadAsync() => //...
  public void WriteAsync(Stream stream) => //...
  public bool ExistsAsync() => //...
  public bool DeleteAsync() => //...
}

public class SnappyStreamCompressor : IStreamCompressor
{
  public Stream Compress(Stream stream, CompressionLevel compressionLevel, bool leaveOpen) => //...
  public Stream Decompress(Stream stream, bool leaveOpen) => //...
}

public class YamlStreamSerializer : IStreamSerializer
{
  public void Serialize(Stream stream, object? value, Type inputType) => //...
  public object? Deserialize(Stream stream, Type returnType) => //...
}
```

You can then provide them to your SaveFile and mix- and match them with existing types.

```csharp
public class App
{
  SaveFile<AzureData> AzureSaveFile { get; set; }
  SaveFile<LocalData> LocalSaveFile { get; set; }

  public void Save()
  {
    // Define a SaveChunk<AzureData> AzureChunk
    // Define a SaveChunk<LocalData> LocalChunk

    AzureSaveFile = new
    (
      root: AzureChunk, 
      asyncIO: new AzureStreamIO(), 
      serializer: new JsonStreamSerializer(), 
      compressor: new SnappyStreamCompressor()
    );

    LocalSaveFile = new
    (
      root: LocalChunk, 
      io: new FileStreamIO(), 
      serializer: new YamlStreamSerializer(), 
      compressor: new BrotliStreamCompressor()
    );
  }
}
```

> [!NOTE]
> If you write your own implementations of these interfaces, consider contributing them back to the Chickensoft community by opening a PR!

## <img src="Chickensoft.SaveFileBuilder/godot-icon.png" width="24" /> Usage in Godot

Using [Introspection] and [AutoInject], you can link chunks together in Godot by providing- and accessing dependencies in your scene tree. Mark the relevant nodes as `IAutoNode`'s, provide dependencies from parent nodes, and access them in child nodes. 

```csharp
using Chickensoft.Introspection;
using Chickensoft.AutoInject;
using Chickensoft.SaveFileBuilder;
using Godot;

// Game is the root node in the scene. It provides the dependency to descendant nodes.
[Meta(typeof(IAutoNode))]
public partial class Game : Node3D
{
  public SaveFile<GameData> SaveFile { get; set; } = default!;

  // Provide the root save chunk to all descendant nodes.
  ISaveChunk<GameData> IProvide<ISaveChunk<GameData>>.Value() => SaveFile.Root;

  public void Setup()
  {
    var root = new SaveChunk<GameData>(onSave: ..., onLoad: ...);
    SaveFile = SaveFile.CreateGZipJsonFile(root, SaveFilePath, JsonOptions);
  }
}

// Player is a child node of the Game node. It accesses the dependency provided by the Game class.
[Meta(typeof(IAutoNode))]
public partial class Player : CharacterBody3D
{
  [Dependency]
  public ISaveChunk<GameData> GameChunk => this.DependOn<ISaveChunk<GameData>>();
  public ISaveChunk<PlayerData> PlayerChunk { get; set; } = default!;

  // Player uses a StateMachine, or LogicBlock, to handle its state.
  public IPlayerLogic PlayerLogic { get; set; } = default!;

  public void Setup()
  {
    PlayerLogic = new PlayerLogic();

    PlayerChunk = new SaveChunk<PlayerData>(
      onSave: (chunk) => new PlayerData()
      {
        GlobalTransform = GlobalTransform,
        StateMachine = PlayerLogic,
        Velocity = Velocity
      },
      onLoad: (chunk, data) =>
      {
        GlobalTransform = data.GlobalTransform;
        Velocity = data.Velocity;
        PlayerLogic.RestoreFrom(data.StateMachine);
        PlayerLogic.Start();
      }
    );
  }

  public void OnResolved()
  {
    // Add a child to our parent save chunk (the game chunk) so that it can
    // look up the player chunk when loading and saving the game.
    GameChunk.AddChunk(PlayerChunk);
  }
}
```

> [!TIP]
> You can easily serialize entire [LogicBlocks] with [Chickensoft.Serialization].

> [!TIP]
> Check out the Chickensoft [Game Demo] for a complete, working example of using SaveFileBuilder to save composed states of everything that needs to be persisted in a game.

---

🐣 Package generated from a 🐤 Chickensoft Template — <https://chickensoft.games>

[chickensoft-badge]: https://chickensoft.games/img/badges/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord-badge]: https://chickensoft.games/img/badges/discord_badge.svg
[discord]: https://discord.gg/gSjaPgMmYW
[read-the-docs-badge]: https://chickensoft.games/img/badges/read_the_docs_badge.svg
[docs]: https://chickensoft.games/docs
[line-coverage]: Chickensoft.SaveFileBuilder.Tests/badges/line_coverage.svg
[branch-coverage]: Chickensoft.SaveFileBuilder.Tests/badges/branch_coverage.svg

[Introspection]: https://github.com/chickensoft-games/Introspection
[AutoInject]: https://github.com/chickensoft-games/AutoInject
[Game Demo]: https://github.com/chickensoft-games/GameDemo
[LogicBlocks]: https://github.com/chickensoft-games/LogicBlocks
[Chickensoft.Serialization]: https://github.com/chickensoft-games/Serialization
[nuget]: https://www.nuget.org/packages/Chickensoft.SaveFileBuilder

[Streams]: https://learn.microsoft.com/en-us/dotnet/api/system.io.stream]
