namespace Chickensoft.SaveFileBuilder;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Provides functionality to serialize from- and deserialize to objects or value types.</summary>
public interface IStreamSerializer
{
  /// <inheritdoc cref="IAsyncStreamSerializer.SerializeAsync(Stream, object?, Type, CancellationToken)" />
  void Serialize(Stream stream, object? value, Type inputType);

  /// <inheritdoc cref="IAsyncStreamSerializer.DeserializeAsync(Stream, Type, CancellationToken)" />
  object? Deserialize(Stream stream, Type returnType);
}

/// <summary>Provides functionality to serialize from- and deserialize to objects or value types asynchronously.</summary>
public interface IAsyncStreamSerializer
{
  /// <summary>Serializes the specified value to the stream.</summary>
  /// <param name="stream">The stream to serialize to.</param>
  /// <param name="value">The object to serialize.</param>
  /// <param name="inputType">The type of the object to serialize.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to cancel the serialization operation.</param>
  Task SerializeAsync(Stream stream, object? value, Type inputType, CancellationToken cancellationToken = default);

  /// <summary>Deserializes the stream to the specified type.</summary>
  /// <param name="stream">The stream to deserialize from.</param>
  /// <param name="returnType">The type of the object to deserialize.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to cancel the deserialization operation.</param>
  /// <returns>The deserialized object.</returns>
  ValueTask<object?> DeserializeAsync(Stream stream, Type returnType, CancellationToken cancellationToken = default);
}

/// <summary>Provides extension methods for <see cref="IStreamSerializer"/> and <see cref="IAsyncStreamSerializer"/>.</summary>
public static class IStreamSerializerExtensions
{
  /// <inheritdoc cref="IStreamSerializer.Serialize(Stream, object?, Type)" />
  /// <typeparam name="TValue">The type of the object to serialize.</typeparam>
  public static void Serialize<TValue>(this IStreamSerializer serializer, Stream stream, TValue value) => serializer.Serialize(stream, value, typeof(TValue));

  /// <inheritdoc cref="IStreamSerializer.Deserialize(Stream, Type)" />
  /// <typeparam name="TValue">The type of the object to deserialize.</typeparam>
  public static TValue? Deserialize<TValue>(this IStreamSerializer serializer, Stream stream) => (TValue?)serializer.Deserialize(stream, typeof(TValue))!;

  /// <inheritdoc cref="IAsyncStreamSerializer.SerializeAsync(Stream, object?, Type, CancellationToken)" />
  /// <typeparam name="TValue">The type of the object to serialize.</typeparam>
  public static Task SerializeAsync<TValue>(this IAsyncStreamSerializer serializer, Stream stream, TValue value, CancellationToken cancellationToken = default) => serializer.SerializeAsync(stream, value, typeof(TValue), cancellationToken);

  /// <inheritdoc cref="IAsyncStreamSerializer.DeserializeAsync(Stream, Type, CancellationToken)" />
  /// <typeparam name="TValue">The type of the object to deserialize.</typeparam>
  public static async ValueTask<TValue?> DeserializeAsync<TValue>(this IAsyncStreamSerializer serializer, Stream stream, CancellationToken cancellationToken = default) => (TValue?)(await serializer.DeserializeAsync(stream, typeof(TValue), cancellationToken))!;
}
