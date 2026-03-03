using System.Text.Json.Serialization;
using Avalonia.Media;

namespace LogSluice.Models;
public class HighlightRule
{
    public string Pattern { get; set; } = string.Empty;
    
    // The string we actually save to JSON
    public string BackgroundColorName { get; set; } = "#FFFF00"; 
    public string ForegroundColorName { get; set; } = "#000000";

    // The Brush the UI uses (ignored by the JSON saver)
    [JsonIgnore] 
    public IBrush Background => new SolidColorBrush(Color.Parse(BackgroundColorName));
    
    [JsonIgnore] 
    public IBrush Foreground => new SolidColorBrush(Color.Parse(ForegroundColorName));
}