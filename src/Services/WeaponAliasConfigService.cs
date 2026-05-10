using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2_Retakes.Interfaces;

namespace SwiftlyS2_Retakes.Services;

/// <summary>
/// Loads user-defined weapon aliases from resources/guns.jsonc (created automatically on first run).
/// The file format is a JSON object mapping weapon entity name → list of aliases, e.g.:
///   { "weapon_ak47": ["ak", "ak47"], "weapon_m4a1_silencer": ["m4a1s", "m4s"] }
/// </summary>
public sealed class WeaponAliasConfigService : IWeaponAliasConfigService
{
  private readonly ISwiftlyCore _core;
  private readonly ILogger _logger;

  private Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);

  private static readonly JsonSerializerOptions JsonReadOptions = new()
  {
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    PropertyNameCaseInsensitive = true,
  };

  public WeaponAliasConfigService(ISwiftlyCore core, ILogger logger)
  {
    _core = core;
    _logger = logger;
    LoadOrCreate();
  }

  public IReadOnlyCollection<string> AllAliases => _aliases.Keys;

  public bool TryResolve(string input, out string weaponName)
  {
    if (_aliases.TryGetValue(input, out var found))
    {
      weaponName = found;
      return true;
    }

    weaponName = string.Empty;
    return false;
  }

  public void LoadOrCreate()
  {
    var path = _core.Configuration.GetConfigPath("guns.jsonc");

    if (!File.Exists(path))
    {
      CreateDefault(path);
    }

    try
    {
      var text = File.ReadAllText(path);
      var parsed = JsonSerializer.Deserialize<Dictionary<string, string[]>>(text, JsonReadOptions);
      if (parsed is not null)
      {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (weapon, aliases) in parsed)
        {
          foreach (var alias in aliases)
          {
            if (!string.IsNullOrWhiteSpace(alias))
              map[alias.Trim()] = weapon;
          }
        }
        _aliases = map;
        _logger.LogInformation("Retakes: loaded {Count} weapon alias(es) from guns.jsonc", _aliases.Count);
      }
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Retakes: failed to load guns.jsonc, using empty alias map");
      _aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
  }

  private void CreateDefault(string path)
  {
    try
    {
      var dir = Path.GetDirectoryName(path);
      if (dir is not null)
        Directory.CreateDirectory(dir);

      const string defaultContent = """
{
  "weapon_ak47":          ["ak", "ak47"],
  "weapon_m4a1":          ["m4a4", "m4"],
  "weapon_m4a1_silencer": ["m4a1s", "m4s"],
  "weapon_aug":           ["aug"],
  "weapon_sg556":         ["sg", "sg556", "krieg"],
  "weapon_galilar":       ["galil", "galilar"],
  "weapon_famas":         ["famas"],
  "weapon_awp":           ["awp", "sniper"],
  "weapon_ssg08":         ["scout", "ssg", "ssg08"],
  "weapon_scar20":        ["scar20", "scar"],
  "weapon_g3sg1":         ["g3sg1", "g3"],
  "weapon_mp9":           ["mp9"],
  "weapon_mp7":           ["mp7"],
  "weapon_mp5sd":         ["mp5", "mp5sd"],
  "weapon_mac10":         ["mac10", "mac"],
  "weapon_ump45":         ["ump", "ump45"],
  "weapon_p90":           ["p90"],
  "weapon_bizon":         ["bizon"],
  "weapon_nova":          ["nova"],
  "weapon_xm1014":        ["xm1014", "xm"],
  "weapon_mag7":          ["mag7", "mag"],
  "weapon_sawedoff":      ["sawedoff", "sawed"],
  "weapon_m249":          ["m249"],
  "weapon_negev":         ["negev"],
  "weapon_deagle":        ["deagle", "de"],
  "weapon_elite":         ["elite", "dualies"],
  "weapon_fiveseven":     ["fiveseven", "five7"],
  "weapon_glock":         ["glock"],
  "weapon_hkp2000":       ["p2000", "hkp2000"],
  "weapon_p250":          ["p250"],
  "weapon_usp_silencer":  ["usp", "usps"],
  "weapon_tec9":          ["tec9", "tec"],
  "weapon_cz75a":         ["cz75", "cz"],
  "weapon_revolver":      ["revolver", "r8"]
}
""";

      File.WriteAllText(path, defaultContent);
      _logger.LogInformation("Retakes: created default guns.jsonc at {Path}", path);
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Retakes: failed to create default guns.jsonc");
    }
  }
}
