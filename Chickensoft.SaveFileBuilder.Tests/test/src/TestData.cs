namespace Chickensoft.SaveFileBuilder.Tests;

using System.Text.Json.Serialization;

public class TestData
{
  public string Name { get; set; } = string.Empty;
  public int Value { get; set; }
}

[JsonSerializable(typeof(TestData))]
internal sealed partial class TestJsonContext : JsonSerializerContext;
