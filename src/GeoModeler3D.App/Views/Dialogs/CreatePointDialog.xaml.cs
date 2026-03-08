using System.Numerics;
using System.Windows;
using GeoModeler3D.App.ViewModels;

namespace GeoModeler3D.App.Views.Dialogs;

public partial class CreatePointDialog : Window
{
    public PointCreationParams? Result { get; private set; }

    public CreatePointDialog()
    {
        InitializeComponent();
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        if (float.TryParse(PosX.Text, out var x) &&
            float.TryParse(PosY.Text, out var y) &&
            float.TryParse(PosZ.Text, out var z))
        {
            Result = new PointCreationParams(new Vector3(x, y, z));
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Please enter valid numeric values.",
                "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
