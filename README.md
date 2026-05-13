# 👽 SaveFileBuilder

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord][discord-badge]][discord] [![Read the docs][read-the-docs-badge]][docs] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

Easily define your own save file with custom serialization formats, compression algorithms, and IO providers by implementing the relevant interfaces. Out-of-the-box support for saving to a file using json and gzip.

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
// Define your (serializable!) save data.
public record UserData
{
  public required string Name { get; init; }
  public required DateTime Birthday { get; init; }
}

// Define your class responsible for saving and loading.
public sealed class User : ISaveable<UserData>
{
  public string Name { get; set; }
  public DateTime Birthday { get; set; }

  // Define your saving and loading behavior through the interfaces.
  public UserData Save() => new() 
  { 
    Name = this.Name, 
    Birthday = this.Birthday 
  };

  public void Load(in UserData data)
  {
    this.Name = data.Name;
    this.Birthday = data.Birthday;

    // Call additional loading logic
    UpdateUI();
  }

  // Let SaveFile take care of the rest.
  private SaveFile _saveFile = SaveFile.CreateGZipJsonFile("savefile.json.gz");

  public ValueTask OnSave() => _saveFile.SaveAsync(Save());
  public async ValueTask OnLoad() => Load(in (await _saveFile.LoadAsync<UserData>()));
}
```

> [!TIP]
> You can define easily serializable types with [Chickensoft.Serialization].

## :cookie: Saveable & Modularity

Saveables define how an object saves and loads itself. They are a minimal interface that only requires you to implement `Save` and `Load` methods. This allows you to keep your save logic close to the relevant data and behavior, and easily compose them together.

To compose saveables, simply call `Save` and `Load` down the stack of saveables to fully populate your root data object.

```csharp
public record UserData
{
  public required PreferencesData Preferences { get; init; }
}

public record PreferencesData
{
  public required bool IsDarkMode { get; init; }
  public required string Language { get; init; }
}

public class User : ISaveable<UserData>
{
  // Reference a Preferences class and calls its 
  // Save and Load methods to compose the data.
  public Preferences Preferences { get; }

  public UserData Save() => new() 
  { 
    Preferences = Preferences.Save(),
  };

  public void Load(in UserData data)
  {
    Preferences.Load(in data.Preferences);
  }
}

public class Preferences : ISaveable<PreferencesData>
{
  public bool IsDarkMode { get; set; }
  public string Language { get; set; }

  // Its Save and Load methods are called by 
  // the User class to compose the data.
  public PreferencesData Save() => new() 
  { 
    IsDarkMode = this.IsDarkMode, 
    Language = this.Language 
  };

  public void Load(in PreferencesData data)
  {
    this.IsDarkMode = data.IsDarkMode;
    this.Language = data.Language;
  }
}
```

> [!NOTE]
> SaveFileBuilder does not help you reference your saveables. How you reference a saveable is up to you: it could be owned, be provided through a constructor, be available through a static instance or be injected using dependency injection.

## :floppy_disk: SaveFile & Flexibility

> [!TIP]
> If you just want to save some data to a file, call the following: `SaveFile.CreateGZipJsonFile("savefile.json.gz");`

Saving a file involves 2 to 3 steps:
- input / output (io)
- serialization
- (preferably) compression

SaveFile handles these steps for you, and optimally at that! By using [Streams] under the hood, SaveFile can efficiently save and load data without unnecessary memory allocations.

But the :zap: REAL POWER :zap: of SaveFile comes from its flexibility. You can define your own IO providers, compression algorithms, and serialization formats by implementing the relevant interfaces:
- IStreamIO / IAsyncStreamIO for io
- IStreamSerializer / IAsyncStreamSerializer for serialization
- IStreamCompressor for compression

```csharp
public class AzureStreamIO : IAsyncStreamIO
{
  public Stream ReadAsync() => //...
  public void WriteAsync(Stream stream) => //...
  public bool ExistsAsync() => //...
  public bool DeleteAsync() => //...
}

public class YamlStreamSerializer : IStreamSerializer
{
  public void Serialize(Stream stream, object? value, Type inputType) => //...
  public object? Deserialize(Stream stream, Type returnType) => //...
}

