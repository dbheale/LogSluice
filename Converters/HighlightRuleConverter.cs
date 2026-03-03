using Avalonia.Data.Converters;
using Avalonia.Media;
using LogSluice.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace LogSluice.Converters;

public class HighlightRuleConverter : IMultiValueConverter
{
    public static readonly HighlightRuleConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // Ensure we have both the string line (values[0]) and the rules list (values[1])
        if (values.Count >= 2 &&
            values[0] is string line &&
            values[1] is IEnumerable<HighlightRule> rules)
        {
            foreach (var rule in rules)
            {
                if (string.IsNullOrEmpty(rule.Pattern)) continue;

                // Standard log search: Case-insensitive
                if (line.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase))
                {
                    // Check if the XAML asked for the Foreground or Background color
                    return (parameter as string) == "Foreground"
                        ? rule.Foreground
                        : rule.Background;
                }
            }
        }

        return (parameter as string) == "Foreground" ? Brushes.LightGray : Brushes.Transparent;
    }
}