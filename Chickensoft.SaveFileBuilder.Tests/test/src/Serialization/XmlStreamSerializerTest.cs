namespace Chickensoft.SaveFileBuilder.Tests.Serialization;

using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Chickensoft.SaveFileBuilder.Serialization;

public partial class XmlStreamSerializerTest
{
  #region Test Models

  [Serializable]
  public class ComplexTestData
  {
    public string Description { get; set; } = string.Empty;
    public int Number { get; set; }
    public double DecimalNumber { get; set; }
    public bool Flag { get; set; }
    public List<string> Items { get; set; } = [];
  }

  [Serializable]
  [XmlRoot("CustomRoot")]
  public sealed class XmlTestData
  {
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlElement("CustomName")]
    public string Name { get; set; } = string.Empty;
  }

  #endregion

  #region Serialize Tests

  [Fact]
  public void Serialize_WithBasicObject_SerializesObject()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var testData = new TestData { Name = "Test", Value = 42 };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var xml = Encoding.UTF8.GetString(stream.ToArray());
    Assert.Contains("<Name>Test</Name>", xml);
    Assert.Contains("<Value>42</Value>", xml);
  }

  [Fact]
  public void Serialize_WithNullValue_SerializesNull()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, (TestData?)null);

    // Assert
    stream.Position = 0;
    var xml = Encoding.UTF8.GetString(stream.ToArray());
    Assert.Contains("xsi:nil=\"true\"", xml);
  }

  [Fact]
  public void Serialize_WithComplexObject_SerializesCorrectly()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var testData = new ComplexTestData
    {
      Description = "Complex Test",
      Number = 100,
      DecimalNumber = 3.14159,
      Flag = true,
      Items = ["Item1", "Item2", "Item3"]
    };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var xml = Encoding.UTF8.GetString(stream.ToArray());
    Assert.Contains("<Description>Complex Test</Description>", xml);
    Assert.Contains("<Number>100</Number>", xml);
    Assert.Contains("<DecimalNumber>3.14159</DecimalNumber>", xml);
    Assert.Contains("<Flag>true</Flag>", xml);
    Assert.Contains("<string>Item1</string>", xml);
    Assert.Contains("<string>Item2</string>", xml);
    Assert.Contains("<string>Item3</string>", xml);
  }

  [Fact]
  public void Serialize_WithCustomAttributes_RespectsAttributes()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var testData = new XmlTestData { Id = 123, Name = "CustomName" };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var xml = Encoding.UTF8.GetString(stream.ToArray());
    Assert.Contains("<CustomRoot", xml);
    Assert.Contains("id=\"123\"", xml);
    Assert.Contains("<CustomName>CustomName</CustomName>", xml);
  }

  [Fact]
  public void Serialize_WithSpecialCharacters_EscapesCorrectly()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var testData = new TestData
    {
      Name = "Test with <special> & \"characters\"",
      Value = 42
    };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var xml = Encoding.UTF8.GetString(stream.ToArray());
    Assert.Contains("&lt;special&gt;", xml);
    Assert.Contains("&amp;", xml);
    Assert.Contains("\"", xml);
  }

  [Fact]
  public void Serialize_WithCustomWriterSettings_UsesSettings()
  {
    // Arrange
    var writerSettings = new XmlWriterSettings
    {
      Indent = true,
      IndentChars = "  ",
      NewLineChars = "\n"
    };
    var serializer = new XmlStreamSerializer
    {
      WriterSettings = writerSettings
    };
    var testData = new TestData { Name = "Test", Value = 42 };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var xml = Encoding.UTF8.GetString(stream.ToArray());
    Assert.Contains("\n  <Name>Test</Name>", xml);
  }

  [Fact]
  public void Serialize_WithEmptyNamespaces_OmitsDefaultNamespaces()
  {
    // Arrange
    var namespaces = new XmlSerializerNamespaces();
    namespaces.Add("", "");
    var serializer = new XmlStreamSerializer
    {
      SerializerNamespaces = namespaces
    };
    var testData = new TestData { Name = "Test", Value = 42 };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var xml = Encoding.UTF8.GetString(stream.ToArray());
    Assert.DoesNotContain("xmlns:xsi", xml);
    Assert.DoesNotContain("xmlns:xsd", xml);
  }

  [Fact]
  public void Serialize_WithOmitXmlDeclaration_OmitsDeclaration()
  {
    // Arrange
    var writerSettings = new XmlWriterSettings
    {
      OmitXmlDeclaration = true
    };
    var serializer = new XmlStreamSerializer
    {
      WriterSettings = writerSettings
    };
    var testData = new TestData { Name = "Test", Value = 42 };
    using var stream = new MemoryStream();

    // Act
    serializer.Serialize(stream, testData);

    // Assert
    stream.Position = 0;
    var xml = Encoding.UTF8.GetString(stream.ToArray());
    Assert.DoesNotContain("<?xml version", xml);
  }

  #endregion

  #region Deserialize Tests

  [Fact]
  public void Deserialize_WithValidXml_DeserializesObject()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var xml = /*lang=xml*/ """
      <?xml version="1.0" encoding="utf-8"?>
      <TestData>
        <Name>DeserializeTest</Name>
        <Value>789</Value>
      </TestData>
      """;
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

    // Act
    var result = serializer.Deserialize<TestData>(stream);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("DeserializeTest", result.Name);
    Assert.Equal(789, result.Value);
  }

  [Fact]
  public void Deserialize_WithComplexXml_DeserializesCorrectly()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var xml = /*lang=xml*/ """
      <?xml version="1.0" encoding="utf-8"?>
      <ComplexTestData>
        <Description>Complex Test</Description>
        <Number>100</Number>
        <DecimalNumber>3.14159</DecimalNumber>
        <Flag>true</Flag>
        <Items>
          <string>Item1</string>
          <string>Item2</string>
        </Items>
      </ComplexTestData>
      """;
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

    // Act
    var result = serializer.Deserialize<ComplexTestData>(stream);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Complex Test", result.Description);
    Assert.Equal(100, result.Number);
    Assert.Equal(3.14159, result.DecimalNumber);
    Assert.True(result.Flag);
    Assert.Equal(2, result.Items.Count);
    Assert.Contains("Item1", result.Items);
    Assert.Contains("Item2", result.Items);
  }

  [Fact]
  public void Deserialize_WithCustomAttributes_RespectsAttributes()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var xml = /*lang=xml*/ """
      <?xml version="1.0" encoding="utf-8"?>
      <CustomRoot id="456">
        <CustomName>TestName</CustomName>
      </CustomRoot>
      """;
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

    // Act
    var result = serializer.Deserialize<XmlTestData>(stream);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(456, result.Id);
    Assert.Equal("TestName", result.Name);
  }

  [Fact]
  public void Deserialize_WithEscapedCharacters_UnescapesCorrectly()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var xml = /*lang=xml*/ """
      <?xml version="1.0" encoding="utf-8"?>
      <TestData>
        <Name>Test with &lt;special&gt; &amp; &quot;characters&quot;</Name>
        <Value>42</Value>
      </TestData>
      """;
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

    // Act
    var result = serializer.Deserialize<TestData>(stream);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test with <special> & \"characters\"", result.Name);
    Assert.Equal(42, result.Value);
  }

  [Fact]
  public void Deserialize_WithInvalidXml_ThrowsInvalidOperationException()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var invalidXml = "<TestData><Name>Test</Value></TestData>";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidXml));

    // Act & Assert
    Assert.Throws<InvalidOperationException>(
      () => serializer.Deserialize<TestData>(stream)
    );
  }

  [Fact]
  public void Deserialize_WithEmptyStream_ThrowsXmlException()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    using var stream = new MemoryStream();

    // Act & Assert
    Assert.Throws<XmlException>(
      () => serializer.Deserialize<TestData>(stream)
    );
  }

  [Fact]
  public void Deserialize_WithMalformedXml_ThrowsInvalidOperationException()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var malformedXml = "<TestData><Name>Test";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(malformedXml));

    // Act & Assert
    Assert.Throws<InvalidOperationException>(
      () => serializer.Deserialize<TestData>(stream)
    );
  }

  [Fact]
  public void Deserialize_WithNonDeserializableXml_ReturnsNull()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var xml = /*lang=xml*/ """
      <?xml version="1.0" encoding="utf-8"?>
      <DifferentRoot>
        <SomeElement>Value</SomeElement>
      </DifferentRoot>
      """;
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

    // Act
    var result = serializer.Deserialize<TestData>(stream);

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public void Deserialize_WithCustomReaderSettings_UsesSettings()
  {
    // Arrange
    var readerSettings = new XmlReaderSettings
    {
      IgnoreWhitespace = true,
      IgnoreComments = true
    };
    var serializer = new XmlStreamSerializer
    {
      ReaderSettings = readerSettings
    };
    var xml = /*lang=xml*/ """
      <?xml version="1.0" encoding="utf-8"?>
      <!-- This is a comment -->
      <TestData>
        <Name>    Test    </Name>
        <Value>42</Value>
      </TestData>
      """;
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

    // Act
    var result = serializer.Deserialize<TestData>(stream);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("    Test    ", result.Name);
    Assert.Equal(42, result.Value);
  }

  [Fact]
  public void Deserialize_WithParserContext_UsesContext()
  {
    // Arrange
    var nameTable = new NameTable();
    var namespaceManager = new XmlNamespaceManager(nameTable);
    namespaceManager.AddNamespace("custom", "http://example.com/custom");
    var parserContext = new XmlParserContext(
      nameTable,
      namespaceManager,
      null,
      XmlSpace.None
    );
    var serializer = new XmlStreamSerializer
    {
      ParserContext = parserContext
    };
    var xml = /*lang=xml*/ """
      <?xml version="1.0" encoding="utf-8"?>
      <TestData>
        <Name>Test</Name>
        <Value>42</Value>
      </TestData>
      """;
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

    // Act
    var result = serializer.Deserialize<TestData>(stream);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test", result.Name);
    Assert.Equal(42, result.Value);
  }

  #endregion

  #region Round-Trip Tests

  [Fact]
  public void SerializeAndDeserialize_RoundTrip_PreservesData()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
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
    Assert.NotNull(deserializedData);
    Assert.Equal(originalData.Name, deserializedData.Name);
    Assert.Equal(originalData.Value, deserializedData.Value);
  }

  [Fact]
  public void SerializeAndDeserialize_WithComplexObject_PreservesAllData()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var originalData = new ComplexTestData
    {
      Description = "Round Trip Complex",
      Number = 999,
      DecimalNumber = 2.71828,
      Flag = false,
      Items = ["Alpha", "Beta", "Gamma"]
    };

    // Act - Serialize
    using var stream = new MemoryStream();
    serializer.Serialize(stream, originalData);

    // Act - Deserialize
    stream.Position = 0;
    var deserializedData = serializer.Deserialize<ComplexTestData>(stream);

    // Assert
    Assert.NotNull(deserializedData);
    Assert.Equal(originalData.Description, deserializedData.Description);
    Assert.Equal(originalData.Number, deserializedData.Number);
    Assert.Equal(originalData.DecimalNumber, deserializedData.DecimalNumber);
    Assert.Equal(originalData.Flag, deserializedData.Flag);
    Assert.Equal(originalData.Items.Count, deserializedData.Items.Count);
    for (var i = 0; i < originalData.Items.Count; i++)
    {
      Assert.Equal(originalData.Items[i], deserializedData.Items[i]);
    }
  }

  [Fact]
  public void SerializeAndDeserialize_WithDifferentSerializers_PreservesData()
  {
    // Arrange
    var serializeSerializer = new XmlStreamSerializer();
    var deserializeSerializer = new XmlStreamSerializer();
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
    Assert.NotNull(deserializedData);
    Assert.Equal(originalData.Name, deserializedData.Name);
    Assert.Equal(originalData.Value, deserializedData.Value);
  }

  [Fact]
  public void SerializeAndDeserialize_WithSpecialCharacters_PreservesData()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var originalData = new TestData
    {
      Name = "Test with <tags>, & ampersands, \"quotes\", and 'apostrophes'",
      Value = 0
    };

    // Act - Serialize
    using var stream = new MemoryStream();
    serializer.Serialize(stream, originalData);

    // Act - Deserialize
    stream.Position = 0;
    var deserializedData = serializer.Deserialize<TestData>(stream);

    // Assert
    Assert.NotNull(deserializedData);
    Assert.Equal(originalData.Name, deserializedData.Name);
    Assert.Equal(originalData.Value, deserializedData.Value);
  }

  [Fact]
  public void SerializeAndDeserialize_WithMaxValues_PreservesData()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
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
    Assert.NotNull(deserializedData);
    Assert.Equal(originalData.Name, deserializedData.Name);
    Assert.Equal(originalData.Value, deserializedData.Value);
  }

  [Fact]
  public void SerializeAndDeserialize_WithEmptyCollections_PreservesData()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var originalData = new ComplexTestData
    {
      Description = "Empty Collections",
      Number = 0,
      DecimalNumber = 0.0,
      Flag = false,
      Items = []
    };

    // Act - Serialize
    using var stream = new MemoryStream();
    serializer.Serialize(stream, originalData);

    // Act - Deserialize
    stream.Position = 0;
    var deserializedData = serializer.Deserialize<ComplexTestData>(stream);

    // Assert
    Assert.NotNull(deserializedData);
    Assert.Equal(originalData.Description, deserializedData.Description);
    Assert.NotNull(deserializedData.Items);
    Assert.Empty(deserializedData.Items);
  }

  [Fact]
  public void SerializeAndDeserialize_WithCustomSettings_PreservesData()
  {
    // Arrange
    var writerSettings = new XmlWriterSettings
    {
      Indent = true,
      Encoding = Encoding.UTF8
    };
    var namespaces = new XmlSerializerNamespaces();
    namespaces.Add("", "");
    var serializer = new XmlStreamSerializer
    {
      WriterSettings = writerSettings,
      SerializerNamespaces = namespaces
    };
    var originalData = new TestData
    {
      Name = "CustomSettingsTest",
      Value = 777
    };

    // Act - Serialize
    using var stream = new MemoryStream();
    serializer.Serialize(stream, originalData);

    // Act - Deserialize
    stream.Position = 0;
    var deserializedData = serializer.Deserialize<TestData>(stream);

    // Assert
    Assert.NotNull(deserializedData);
    Assert.Equal(originalData.Name, deserializedData.Name);
    Assert.Equal(originalData.Value, deserializedData.Value);
  }

  #endregion

  #region Property Initialization Tests

  [Fact]
  public void Constructor_DefaultValues_InitializesProperties()
  {
    // Act
    var serializer = new XmlStreamSerializer();

    // Assert
    Assert.NotNull(serializer.ReaderSettings);
    Assert.NotNull(serializer.WriterSettings);
    Assert.NotNull(serializer.SerializerNamespaces);
    Assert.Null(serializer.ParserContext);
  }

  [Fact]
  public void Properties_CanBeSetViaInitializer()
  {
    // Arrange
    var readerSettings = new XmlReaderSettings { IgnoreWhitespace = true };
    var writerSettings = new XmlWriterSettings { Indent = true };
    var namespaces = new XmlSerializerNamespaces();
    namespaces.Add("custom", "http://example.com");
    var nameTable = new NameTable();
    var parserContext = new XmlParserContext(nameTable, null, null, XmlSpace.None);

    // Act
    var serializer = new XmlStreamSerializer
    {
      ReaderSettings = readerSettings,
      WriterSettings = writerSettings,
      SerializerNamespaces = namespaces,
      ParserContext = parserContext
    };

    // Assert
    Assert.Same(readerSettings, serializer.ReaderSettings);
    Assert.Same(writerSettings, serializer.WriterSettings);
    Assert.Same(namespaces, serializer.SerializerNamespaces);
    Assert.Same(parserContext, serializer.ParserContext);
  }

  #endregion
}
