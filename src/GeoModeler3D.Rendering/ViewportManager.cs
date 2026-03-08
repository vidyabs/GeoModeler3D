using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering;

public class ViewportManager
{
    private HelixViewport3D? _viewport;
    private GridLinesVisual3D? _gridLines;
    private CoordinateSystemVisual3D? _coordinateSystem;
    private bool _axesVisible = true;

    public HelixViewport3D? Viewport => _viewport;

    public void Initialize(HelixViewport3D viewport)
    {
        _viewport = viewport;

        // Default camera
        viewport.Camera = new PerspectiveCamera
        {
            Position = new Point3D(10, 10, 10),
            LookDirection = new Vector3D(-1, -1, -1),
            UpDirection = new Vector3D(0, 0, 1),
            FieldOfView = 45
        };

        // Default lights
        var lightGroup = new ModelVisual3D();
        var lightModel = new Model3DGroup();
        lightModel.Children.Add(new AmbientLight(Color.FromRgb(80, 80, 80)));
        lightModel.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)));
        lightModel.Children.Add(new DirectionalLight(Color.FromRgb(100, 100, 100), new Vector3D(1, 1, 0.5)));
        lightGroup.Content = lightModel;
        viewport.Children.Add(lightGroup);

        // Grid
        _gridLines = new GridLinesVisual3D
        {
            Width = 20,
            Length = 20,
            MinorDistance = 1,
            MajorDistance = 5,
            Thickness = 0.02
        };
        viewport.Children.Add(_gridLines);

        // Coordinate axes
        _coordinateSystem = new CoordinateSystemVisual3D { ArrowLengths = 2 };
        viewport.Children.Add(_coordinateSystem);
    }

    public void SetGridVisible(bool visible)
    {
        if (_gridLines != null)
            _gridLines.Visible = visible;
    }

    public void SetAxesVisible(bool visible)
    {
        if (_coordinateSystem == null || _viewport == null) return;
        if (visible == _axesVisible) return;

        _axesVisible = visible;
        if (visible)
            _viewport.Children.Add(_coordinateSystem);
        else
            _viewport.Children.Remove(_coordinateSystem);
    }

    public bool IsGridVisible => _gridLines?.Visible ?? false;
    public bool IsAxesVisible => _axesVisible;

    public void ZoomToFit()
    {
        _viewport?.ZoomExtents();
    }

    public void SetCameraView(Point3D position, Vector3D lookDirection, Vector3D upDirection)
    {
        if (_viewport?.Camera is PerspectiveCamera cam)
        {
            cam.Position = position;
            cam.LookDirection = lookDirection;
            cam.UpDirection = upDirection;
        }
    }
}
