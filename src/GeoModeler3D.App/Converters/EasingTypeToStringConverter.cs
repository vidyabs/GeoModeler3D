using System.Globalization;
using System.Windows.Data;
using GeoModeler3D.Core.Animation;

namespace GeoModeler3D.App.Converters;

/// <summary>Converts EasingType enum values to display-friendly strings.</summary>
public class EasingTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EasingType easing)
        {
            return easing switch
            {
                EasingType.Linear => "Linear",
                EasingType.EaseInQuad => "Ease In (Quad)",
                EasingType.EaseOutQuad => "Ease Out (Quad)",
                EasingType.EaseInOutQuad => "Ease In/Out (Quad)",
                EasingType.EaseInCubic => "Ease In (Cubic)",
                EasingType.EaseOutCubic => "Ease Out (Cubic)",
                EasingType.EaseInOutCubic => "Ease In/Out (Cubic)",
                EasingType.EaseInSine => "Ease In (Sine)",
                EasingType.EaseOutSine => "Ease Out (Sine)",
                EasingType.EaseInOutSine => "Ease In/Out (Sine)",
                _ => easing.ToString()
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
