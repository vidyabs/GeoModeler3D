using CommunityToolkit.Mvvm.ComponentModel;
using GeoModeler3D.Rendering;

namespace GeoModeler3D.App.ViewModels;

/// <summary>ViewModel for viewport state: camera, display mode, grid/axes visibility.</summary>
public partial class ViewportViewModel : ObservableObject
{
    [ObservableProperty]
    private DisplayMode _displayMode = DisplayMode.Shaded;

    [ObservableProperty]
    private bool _showGrid = true;

    [ObservableProperty]
    private bool _showAxes = true;

    [ObservableProperty]
    private bool _showViewCube = true;

    [ObservableProperty]
    private string _cameraPreset = "Isometric";
}
