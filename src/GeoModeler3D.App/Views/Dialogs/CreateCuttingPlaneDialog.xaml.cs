using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using GeoModeler3D.App.ViewModels;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.Views.Dialogs;

public partial class CreateCuttingPlaneDialog : Window
{
    public CuttingPlaneCreationParams? Result { get; private set; }

    public CreateCuttingPlaneDialog(IEnumerable<IGeometricEntity> sceneEntities)
    {
        InitializeComponent();

        foreach (var entity in sceneEntities)
        {
            TargetList.Items.Add(new ListBoxItem
            {
                Content = $"{entity.Name} ({entity.GetType().Name.Replace("Entity", "")})",
                Tag = entity.Id
            });
        }
    }

    private void OnOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OpacityLabel is not null)
            OpacityLabel.Text = e.NewValue.ToString("F2", CultureInfo.InvariantCulture);
    }

    private void OnClipSideChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GapRow is null || ClipSideCombo.SelectedItem is not ComboBoxItem selected) return;
        GapRow.Visibility = (string)selected.Tag == "BothWithGap"
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void OnCreate(object sender, RoutedEventArgs e)
    {
        if (!TryParseFloat(OriginX.Text, out var ox) ||
            !TryParseFloat(OriginY.Text, out var oy) ||
            !TryParseFloat(OriginZ.Text, out var oz) ||
            !TryParseFloat(NormI.Text, out var ni) ||
            !TryParseFloat(NormJ.Text, out var nj) ||
            !TryParseFloat(NormK.Text, out var nk) ||
            !TryParseDouble(DisplayWidth.Text, out var dw) ||
            !TryParseDouble(DisplayHeight.Text, out var dh))
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

        if (dw <= 0 || dh <= 0)
        {
            MessageBox.Show("Width and Height must be positive.",
                "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var clipSideTag = ClipSideCombo.SelectedItem is ComboBoxItem cbi ? (string)cbi.Tag : "None";
        var clipSide = Enum.Parse<ClipSide>(clipSideTag);

        double gapDistance = 0.5;
        if (clipSide == ClipSide.BothWithGap)
        {
            if (!TryParseDouble(GapDistanceBox.Text, out gapDistance) || gapDistance <= 0)
            {
                MessageBox.Show("Gap distance must be a positive number.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        var targetIds = TargetList.SelectedItems
            .OfType<ListBoxItem>()
            .Select(item => (Guid)item.Tag)
            .ToList();

        Result = new CuttingPlaneCreationParams(
            Origin: new Vector3(ox, oy, oz),
            Normal: normal,
            DisplayWidth: dw,
            DisplayHeight: dh,
            Opacity: OpacitySlider.Value,
            IsCappingEnabled: CappingCheck.IsChecked == true,
            ClipSide: clipSide,
            GapDistance: gapDistance,
            TargetEntityIds: targetIds);

        DialogResult = true;
    }

    private static bool TryParseFloat(string text, out float value) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    private static bool TryParseDouble(string text, out double value) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
