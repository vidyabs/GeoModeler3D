using System.Numerics;
using System.Windows;
using GeoModeler3D.App.ViewModels;

namespace GeoModeler3D.App.Views.Dialogs;

public partial class CreateCylinderDialog : Window
{
    public CylinderCreationParams? Result { get; private set; }

    public CreateCylinderDialog()
    {
        InitializeComponent();
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        if (float.TryParse(CenterX.Text, out var cx) &&
            float.TryParse(CenterY.Text, out var cy) &&
            float.TryParse(CenterZ.Text, out var cz) &&
            float.TryParse(AxisX.Text, out var ax) &&
            float.TryParse(AxisY.Text, out var ay) &&
            float.TryParse(AxisZ.Text, out var az) &&
            double.TryParse(RadiusBox.Text, out var r) && r > 0 &&
            double.TryParse(HeightBox.Text, out var h) && h > 0)
        {
            var axis = new Vector3(ax, ay, az);
            if (axis.LengthSquared() < 1e-6f)
            {
                MessageBox.Show("Axis cannot be zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Result = new CylinderCreationParams(new Vector3(cx, cy, cz), axis, r, h);
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Please enter valid numeric values. Radius and Height must be positive.",
                "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
