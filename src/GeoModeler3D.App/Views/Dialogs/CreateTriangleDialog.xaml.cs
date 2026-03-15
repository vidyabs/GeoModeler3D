using System.Numerics;
using System.Windows;
using GeoModeler3D.App.ViewModels;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.Views.Dialogs;

public partial class CreateTriangleDialog : Window
{
    private readonly PointEntity _p0;
    private readonly PointEntity _p1;
    private readonly PointEntity _p2;

    public TriangleCreationParams? Result { get; private set; }

    public CreateTriangleDialog(PointEntity p0, PointEntity p1, PointEntity p2)
    {
        InitializeComponent();
        _p0 = p0;
        _p1 = p1;
        _p2 = p2;

        Point0Info.Text = $"{p0.Name}   ({p0.Position.X:F3}, {p0.Position.Y:F3}, {p0.Position.Z:F3})";
        Point1Info.Text = $"{p1.Name}   ({p1.Position.X:F3}, {p1.Position.Y:F3}, {p1.Position.Z:F3})";
        Point2Info.Text = $"{p2.Name}   ({p2.Position.X:F3}, {p2.Position.Y:F3}, {p2.Position.Z:F3})";
    }

    private void OnCreate(object sender, RoutedEventArgs e)
    {
        var v0 = _p0.Position;
        var v1 = _p1.Position;
        var v2 = _p2.Position;

        // Ensure the three positions are distinct
        if (v0 == v1 || v1 == v2 || v0 == v2)
        {
            MessageBox.Show(
                "Two or more selected points occupy the same position. A triangle requires 3 distinct points.",
                "Invalid Points", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Ensure the three points are not collinear
        var cross = Vector3.Cross(v1 - v0, v2 - v0);
        if (cross.Length() < 1e-6f)
        {
            MessageBox.Show(
                "The three selected points are collinear and cannot form a valid triangle.",
                "Invalid Points", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Result = new TriangleCreationParams(v0, v1, v2);
        DialogResult = true;
    }
}
