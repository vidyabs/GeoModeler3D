using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.Converters;

public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EntityColor ec)
            return new SolidColorBrush(Color.FromArgb(ec.A, ec.R, ec.G, ec.B));
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
