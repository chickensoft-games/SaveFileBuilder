namespace Chickensoft.SaveFileBuilder.Tests.Serialization;

using System.Text;
using System.Text.Json;
using Chickensoft.SaveFileBuilder.Serialization;

public class JsonStreamSerializerTest(ITestContextAccessor testContextAccessor)
{
  private CancellationToken CancellationToken { get; } = testContextAccessor.Current.CancellationToken;

  #region Serialize Tests

  [Fact]
  public void Serialize_WithJsonTypeInfo_SerializesObject()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var testData = new TestData { Name = "Test", Value = 42 };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var json = Encoding.UTF8.GetString(stream.ToArray());
    json.ShouldContain("\"Name\":\"Test\"");
    json.ShouldContain("\"Value\":42");
  }

  [Fact]
  public void Serialize_WithJsonSerializerOptions_SerializesObject()
  {
    // Arrange
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    var serializer = new JsonStreamSerializer(options);
    var testData = new TestData { Name = "Test", Value = 42 };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var json = Encoding.UTF8.GetString(stream.ToArray());
    json.ShouldContain("\"name\":");
    json.ShouldContain("\"value\":");
  }

  [Fact]
  public void Serialize_WithJsonSerializerContext_SerializesObject()
  {
    // Arrange
    var context = TestJsonContext.Default;
    var serializer = new JsonStreamSerializer(context);
    var testData = new TestData { Name = "Test", Value = 42 };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var json = Encoding.UTF8.GetString(stream.ToArray());
    json.ShouldContain("\"Name\":\"Test\"");
    json.ShouldContain("\"Value\":42");
  }

  [Fact]
  public void Serialize_WithNullValue_SerializesNull()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.String;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, null, typeof(string));

    // Assert
    stream.Position = 0;
    var json = Encoding.UTF8.GetString(stream.ToArray());
    json.ShouldBe("null");
  }

  [Fact]
  public void Serialize_WithComplexObject_SerializesCorrectly()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var testData = new TestData
    {
      Name = "Complex Test with \"quotes\" and \n newlines",
      Value = int.MaxValue
    };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var json = Encoding.UTF8.GetString(stream.ToArray());
    json.ShouldContain("\"Name\":\"Complex Test with \\u0022quotes\\u0022 and \\n newlines\"");
    json.ShouldContain("\"Value\":2147483647");
  }

  #endregion

  #region SerializeAsync Tests

  [Fact]
  public async Task SerializeAsync_WithJsonTypeInfo_SerializesObject()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var testData = new TestData { Name = "AsyncTest", Value = 123 };
    using var stream = new MemoryStream();

    // Act
    await serializer.SerializeAsync(stream, testData, CancellationToken);

    // Assert
    stream.Position = 0;
    var json = Encoding.UTF8.GetString(stream.ToArray());
    json.ShouldContain("\"Name\":\"AsyncTest\"");
    json.ShouldContain("\"Value\":123");
  }

  [Fact]
  public async Task SerializeAsync_WithJsonSerializerOptions_SerializesObject()
  {
    // Arrange
    var options = new JsonSerializerOptions
    {
      WriteIndented = true
    };
    var serializer = new JsonStreamSerializer(options);
    var testData = new TestData { Name = "AsyncTest", Value = 123 };
    using var stream = new MemoryStream();

    // Act
    await serializer.SerializeAsync(stream, testData, CancellationToken);

    // Assert
    stream.Position = 0;
    var json = Encoding.UTF8.GetString(stream.ToArray());
    json.ShouldContain("\n"); // Indented JSON contains newlines
  }

  [Fact]
  public async Task SerializeAsync_WithJsonSerializerContext_SerializesObject()
  {
    // Arrange
    var context = TestJsonContext.Default;
    var serializer = new JsonStreamSerializer(context);
    var testData = new TestData { Name = "AsyncTest", Value = 123 };
    using var stream = new MemoryStream();

    // Act
    await serializer.SerializeAsync(stream, testData, CancellationToken);

    // Assert
    stream.Position = 0;
    var json = Encoding.UTF8.GetString(stream.ToArray());
    json.ShouldContain("\"Name\":\"AsyncTest\"");
    json.ShouldContain("\"Value\":123");
  }

  [Fact]
  public async Task SerializeAsync_WithCancelledToken_ThrowsOperationCanceledException()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var testData = new TestData { Name = "CancelTest", Value = 456 };
    using var stream = new MemoryStream();
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
    cts.Cancel();

    // Act & Assert
    await Should.ThrowAsync<TaskCanceledException>(async () => await serializer.SerializeAsync(stream, testData, cts.Token));
  }

  [Fact]
  public async Task SerializeAsync_WithNullValue_SerializesNull()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.String;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    using var stream = new MemoryStream();

    // Act
    await serializer.SerializeAsync(stream, null, typeof(string), CancellationToken);

    // Assert
    stream.Position = 0;
    var json = Encoding.UTF8.GetString(stream.ToArray());
    json.ShouldBe("null");
  }

  #endregion

  #region Deserialize Tests

  [Fact]
  public void Deserialize_WithJsonTypeInfo_DeserializesObject()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var json = /*lang=json,strict*/ "{\"Name\":\"DeserializeTest\",\"Value\":789}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    // Act
    var result = serializer.Deserialize<TestData>(stream);

    // Assert
    result.ShouldNotBeNull();
    result.Name.ShouldBe("DeserializeTest");
    result.Value.ShouldBe(789);
  }

  [Fact]
  public void Deserialize_WithJsonSerializerOptions_DeserializesObject()
  {
    // Arrange
    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };
    var serializer = new JsonStreamSerializer(options);
    var json = /*lang=json,strict*/ "{\"name\":\"DeserializeTest\",\"value\":789}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    // Act
    var result = serializer.Deserialize<TestData>(stream);

    // Assert
    result.ShouldNotBeNull();
    result.Name.ShouldBe("DeserializeTest");
    result.Value.ShouldBe(789);
  }

  [Fact]
  public void Deserialize_WithJsonSerializerContext_DeserializesObject()
  {
    // Arrange
    var context = TestJsonContext.Default;
    var serializer = new JsonStreamSerializer(context);
    var json = /*lang=json,strict*/ "{\"Name\":\"DeserializeTest\",\"Value\":789}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    // Act
    var result = serializer.Deserialize<TestData>(stream);

    // Assert
    result.ShouldNotBeNull();
    result.Name.ShouldBe("DeserializeTest");
    result.Value.ShouldBe(789);
  }

  [Fact]
  public void Deserialize_WithNullJson_ReturnsNull()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.String;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var json = "null";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    // Act
    var result = serializer.Deserialize<string>(stream);

    // Assert
    result.ShouldBeNull();
  }

  [Fact]
  public void Deserialize_WithInvalidJson_ThrowsJsonException()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var invalidJson = "{\"Name\":\"Test\",\"Value\":}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));

    // Act & Assert
    Should.Throw<JsonException>(() => serializer.Deserialize<TestData>(stream));
  }

  [Fact]
  public void Deserialize_WithEmptyStream_ThrowsJsonException()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    using var stream = new MemoryStream();

    // Act & Assert
    Should.Throw<JsonException>(() => serializer.Deserialize<TestData>(stream));
  }

  #endregion

  #region DeserializeAsync Tests

  [Fact]
  public async Task DeserializeAsync_WithJsonTypeInfo_DeserializesObject()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var json = /*lang=json,strict*/ "{\"Name\":\"AsyncDeserializeTest\",\"Value\":999}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    // Act
    var result = await serializer.DeserializeAsync<TestData>(stream, CancellationToken);

    // Assert
    result.ShouldNotBeNull();
    result.Name.ShouldBe("AsyncDeserializeTest");
    result.Value.ShouldBe(999);
  }

  [Fact]
  public async Task DeserializeAsync_WithJsonSerializerOptions_DeserializesObject()
  {
    // Arrange
    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };
    var serializer = new JsonStreamSerializer(options);
    var json = /*lang=json,strict*/ "{\"name\":\"AsyncDeserializeTest\",\"value\":999}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    // Act
    var result = await serializer.DeserializeAsync<TestData>(stream, CancellationToken);

    // Assert
    result.ShouldNotBeNull();
    result.Name.ShouldBe("AsyncDeserializeTest");
    result.Value.ShouldBe(999);
  }

  [Fact]
  public async Task DeserializeAsync_WithJsonSerializerContext_DeserializesObject()
  {
    // Arrange
    var context = TestJsonContext.Default;
    var serializer = new JsonStreamSerializer(context);
    var json = /*lang=json,strict*/ "{\"Name\":\"AsyncDeserializeTest\",\"Value\":999}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    // Act
    var result = await serializer.DeserializeAsync<TestData>(stream, CancellationToken);

    // Assert
    result.ShouldNotBeNull();
    result.Name.ShouldBe("AsyncDeserializeTest");
    result.Value.ShouldBe(999);
  }

  [Fact]
  public async Task DeserializeAsync_WithCancelledToken_ThrowsOperationCanceledException()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var json = /*lang=json,strict*/ "{\"Name\":\"CancelTest\",\"Value\":777}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
    cts.Cancel();

    // Act & Assert
    await Should.ThrowAsync<TaskCanceledException>(async () => await serializer.DeserializeAsync<TestData>(stream, cts.Token));
  }

  [Fact]
  public async Task DeserializeAsync_WithNullJson_ReturnsNull()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.String;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var json = "null";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    // Act
    var result = await serializer.DeserializeAsync<string>(stream, CancellationToken);

    // Assert
    result.ShouldBeNull();
  }

  [Fact]
  public async Task DeserializeAsync_WithInvalidJson_ThrowsJsonException()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var invalidJson = "{\"Name\":\"Test\",\"Value\":}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));

    // Act & Assert
    await Should.ThrowAsync<JsonException>(async () => await serializer.DeserializeAsync<TestData>(stream, CancellationToken));
  }

  [Fact]
  public async Task DeserializeAsync_WithEmptyStream_ThrowsJsonException()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    using var stream = new MemoryStream();

    // Act & Assert
    await Should.ThrowAsync<JsonException>(async () => await serializer.DeserializeAsync<TestData>(stream, CancellationToken));
  }

  #endregion

  #region Round-Trip Tests

  [Fact]
  public void SerializeAndDeserialize_RoundTrip_PreservesData()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var originalData = new TestData
    {
      Name = "RoundTripTest",
      Value = 12345
    };

    // Act - Serialize
    using var stream = new MemoryStream();
    serializer.Serialize(stream, originalData);

    // Act - Deserialize
    stream.Position = 0;
    var deserializedData = serializer.Deserialize<TestData>(stream);

    // Assert
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
  }

  [Fact]
  public async Task SerializeAsyncAndDeserializeAsync_RoundTrip_PreservesData()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var originalData = new TestData
    {
      Name = "AsyncRoundTripTest",
      Value = 54321
    };

    // Act - Serialize
    using var stream = new MemoryStream();
    await serializer.SerializeAsync(stream, originalData, CancellationToken);

    // Act - Deserialize
    stream.Position = 0;
    var deserializedData = await serializer.DeserializeAsync<TestData>(stream, CancellationToken);

    // Assert
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
  }

  [Fact]
  public void SerializeAndDeserialize_WithDifferentSerializers_PreservesData()
  {
    // Arrange
    var serializeSerializer = new JsonStreamSerializer(TestJsonContext.Default.TestData);
    var deserializeSerializer = new JsonStreamSerializer();
    var originalData = new TestData
    {
      Name = "CrossSerializerTest",
      Value = 99999
    };

    // Act - Serialize with one serializer
    using var stream = new MemoryStream();
    serializeSerializer.Serialize(stream, originalData);

    // Act - Deserialize with another serializer
    stream.Position = 0;
    var deserializedData = deserializeSerializer.Deserialize<TestData>(stream);

    // Assert
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
  }

  [Fact]
  public void SerializeAndDeserialize_WithSpecialCharacters_PreservesData()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var originalData = new TestData
    {
      Name = "Test with \"quotes\", \n newlines, \t tabs, and \\ backslashes",
      Value = 0
    };

    // Act - Serialize
    using var stream = new MemoryStream();
    serializer.Serialize(stream, originalData);

    // Act - Deserialize
    stream.Position = 0;
    var deserializedData = serializer.Deserialize<TestData>(stream);

    // Assert
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
  }

  [Fact]
  public void SerializeAndDeserialize_WithMaxValues_PreservesData()
  {
    // Arrange
    var jsonTypeInfo = TestJsonContext.Default.TestData;
    var serializer = new JsonStreamSerializer(jsonTypeInfo);
    var originalData = new TestData
    {
      Name = new string('X', 10000), // Long string
      Value = int.MaxValue
    };

    // Act - Serialize
    using var stream = new MemoryStream();
    serializer.Serialize(stream, originalData);

    // Act - Deserialize
    stream.Position = 0;
    var deserializedData = serializer.Deserialize<TestData>(stream);

    // Assert
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
  }

  #endregion
}
