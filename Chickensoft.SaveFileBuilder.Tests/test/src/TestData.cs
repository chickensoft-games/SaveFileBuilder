namespace Chickensoft.SaveFileBuilder.Tests;

using System.Text.Json.Serialization;

public class TestData
{
  public required string Name { get; init; }
  public required int Value { get; init; }
}

[JsonSerializable(typeof(TestData))]
internal sealed partial class TestJsonContext : JsonSerializerContext;
