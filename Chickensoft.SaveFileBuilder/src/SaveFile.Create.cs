namespace Chickensoft.SaveFileBuilder;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Chickensoft.SaveFileBuilder.Compression;
using Chickensoft.SaveFileBuilder.IO;
using Chickensoft.SaveFileBuilder.Serialization;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

/// <summary>Provides factory methods for creating common save file configurations.</summary>
public partial class SaveFile
{
  /// <summary>Creates a new <see cref="SaveFile"/> that uses JSON serialization and GZip compression.</summary>
#if NET8_0_OR_GREATER
  [RequiresUnreferencedCode(JsonStreamSerializer.Messages.REQUIRES_UNREFERENCED_CODE)]
  [RequiresDynamicCode(JsonStreamSerializer.Messages.REQUIRES_DYNAMIC_CODE)]
#endif
  public static SaveFile CreateGZipJsonFile(string filePath, JsonSerializerOptions? options = null) => new(
    io: new FileStreamIO(filePath),
    serializer: new JsonStreamSerializer(options),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonFile(string, JsonSerializerOptions?)" />
  public static SaveFile CreateGZipJsonFile(string filePath, JsonSerializerContext context) => new(
    io: new FileStreamIO(filePath),
    serializer: new JsonStreamSerializer(context),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonFile(string, JsonSerializerOptions?)" />
  public static SaveFile CreateGZipJsonFile(string filePath, JsonTypeInfo jsonTypeInfo) => new(
    io: new FileStreamIO(filePath),
    serializer: new JsonStreamSerializer(jsonTypeInfo),
    compressor: new GZipStreamCompressor()
  );

  /// <summary>Creates a new <see cref="SaveFile"/> that uses the specified io, JSON serialization and GZip compression.</summary>
#if NET8_0_OR_GREATER
  [RequiresUnreferencedCode(JsonStreamSerializer.Messages.REQUIRES_UNREFERENCED_CODE)]
  [RequiresDynamicCode(JsonStreamSerializer.Messages.REQUIRES_DYNAMIC_CODE)]
#endif
  public static SaveFile CreateGZipJsonIO(IStreamIO io, JsonSerializerOptions? options = null) => new(
    io: io,
    serializer: new JsonStreamSerializer(options),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonIO(IStreamIO, JsonSerializerOptions?)" />
  public static SaveFile CreateGZipJsonIO(IStreamIO io, JsonSerializerContext context) => new(
    io: io,
    serializer: new JsonStreamSerializer(context),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonIO(IStreamIO, JsonSerializerOptions?)" />
  public static SaveFile CreateGZipJsonIO(IStreamIO io, JsonTypeInfo jsonTypeInfo) => new(
    io: io,
    serializer: new JsonStreamSerializer(jsonTypeInfo),
    compressor: new GZipStreamCompressor()
  );

  /// <summary>Creates a new <see cref="SaveFile"/> that uses the specified io, JSON serialization and GZip compression.</summary>
#if NET8_0_OR_GREATER
  [RequiresUnreferencedCode(JsonStreamSerializer.Messages.REQUIRES_UNREFERENCED_CODE)]
  [RequiresDynamicCode(JsonStreamSerializer.Messages.REQUIRES_DYNAMIC_CODE)]
#endif
  public static SaveFile CreateGZipJsonIO(IAsyncStreamIO asyncIO, JsonSerializerOptions? options = null) => new(
    asyncIO: asyncIO,
    asyncSerializer: new JsonStreamSerializer(options),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonIO(IAsyncStreamIO, JsonSerializerOptions?)" />
  public static SaveFile CreateGZipJsonIO(IAsyncStreamIO asyncIO, JsonSerializerContext context) => new(
    asyncIO: asyncIO,
    asyncSerializer: new JsonStreamSerializer(context),
    compressor: new GZipStreamCompressor()
  );

  /// <inheritdoc cref="CreateGZipJsonIO(IAsyncStreamIO, JsonSerializerOptions?)" />
  public static SaveFile CreateGZipJsonFIO(IAsyncStreamIO asyncIO, JsonTypeInfo jsonTypeInfo) => new(
    asyncIO: asyncIO,
    asyncSerializer: new JsonStreamSerializer(jsonTypeInfo),
    compressor: new GZipStreamCompressor()
  );
}

