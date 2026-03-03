using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace LogSluice.Converters;

public class TextWrapConverter : IValueConverter
{
    // A static instance makes it incredibly easy to use in XAML
    public static readonly TextWrapConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? TextWrapping.Wrap : TextWrapping.NoWrap;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}