namespace Chickensoft.SaveFileBuilder;
/// <summary>Defines functionality to save and load an object's state using a specified data type.</summary>
/// <typeparam name="TData">The type of data used to represent the object's state. Must be a reference type.</typeparam>
public interface ISaveable<TData> where TData : class
{
  /// <summary>Saves the current state and returns the resulting data object.</summary>
  /// <returns>The data object representing the saved state.</returns>
  TData Save();

  /// <summary>Loads the state from the specified data object.</summary>
  /// <param name="data">The data object containing the state to load.</param>
  void Load(in TData data);
}
