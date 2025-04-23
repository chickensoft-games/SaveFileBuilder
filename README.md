# 👽 SaveFileBuilder

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord][discord-badge]][discord] [![Read the docs][read-the-docs-badge]][docs] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

Compose chunks of save data into a single data type by creating loosely coupled save chunks at various points in the scene tree.

---

<p align="center">
<img alt="Chickensoft.SaveFileBuilder" src="Chickensoft.SaveFileBuilder/icon.png" width="200">
</p>

## 🥚 Getting Started

Find the latest version of [`Chickensoft.SaveFileBuilder`][nuget] on nuget.

```sh
dotnet add package Chickensoft.SaveFileBuilder
```

## 📄 SaveFile and Root SaveChunk

Find the highest node in your scene tree that needs to be concerned with save data to use as the root of your save file. Use [AutoInject] to provide the root save chunk to all its descendant nodes.

> [!TIP]
> Check out the Chickensoft [Game Demo] for a complete, working example of using SaveFileBuilder to save composed states of everything that needs to be persisted in a game.

```csharp
using Chickensoft.Introspection;
using Chickensoft.AutoInject;
using Chickensoft.SaveFileBuilder;
using Godot;

[Meta(typeof(IAutoNode))]
public partial class Game : Node3D {
  public SaveFile<GameData> SaveFile { get; set; } = default!;

  // Provide the root save chunk to all descendant nodes.
  ISaveChunk<GameData> IProvide<ISaveChunk<GameData>>.Value() => SaveFile.Root;

  public void Setup() {
    SaveFile = new SaveFile<GameData>(
      root: new SaveChunk<GameData>(
        onSave: (chunk) => {
          // Use root chunk to get child chunks that were added to us
          // lower in the scene tree.
          var gameData = new GameData() {
            MapData = chunk.GetChunkSaveData<MapData>(),
            PlayerData = chunk.GetChunkSaveData<PlayerData>(),
            PlayerCameraData = chunk.GetChunkSaveData<PlayerCameraData>()
          };

          return gameData;
        },
        onLoad: (chunk, data) => {
          // Break up the game data and send it to the child chunks so that
          // they can load the data into the nodes they belong to.
          chunk.LoadChunkSaveData(data.MapData);
          chunk.LoadChunkSaveData(data.PlayerData);
          chunk.LoadChunkSaveData(data.PlayerCameraData);
        }
      ),
      onSave: async (GameData data) => {
        // Save the game data to disk.
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await FileSystem.File.WriteAllTextAsync(SaveFilePath, json);
      },
      onLoad: async () => {
        // Load the game data from disk.
        if (!FileSystem.File.Exists(SaveFilePath)) {
          GD.Print("No save file to load :'(");
          return null;
        }

        var json = await FileSystem.File.ReadAllTextAsync(SaveFilePath);
        return JsonSerializer.Deserialize<GameData>(json, JsonOptions);
      }
    );

    ...
  }
}
```

## 🍪 Defining Save Chunks

SaveChunks are smaller pieces of save data that are composed together into the overall save file's data. Simply add a chunk to a descendant node of the scene with the root SaveChunk and register it with the root save chunk once you've resolved dependencies with AutoInject.

```csharp
[Meta(typeof(IAutoNode))]
public partial class Player : CharacterBody3D {
  [Dependency]
  public ISaveChunk<GameData> GameChunk => this.DependOn<ISaveChunk<GameData>>();
  public ISaveChunk<PlayerData> PlayerChunk { get; set; } = default!;

  public void Setup() {
    ...

    PlayerChunk = new SaveChunk<PlayerData>(
      onSave: (chunk) => new PlayerData() {
        GlobalTransform = GlobalTransform,
        StateMachine = (PlayerLogic)PlayerLogic,
        Velocity = Velocity
      },
      onLoad: (chunk, data) => {
        GlobalTransform = data.GlobalTransform;
        Velocity = data.Velocity;
        PlayerLogic.RestoreFrom(data.StateMachine);
        PlayerLogic.Start();
      }
    );

    ...
  }

  public void OnResolved() {
    // Add a child to our parent save chunk (the game chunk) so that it can
    // look up the player chunk when loading and saving the game.
    GameChunk.AddChunk(PlayerChunk);

    ...
  }
}
```

Once a save chunk has been added to a parent save chunk, the parent save chunk can access it from the callbacks specified by `onSave` and `onLoad`, querying its data or forcing it load data into its node.

> [!TIP]
> You can define easily serializable types, as well as serialize entire [LogicBlocks] with [Chickensoft.Serialization].

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

[AutoInject]: https://github.com/chickensoft-games/AutoInject
[Game Demo]: https://github.com/chickensoft-games/GameDemo
[LogicBlocks]: https://github.com/chickensoft-games/LogicBlocks
[Chickensoft.Serialization]: https://github.com/chickensoft-games/Serialization
[nuget]: https://www.nuget.org/packages/Chickensoft.SaveFileBuilder
