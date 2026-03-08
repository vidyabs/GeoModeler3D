using System.Numerics;
using System.Windows;
using GeoModeler3D.App.ViewModels;

namespace GeoModeler3D.App.Views.Dialogs;

public partial class CreateTorusDialog : Window
{
    public TorusCreationParams? Result { get; private set; }

    public CreateTorusDialog()
    {
        InitializeComponent();
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        if (float.TryParse(CenterX.Text, out var cx) &&
            float.TryParse(CenterY.Text, out var cy) &&
            float.TryParse(CenterZ.Text, out var cz) &&
            float.TryParse(NormalX.Text, out var nx) &&
            float.TryParse(NormalY.Text, out var ny) &&
            float.TryParse(NormalZ.Text, out var nz) &&
            double.TryParse(MajorRadiusBox.Text, out var major) && major > 0 &&
            double.TryParse(MinorRadiusBox.Text, out var minor) && minor > 0)
        {
            var normal = new Vector3(nx, ny, nz);
            if (normal.LengthSquared() < 1e-6f)
            {
                MessageBox.Show("Normal cannot be zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Result = new TorusCreationParams(new Vector3(cx, cy, cz), normal, major, minor);
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Please enter valid numeric values. Radii must be positive.",
                "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
