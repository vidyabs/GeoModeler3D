using System.Numerics;
using System.Windows;
using GeoModeler3D.App.ViewModels;

namespace GeoModeler3D.App.Views.Dialogs;

public partial class CreateSphereDialog : Window
{
    public SphereCreationParams? Result { get; private set; }

    public CreateSphereDialog()
    {
        InitializeComponent();
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        if (float.TryParse(CenterX.Text, out var cx) &&
            float.TryParse(CenterY.Text, out var cy) &&
            float.TryParse(CenterZ.Text, out var cz) &&
            double.TryParse(RadiusBox.Text, out var r) && r > 0)
        {
            Result = new SphereCreationParams(new Vector3(cx, cy, cz), r);
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Please enter valid numeric values. Radius must be positive.",
                "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
