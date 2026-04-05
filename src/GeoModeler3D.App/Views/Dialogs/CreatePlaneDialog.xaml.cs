using System.Globalization;
using System.Numerics;
using System.Windows;
using GeoModeler3D.App.ViewModels;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.Views.Dialogs;

public partial class CreatePlaneDialog : Window
{
    private readonly PointEntity? _point;
    private readonly VectorEntity? _vector;

    public PlaneCreationParams? Result { get; private set; }

    public CreatePlaneDialog(PointEntity? point, VectorEntity? vector)
    {
        InitializeComponent();
        _point = point;
        _vector = vector;

        if (point is not null && vector is not null)
        {
            // Both entities available — default to entity mode
            ModeEntities.IsChecked = true;
            PointInfo.Text =
                $"{point.Name}   ({point.Position.X:F3}, {point.Position.Y:F3}, {point.Position.Z:F3})";
            VectorInfo.Text =
                $"{vector.Name}   dir ({vector.Direction.X:F3}, {vector.Direction.Y:F3}, {vector.Direction.Z:F3})";
        }
        else
        {
            // Missing one or both entities — default to manual mode
            ModeManual.IsChecked = true;
            EntityPanel.Visibility = Visibility.Collapsed;
            ManualPanel.Visibility = Visibility.Visible;

            PointInfo.Text = point is not null
                ? $"{point.Name}   ({point.Position.X:F3}, {point.Position.Y:F3}, {point.Position.Z:F3})"
                : "(no point entity selected)";
            VectorInfo.Text = vector is not null
                ? $"{vector.Name}   dir ({vector.Direction.X:F3}, {vector.Direction.Y:F3}, {vector.Direction.Z:F3})"
                : "(no vector entity selected)";

            // Pre-fill origin from point if available
            if (point is not null)
            {
                OriginX.Text = point.Position.X.ToString("F3", CultureInfo.InvariantCulture);
                OriginY.Text = point.Position.Y.ToString("F3", CultureInfo.InvariantCulture);
                OriginZ.Text = point.Position.Z.ToString("F3", CultureInfo.InvariantCulture);
            }

            // Pre-fill normal from vector if available
            if (vector is not null)
            {
                NormI.Text = vector.Direction.X.ToString("F3", CultureInfo.InvariantCulture);
                NormJ.Text = vector.Direction.Y.ToString("F3", CultureInfo.InvariantCulture);
                NormK.Text = vector.Direction.Z.ToString("F3", CultureInfo.InvariantCulture);
            }
        }
    }

    private void OnModeChanged(object sender, RoutedEventArgs e)
    {
        if (EntityPanel is null || ManualPanel is null) return;
        bool entityMode = ModeEntities.IsChecked == true;
        EntityPanel.Visibility = entityMode ? Visibility.Visible : Visibility.Collapsed;
        ManualPanel.Visibility = entityMode ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnCreate(object sender, RoutedEventArgs e)
    {
        if (ModeEntities.IsChecked == true)
        {
            if (_point is null || _vector is null)
            {
                MessageBox.Show(
                    "A point entity and a vector entity must both be selected to use this mode.\n\n" +
                    "Ctrl+click a point and a vector in the entity list, then open Create > Plane.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var normal = _vector.Direction;
            if (normal.Length() < 1e-6f)
            {
                MessageBox.Show(
                    "The selected vector has zero length and cannot be used as a plane normal.",
                    "Invalid Vector", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Result = new PlaneCreationParams(_point.Position, normal);
            DialogResult = true;
        }
        else
        {
            if (!TryParseFloat(OriginX.Text, out var ox) ||
                !TryParseFloat(OriginY.Text, out var oy) ||
                !TryParseFloat(OriginZ.Text, out var oz) ||
                !TryParseFloat(NormI.Text, out var ni) ||
                !TryParseFloat(NormJ.Text, out var nj) ||
                !TryParseFloat(NormK.Text, out var nk))
            {
                MessageBox.Show("Please enter valid numeric values for all fields.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var normal = new Vector3(ni, nj, nk);
            if (normal.Length() < 1e-6f)
            {
                MessageBox.Show("Normal (I, J, K) must not be a zero vector.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Result = new PlaneCreationParams(new Vector3(ox, oy, oz), normal);
            DialogResult = true;
        }
    }

    private static bool TryParseFloat(string text, out float value) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