public class SnappyStreamCompressor : IStreamCompressor
{
  public Stream Compress(Stream stream, CompressionLevel compressionLevel, bool leaveOpen) => //...
  public Stream Decompress(Stream stream, bool leaveOpen) => //...
}
```

You can then provide them to your SaveFile and mix- and match them with existing types.

```csharp
public class App
{
  // Save to Azure using Json and Snappy
  SaveFile AzureSaveFile { get; } = new(
    asyncIO: new AzureStreamIO(), 
    serializer: new JsonStreamSerializer(), 
    compressor: new SnappyStreamCompressor()
  );

  // Save to File using Yaml and Brotli
  SaveFile<LocalData> LocalSaveFile { get; } = new(
    io: new FileStreamIO(), 
    serializer: new YamlStreamSerializer(), 
    compressor: new BrotliStreamCompressor()
  );
}
```

> [!NOTE]
> If you write your own implementations of these interfaces, consider contributing them back to the Chickensoft community by opening a PR!

## <img src="Chickensoft.SaveFileBuilder/godot-icon.png" width="24" /> Usage in Godot

Using [Introspection] and [AutoInject], you can link saveables together in Godot by providing- and accessing dependencies in your scene tree. Mark the relevant nodes as `IAutoNode`'s and use the `[Node]` attribute to inject them into your saveable classes. Then, simply call `Save` and `Load` on your root node to save and load the entire game state.

```csharp
using Chickensoft.Introspection;
using Chickensoft.AutoInject;
using Chickensoft.SaveFileBuilder;
using Godot;

public interface IGameData : INode3D, ISaveable<GameData>;

// Game is the root node in the scene.
[Meta(typeof(IAutoNode))]
public partial class Game : Node3D, IGame
{
  // The Player node is a child of the Game node.
  [Node] public IPlayer Player { get; set; } = default!;

  // GameData is the root data object that contains all the data that needs to be saved.
  public GameData Save() => new() 
  { 
    Player = Player.Save() 
  };

  public void Load(in GameData data) => Player.Load(data.Player);

  // SaveFile handles the saving and loading of the game data.
  private SaveFile _saveFile = SaveFile.CreateGZipJsonFile("savefile.json.gz");
  
  public Task OnSave() => _saveFile.SaveAsync(Save());
  public async Task OnLoad() => Load(_saveFile.LoadAsync<GameData>());
}

public interface IPlayer : ICharacterBody3D, ISaveable<PlayerData>;

[Meta(typeof(IAutoNode))]
public partial class Player : CharacterBody3D, IPlayer
{
  // Player uses a StateMachine, or LogicBlock, to handle its state.
  private PlayerLogic _playerLogic = new();

  // PlayerData is the data object for the Player.
  public PlayerData Save() => new()
  {
    GlobalTransform = GlobalTransform,
    StateMachine = _playerLogic,
    Velocity = Velocity
  };

  public void Load(in PlayerData data)
  {
    GlobalTransform = data.GlobalTransform;
    Velocity = data.Velocity;
    _playerLogic.RestoreFrom(data.StateMachine);
    _playerLogic.Start();
  }

  // Start and Stop our state machine.
  public void OnResolved()
  {
    _playerLogic.Start();
  }

  public void OnExitTree()
  {
    _playerLogic.Stop();
  }
}
```

If you need something more indirect, you can use an [EntityTable] to store and retrieve saveables by their unique identifiers. This allows you to save and load saveables that are not directly referenced in your scene tree.

```csharp
public interface IGameData : INode3D
  , ISaveable<GameData>
  , IProvide<EntityTable>;

[Meta(typeof(IAutoNode))]
public partial class Game : Node3D, IGame
{
  private EntityTable Saveables { get; } = new();

  EntityTable IProvide<EntityTable>.Value() => Saveables;

  public GameData Save() => new()
  {
    Player = EntityTable.Get<IPlayer>("player")?.Save() 
      ?? throw new InvalidOperationException("Player not found in EntityTable.")
  };

  public void Load(in GameData data)
  {
    var player = EntityTable.Get<IPlayer>("player")
      ?? throw new InvalidOperationException("Player not found in EntityTable.");
    player.Load(data.Player);
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
[EntityTable]: https://github.com/chickensoft-games/Collections#entitytable

[Streams]: https://learn.microsoft.com/en-us/dotnet/api/system.io.stream]
