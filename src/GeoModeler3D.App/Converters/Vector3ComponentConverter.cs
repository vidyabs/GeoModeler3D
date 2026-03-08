using System.Globalization;
using System.Numerics;
using System.Windows.Data;

namespace GeoModeler3D.App.Converters;

/// <summary>
/// Extracts a single component (X, Y, or Z) from a System.Numerics.Vector3.
/// WPF cannot bind to Vector3.X/Y/Z directly because they are fields, not properties.
/// Usage: {Binding Center, Converter={StaticResource Vec3Comp}, ConverterParameter=X}
/// </summary>
public class Vector3ComponentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Vector3 v && parameter is string component)
        {
            return component.ToUpperInvariant() switch
            {
                "X" => v.X.ToString("F3"),
                "Y" => v.Y.ToString("F3"),
                "Z" => v.Z.ToString("F3"),
                _ => string.Empty
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
