namespace Chickensoft.SaveFileBuilder.Serialization;

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

/// <summary>Provides functionality to serialize from- and deserialize to objects or value types using the <see cref="XmlSerializer"/>.</summary>
/// <remarks>
/// <see cref="XmlStreamSerializer"/> does not provide options for specifying SOAP serialization. SOAP is an application communication protocol, not meant for saving and loading scenarios such as the <see cref="XmlStreamSerializer"/> provides.
/// </remarks>
public class XmlStreamSerializer : IStreamSerializer
{
  /// <summary>Settings for deserializing from xml.</summary>
  public XmlReaderSettings ReaderSettings { get; init; } = new();

  /// <summary>Settings for serializing to xml.</summary>
  public XmlWriterSettings WriterSettings { get; init; } = new();

  /// <inheritdoc cref="XmlSerializerNamespaces" />
  public XmlSerializerNamespaces SerializerNamespaces { get; init; } = new();

  /// <summary>The context information required when deserializing from xml.</summary>
  public XmlParserContext? ParserContext { get; init; } = null;

  /// <summary>Contains events that are invoked during different stages of the deserialization process.</summary>
  public XmlDeserializationEvents DeserializationEvents { get; init; } = default;

  /// <inheritdoc />
  public void Serialize(Stream stream, object? value, Type inputType)
  {
    var serializer = new XmlSerializer(inputType);
    using var writer = XmlWriter.Create(stream, WriterSettings);

    serializer.Serialize(writer, value, SerializerNamespaces);
  }

  /// <inheritdoc />
  public object? Deserialize(Stream stream, Type returnType)
  {
    var serializer = new XmlSerializer(returnType);
    using var reader = XmlReader.Create(stream, ReaderSettings, ParserContext);

    if (!serializer.CanDeserialize(reader))
    {
      return null;
    }

    return serializer.Deserialize(reader, DeserializationEvents);
  }
}
