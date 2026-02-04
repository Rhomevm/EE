using System;
using System.IO;
using Newtonsoft.Json;

namespace EELanLauncher.Models;

public class Config
{
    public string? GamePath { get; set; }
    public string? ZeroTierNetworkId { get; set; } = "f3797ba7a8ab2c2b";
    public bool HidePlayConfirmation { get; set; }

    [JsonIgnore]
    public static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EE-LAN-Launcher", "config.json");

    public static Config Load()
    {
        try
        {
            var path = ConfigPath;
            if (!File.Exists(path)) return new Config();
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
        }
        catch
        {
            return new Config();
        }
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(ConfigPath)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
