using LogSluice.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LogSluice.Services;

public class AppSettings
{
    public List<string> RecentFiles { get; set; } = new();
    public List<HighlightRule> GlobalRules { get; set; } = new();
    public bool FollowTailDefault { get; set; } = true;
    public bool WrapTextDefault { get; set; } = false;
}

public static class SettingsService
{
    private static readonly string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogSluice");
    private static readonly string ConfigFile = Path.Combine(ConfigFolder, "settings.json");

    public static AppSettings Load()
    {
        if (!File.Exists(ConfigFile))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(ConfigFile);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(ConfigFolder);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigFile, json);
    }
}