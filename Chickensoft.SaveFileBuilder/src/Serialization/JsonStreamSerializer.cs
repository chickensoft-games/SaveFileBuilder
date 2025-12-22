namespace Chickensoft.SaveFileBuilder.Serialization;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Provides functionality to serialize from- and deserialize to objects or value types using the <see cref="JsonSerializer"/>.</summary>
public class JsonStreamSerializer : IStreamSerializer, IAsyncStreamSerializer
{
  private static class DynamicCodeSuppress
  {
    public static class IL2026
    {
      public const string CATEGORY = "Trimming";
      public const string CHECK_ID = "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code";
    }

    public static class IL3050
    {
      public const string CATEGORY = "AOT";
      public const string CHECK_ID = "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.";
    }

    public const string JUSTIFICATION = "Members annotated with the 'RequiresUnreferencedCodeAttribute' and 'RequiresDynamicCodeAttribute' will not be called because the initialization steps required for these members are already decorated with these attributes.";
  }

  private readonly JsonTypeInfo? _jsonTypeInfo;
  private readonly JsonSerializerOptions? _options;
  private readonly JsonSerializerContext? _context;

  /// <summary>Initializes a new instance of the <see cref="JsonStreamSerializer"/> class.</summary>
  /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
  public JsonStreamSerializer(JsonTypeInfo jsonTypeInfo)
  {
    _jsonTypeInfo = jsonTypeInfo;
  }

  /// <summary>Initializes a new instance of the <see cref="JsonStreamSerializer"/> class.</summary>
  /// <param name="options">Options to control serialization behavior.</param>
#if NET8_0_OR_GREATER
  [RequiresUnreferencedCode("Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
  [RequiresDynamicCode("Use System.Text.Json source generation for native AOT applications.")]
#endif
  public JsonStreamSerializer(JsonSerializerOptions? options = null)
  {
    _options = options;
  }

  /// <summary>Initializes a new instance of the <see cref="JsonStreamSerializer"/> class.</summary>
  /// <param name="context">A metadata provider for serializable types.</param>
  public JsonStreamSerializer(JsonSerializerContext context)
  {
    _context = context;
  }

  /// <inheritdoc />
  [SuppressMessage(DynamicCodeSuppress.IL2026.CATEGORY, DynamicCodeSuppress.IL2026.CHECK_ID, Justification = DynamicCodeSuppress.JUSTIFICATION)]
  [SuppressMessage(DynamicCodeSuppress.IL3050.CATEGORY, DynamicCodeSuppress.IL3050.CHECK_ID, Justification = DynamicCodeSuppress.JUSTIFICATION)]
  public void Serialize(Stream stream, object? value, Type inputType)
  {
    if (_jsonTypeInfo != null)
    {
      JsonSerializer.Serialize(stream, value, _jsonTypeInfo);
    }
    else if (_context != null)
    {
      JsonSerializer.Serialize(stream, value, inputType, _context);
    }
    else
    {
      JsonSerializer.Serialize(stream, value, inputType, _options);
    }
  }

  /// <inheritdoc />
  [SuppressMessage(DynamicCodeSuppress.IL2026.CATEGORY, DynamicCodeSuppress.IL2026.CHECK_ID, Justification = DynamicCodeSuppress.JUSTIFICATION)]
  [SuppressMessage(DynamicCodeSuppress.IL3050.CATEGORY, DynamicCodeSuppress.IL3050.CHECK_ID, Justification = DynamicCodeSuppress.JUSTIFICATION)]
  public Task SerializeAsync(Stream stream, object? value, Type inputType, CancellationToken cancellationToken = default)
  {
    if (_jsonTypeInfo != null)
    {
      return JsonSerializer.SerializeAsync(stream, value, _jsonTypeInfo, cancellationToken);
    }
    else if (_context != null)
    {
      return JsonSerializer.SerializeAsync(stream, value, inputType, _context, cancellationToken);
    }
    else
    {
      return JsonSerializer.SerializeAsync(stream, value, inputType, _options, cancellationToken);
    }
  }

  /// <inheritdoc />
  [SuppressMessage(DynamicCodeSuppress.IL2026.CATEGORY, DynamicCodeSuppress.IL2026.CHECK_ID, Justification = DynamicCodeSuppress.JUSTIFICATION)]
  [SuppressMessage(DynamicCodeSuppress.IL3050.CATEGORY, DynamicCodeSuppress.IL3050.CHECK_ID, Justification = DynamicCodeSuppress.JUSTIFICATION)]
  public object? Deserialize(Stream stream, Type returnType)
  {
    if (_jsonTypeInfo != null)
    {
      return JsonSerializer.Deserialize(stream, _jsonTypeInfo);
    }
    else if (_context != null)
    {
      return JsonSerializer.Deserialize(stream, returnType, _context);
    }
    else
    {
      return JsonSerializer.Deserialize(stream, returnType, _options);
    }
  }

  /// <inheritdoc />
  [SuppressMessage(DynamicCodeSuppress.IL2026.CATEGORY, DynamicCodeSuppress.IL2026.CHECK_ID, Justification = DynamicCodeSuppress.JUSTIFICATION)]
  [SuppressMessage(DynamicCodeSuppress.IL3050.CATEGORY, DynamicCodeSuppress.IL3050.CHECK_ID, Justification = DynamicCodeSuppress.JUSTIFICATION)]
  public ValueTask<object?> DeserializeAsync(Stream stream, Type returnType, CancellationToken cancellationToken = default)
  {
    if (_jsonTypeInfo != null)
    {
      return JsonSerializer.DeserializeAsync(stream, _jsonTypeInfo, cancellationToken);
    }
    else if (_context != null)
    {
      return JsonSerializer.DeserializeAsync(stream, returnType, _context, cancellationToken);
    }
    else
    {
      return JsonSerializer.DeserializeAsync(stream, returnType, _options, cancellationToken);
    }
  }
}
