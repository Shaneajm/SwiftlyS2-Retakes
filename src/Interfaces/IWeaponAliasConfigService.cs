namespace SwiftlyS2_Retakes.Interfaces;

/// <summary>
/// Provides user-defined weapon aliases loaded from resources/guns.jsonc.
/// </summary>
public interface IWeaponAliasConfigService
{
  /// <summary>
  /// Tries to resolve <paramref name="input"/> to a weapon_ entity name via the user-defined alias map.
  /// Returns <c>true</c> and sets <paramref name="weaponName"/> when a match is found.
  /// </summary>
  bool TryResolve(string input, out string weaponName);

  /// <summary>
  /// Loads the alias config from disk, creating a default file if none exists.
  /// </summary>
  void LoadOrCreate();

  /// <summary>
  /// All registered alias keys (e.g. "ak", "xm", "deagle").
  /// </summary>
  IReadOnlyCollection<string> AllAliases { get; }
}
