# 👽 SaveFileBuilder

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord][discord-badge]][discord] [![Read the docs][read-the-docs-badge]][docs] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

Compose chunks of save data into a single data type by creating loosely coupled save chunks at various points in your application, and modularly configure your saving method with plug-and-play support for saving to a file using json and gzip.

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
// MUST provide an empty constructor!
public class UserData
{
  public string Name { get; set; }
  public DateTime Birthday { get; set; }
}

// Define your class responsible for saving and loading.
public sealed class User : IDisposable
{
  public string Name { get; set; }
  public DateTime Birthday { get; set; }

  private SaveFile _saveFile = SaveFile.CreateGZipJsonFile("savefile.json.gz");
  private SaveChunk<UserData> _userChunk = new();
  private ISaveChunk<UserData>.Binding _userBinding;

  public User()
  {
    // Define your saving and loading behavior at the start, and never again!
    _userBinding = _userChunk.Bind()
      .OnSave(data => {
        data.Name = Name;
        data.Birthday = Birthday;
      })
      .OnLoad(data => {
        Name = data.Name;
        Birthday = data.Birthday;
      });
  }

  // Let SaveFile take care of the rest.
  public Task OnSave() => SaveFile.SaveAsync(_userChunk.Save());
  public async Task OnLoad() => _userChunk.Load(await SaveFile.LoadAsync<UserData>());

  // Dispose your saving and loading behavior and keep your save file clean.
  public void Dispose() => _userBinding.Dispose();
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

We can link our save chunks by exposing our `ISaveChunk<UserData>` and defining extra save functionality for our `User` in our `UserPreferences`.

```csharp
// Handle user logic.
public sealed class User : IDisposable
{
  public string Name { get; set; }
  public DateTime Birthday { get; set; }

  // Publically expose our save chunk.
  public ISaveChunk<UserData> UserChunk => _userChunk;

  private SaveChunk<UserData> _userChunk = new();
  private ISaveChunk<UserData>.Binding _userBinding;

  public User()
  {
    // Define our user chunk, but leave preferences empty.
    _userBinding = _userChunk.Bind()
      .OnSave(data => {
        data.Name = Name;
        data.Birthday = Birthday;
      })
      .OnLoad(data => {
        Name = data.Name;
        Birthday = data.Birthday;
      });
  }

  public void Dispose() => _userBinding.Dispose();
}

// Handle preferences logic.
public sealed class UserPreferences : IDisposable
{
  public bool IsDarkMode { get; set; }
  public string Language { get; set; }

  public ISaveChunk<PreferencesData> PreferencesChunk => _preferencesChunk;

  private SaveChunk<PreferencesData> _preferencesChunk = new();
  private ISaveChunk<PreferencesData>.Binding _preferencesBinding;
  private ISaveChunk<UserData>.Binding _userBinding;

  public UserPreferences(User user)
  {
    // Define our preferences chunk.
    _preferencesBinding = _preferencesChunk.Bind()
      .OnSave(data => {
        data.IsDarkMode = IsDarkMode;
        data.Language = Language;
      })
      .OnLoad(data => {
        IsDarkMode = data.IsDarkMode;
        Language = data.Language;
      });

    // Define how our user saves our preferences.
    _userBinding = user.UserChunk.Bind()
      .OnSave(data => data.Preferences = _preferencesChunk.Save())
      .OnLoad(data => _preferencesChunk.Load(data.Preferences));
  }

  public void Dispose()
  {
    _preferencesBinding.Dispose();
    _userBinding.Dispose();
  }
}
```

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
  // Provide the root save chunk to all descendant nodes.
  ISaveChunk<GameData> IProvide<ISaveChunk<GameData>>.Value() => _gameChunk;

  private SaveFile _saveFile = SaveFile.CreateGZipJsonFile("savefile.json.gz");
  private SaveChunk<GameData> _gameChunk = new();

  public Task OnSave() => _saveFile.SaveAsync(_gameChunk.Save());
  public async Task OnLoad() => _gameChunk.Load(_saveFile.LoadAsync<GameData>());
}

// Player is a child node of the Game node. It accesses the dependency provided by the Game class.
[Meta(typeof(IAutoNode))]
public partial class Player : CharacterBody3D
{
  [Dependency]
  public ISaveChunk<GameData> GameChunk => this.DependOn<ISaveChunk<GameData>>();

  private SaveChunk<PlayerData> _playerChunk = new();

  // Player uses a StateMachine, or LogicBlock, to handle its state.
  private PlayerLogic _playerLogic = new();

  // Utility class for collecting disposables.
  private CompositeDisposable _disposal = new();

  public void Setup()
  {
    _playerChunk.Bind()
      .OnSave(data => {
        data.GlobalTransform = GlobalTransform,
        data.StateMachine = _playerLogic,
        data.Velocity = Velocity
      })
      .OnLoad(data => {
        GlobalTransform = data.GlobalTransform;
        Velocity = data.Velocity;
        _playerLogic.RestoreFrom(data.StateMachine);
        _playerLogic.Start();
      })
      .DisposeWith(_disposal);
  }

  public void OnResolved()
  {
    GameChunk.Bind()
      .OnSave(data => data.Player = _playerChunk.Save())
      .OnLoad(data => _playerChunk.Load(data.Player))
      .DisposeWith(_disposal);

    _playerLogic.Start();
  }

  public void OnExitTree()
  {
    _playerLogic.Stop();
    _disposal.Dispose();
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
