using System.Globalization;
using System.Numerics;
using System.Windows;
using GeoModeler3D.App.ViewModels;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.Views.Dialogs;

public partial class CreateVectorDialog : Window
{
    private readonly PointEntity? _p0;
    private readonly PointEntity? _p1;

    public VectorCreationParams? Result { get; private set; }

    public CreateVectorDialog(PointEntity? p0, PointEntity? p1)
    {
        InitializeComponent();
        _p0 = p0;
        _p1 = p1;

        if (p0 is not null && p1 is not null)
        {
            // Both points available — default to two-points mode
            ModeTwoPoints.IsChecked = true;
            Point0Info.Text = $"{p0.Name}   ({p0.Position.X:F3}, {p0.Position.Y:F3}, {p0.Position.Z:F3})";
            Point1Info.Text = $"{p1.Name}   ({p1.Position.X:F3}, {p1.Position.Y:F3}, {p1.Position.Z:F3})";
        }
        else
        {
            // Not enough points — default to manual mode
            ModeManual.IsChecked = true;
            TwoPointsPanel.Visibility = Visibility.Collapsed;
            ManualPanel.Visibility = Visibility.Visible;

            // Pre-fill origin if one point is available
            if (p0 is not null)
            {
                OriginX.Text = p0.Position.X.ToString("F3", CultureInfo.InvariantCulture);
                OriginY.Text = p0.Position.Y.ToString("F3", CultureInfo.InvariantCulture);
                OriginZ.Text = p0.Position.Z.ToString("F3", CultureInfo.InvariantCulture);
            }

            Point0Info.Text = p0 is not null
                ? $"{p0.Name}   ({p0.Position.X:F3}, {p0.Position.Y:F3}, {p0.Position.Z:F3})"
                : "(no point selected)";
            Point1Info.Text = "(no point selected)";
        }
    }

    private void OnModeChanged(object sender, RoutedEventArgs e)
    {
        if (TwoPointsPanel is null || ManualPanel is null) return;
        bool twoPoints = ModeTwoPoints.IsChecked == true;
        TwoPointsPanel.Visibility = twoPoints ? Visibility.Visible : Visibility.Collapsed;
        ManualPanel.Visibility = twoPoints ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnCreate(object sender, RoutedEventArgs e)
    {
        if (ModeTwoPoints.IsChecked == true)
        {
            if (_p0 is null || _p1 is null)
            {
                MessageBox.Show(
                    "Two point entities must be selected to use this mode.\n\n" +
                    "Ctrl+click two points in the entity list, then open Create > Vector.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var origin = _p0.Position;
            var direction = _p1.Position - _p0.Position;

            if (direction.Length() < 1e-6f)
            {
                MessageBox.Show(
                    "The two selected points are at the same position. The direction vector would be zero.",
                    "Invalid Points", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Result = new VectorCreationParams(origin, direction);
            DialogResult = true;
        }
        else
        {
            if (!TryParseFloat(OriginX.Text, out var ox) ||
                !TryParseFloat(OriginY.Text, out var oy) ||
                !TryParseFloat(OriginZ.Text, out var oz) ||
                !TryParseFloat(DirI.Text, out var di) ||
                !TryParseFloat(DirJ.Text, out var dj) ||
                !TryParseFloat(DirK.Text, out var dk))
            {
                MessageBox.Show("Please enter valid numeric values for all fields.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var direction = new Vector3(di, dj, dk);
            if (direction.Length() < 1e-6f)
            {
                MessageBox.Show("Direction (I, J, K) must not be a zero vector.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Result = new VectorCreationParams(new Vector3(ox, oy, oz), direction);
            DialogResult = true;
        }
    }

    private static bool TryParseFloat(string text, out float value) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
