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
    xml.ShouldContain("<Name>Test</Name>");
    xml.ShouldContain("<Value>42</Value>");
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
    xml.ShouldContain("xsi:nil=\"true\"");
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
    xml.ShouldContain("<Description>Complex Test</Description>");
    xml.ShouldContain("<Number>100</Number>");
    xml.ShouldContain("<DecimalNumber>3.14159</DecimalNumber>");
    xml.ShouldContain("<Flag>true</Flag>");
    xml.ShouldContain("<string>Item1</string>");
    xml.ShouldContain("<string>Item2</string>");
    xml.ShouldContain("<string>Item3</string>");
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
    xml.ShouldContain("<CustomRoot");
    xml.ShouldContain("id=\"123\"");
    xml.ShouldContain("<CustomName>CustomName</CustomName>");
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
    xml.ShouldContain("&lt;special&gt;");
    xml.ShouldContain("&amp;");
    xml.ShouldContain("\"");
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
    xml.ShouldContain("\n  <Name>Test</Name>");
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
    xml.ShouldNotContain("xmlns:xsi");
    xml.ShouldNotContain("xmlns:xsd");
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
    xml.ShouldNotContain("<?xml version");
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
    result.ShouldNotBeNull();
    result.Name.ShouldBe("DeserializeTest");
    result.Value.ShouldBe(789);
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
    result.ShouldNotBeNull();
    result.Description.ShouldBe("Complex Test");
    result.Number.ShouldBe(100);
    result.DecimalNumber.ShouldBe(3.14159);
    result.Flag.ShouldBeTrue();
    result.Items.Count.ShouldBe(2);
    result.Items.ShouldContain("Item1");
    result.Items.ShouldContain("Item2");
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
    result.ShouldNotBeNull();
    result.Id.ShouldBe(456);
    result.Name.ShouldBe("TestName");
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
    result.ShouldNotBeNull();
    result.Name.ShouldBe("Test with <special> & \"characters\"");
    result.Value.ShouldBe(42);
  }

  [Fact]
  public void Deserialize_WithInvalidXml_ThrowsInvalidOperationException()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var invalidXml = "<TestData><Name>Test</Value></TestData>";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidXml));

    // Act & Assert
    Should.Throw<InvalidOperationException>(() => serializer.Deserialize<TestData>(stream));
  }

  [Fact]
  public void Deserialize_WithEmptyStream_ThrowsXmlException()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    using var stream = new MemoryStream();

    // Act & Assert
    Should.Throw<XmlException>(() => serializer.Deserialize<TestData>(stream));
  }

  [Fact]
  public void Deserialize_WithMalformedXml_ThrowsInvalidOperationException()
  {
    // Arrange
    var serializer = new XmlStreamSerializer();
    var malformedXml = "<TestData><Name>Test";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(malformedXml));

    // Act & Assert
    Should.Throw<InvalidOperationException>(() => serializer.Deserialize<TestData>(stream));
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
    result.ShouldBeNull();
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
    result.ShouldNotBeNull();
    result.Name.ShouldBe("    Test    ");
    result.Value.ShouldBe(42);
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
    result.ShouldNotBeNull();
    result.Name.ShouldBe("Test");
    result.Value.ShouldBe(42);
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
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
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
    deserializedData.ShouldNotBeNull();
    deserializedData.Description.ShouldBe(originalData.Description);
    deserializedData.Number.ShouldBe(originalData.Number);
    deserializedData.DecimalNumber.ShouldBe(originalData.DecimalNumber);
    deserializedData.Flag.ShouldBe(originalData.Flag);
    deserializedData.Items.Count.ShouldBe(originalData.Items.Count);
    for (var i = 0; i < originalData.Items.Count; i++)
    {
      deserializedData.Items[i].ShouldBe(originalData.Items[i]);
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
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
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
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
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
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
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
    deserializedData.ShouldNotBeNull();
    deserializedData.Description.ShouldBe(originalData.Description);
    deserializedData.Items.ShouldNotBeNull();
    deserializedData.Items.ShouldBeEmpty();
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
    deserializedData.ShouldNotBeNull();
    deserializedData.Name.ShouldBe(originalData.Name);
    deserializedData.Value.ShouldBe(originalData.Value);
  }

  #endregion

  #region Property Initialization Tests

  [Fact]
  public void Constructor_DefaultValues_InitializesProperties()
  {
    // Act
    var serializer = new XmlStreamSerializer();

    // Assert
    serializer.ReaderSettings.ShouldNotBeNull();
    serializer.WriterSettings.ShouldNotBeNull();
    serializer.SerializerNamespaces.ShouldNotBeNull();
    serializer.ParserContext.ShouldBeNull();
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
    serializer.ReaderSettings.ShouldBeSameAs(readerSettings);
    serializer.WriterSettings.ShouldBeSameAs(writerSettings);
    serializer.SerializerNamespaces.ShouldBeSameAs(namespaces);
    serializer.ParserContext.ShouldBeSameAs(parserContext);
  }

  #endregion
}
