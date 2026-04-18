using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using GeoModeler3D.App.ViewModels;
using GeoModeler3D.App.Views.Dialogs;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;
using GeoModeler3D.Rendering;

namespace GeoModeler3D.App.Views;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private ViewportManager? _viewportManager;

    // Guard against feedback loops between the entity ListBox and SelectionManager
    private int _selectionSyncDepth = 0;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void InitializeServices(
        IRenderingService renderingService,
        ViewportManager viewportManager,
        SelectionManager selectionManager,
        SceneManager sceneManager)
    {
        _viewportManager = viewportManager;
        ViewportControl.Initialize(renderingService, viewportManager, selectionManager);

        // Sync SelectionManager → entity ListBox (e.g. when viewport click changes selection)
        selectionManager.SelectionChanged += () =>
        {
            if (_selectionSyncDepth > 0) return;
            _selectionSyncDepth++;
            try
            {
                EntityList.SelectedItems.Clear();
                foreach (var id in selectionManager.SelectedIds)
                {
                    var entity = sceneManager.GetById(id);
                    if (entity != null) EntityList.SelectedItems.Add(entity);
                }
            }
            finally
            {
                _selectionSyncDepth--;
            }
        };

        // Wire SceneManager events to RenderingService
        sceneManager.EntityAdded += entity => renderingService.AddEntity(entity);
        sceneManager.EntityChanged += entity => renderingService.UpdateEntity(entity);
        sceneManager.EntityRemoved += id => renderingService.RemoveEntity(id);
    }

    // Menu: File
    private void OnExit(object sender, RoutedEventArgs e) => Close();

    // Menu: Create
    private void OnCreateSphere(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateSphereDialog { Owner = this };
        if (dialog.ShowDialog() == true)
            ViewModel.CreateSphereCommand.Execute(dialog.Result);
    }

    private void OnCreateCylinder(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateCylinderDialog { Owner = this };
        if (dialog.ShowDialog() == true)
            ViewModel.CreateCylinderCommand.Execute(dialog.Result);
    }

    private void OnCreateCone(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateConeDialog { Owner = this };
        if (dialog.ShowDialog() == true)
            ViewModel.CreateConeCommand.Execute(dialog.Result);
    }

    private void OnCreateTorus(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateTorusDialog { Owner = this };
        if (dialog.ShowDialog() == true)
            ViewModel.CreateTorusCommand.Execute(dialog.Result);
    }

    private void OnCreatePoint(object sender, RoutedEventArgs e)
    {
        var dialog = new CreatePointDialog { Owner = this };
        if (dialog.ShowDialog() == true)
            ViewModel.CreatePointCommand.Execute(dialog.Result);
    }

    private void OnCreateTriangle(object sender, RoutedEventArgs e)
    {
        var sm = ViewModel.SelectionManager;
        var scene = ViewModel.SceneManager;

        if (sm.SelectedIds.Count != 3)
        {
            MessageBox.Show(
                "Please select exactly 3 point entities to create a triangle.\n\n" +
                "Hold Ctrl and click entities in the list to multi-select.",
                "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var points = sm.SelectedIds
            .Select(id => scene.GetById(id))
            .OfType<PointEntity>()
            .ToList();

        if (points.Count != 3)
        {
            MessageBox.Show(
                "All 3 selected entities must be Point entities.\n\n" +
                $"Currently selected: {string.Join(", ", sm.SelectedIds.Select(id => scene.GetById(id)?.GetType().Name.Replace("Entity", "") ?? "?"))}.",
                "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new CreateTriangleDialog(points[0], points[1], points[2]) { Owner = this };
        if (dialog.ShowDialog() == true)
            ViewModel.CreateTriangleCommand.Execute(dialog.Result);
    }

    private void OnCreateVector(object sender, RoutedEventArgs e)
    {
        var sm = ViewModel.SelectionManager;
        var scene = ViewModel.SceneManager;

        var selectedPoints = sm.SelectedIds
            .Select(id => scene.GetById(id))
            .OfType<PointEntity>()
            .Take(2)
            .ToList();

        var p0 = selectedPoints.Count > 0 ? selectedPoints[0] : null;
        var p1 = selectedPoints.Count > 1 ? selectedPoints[1] : null;

        var dialog = new CreateVectorDialog(p0, p1) { Owner = this };
        if (dialog.ShowDialog() == true)
            ViewModel.CreateVectorCommand.Execute(dialog.Result);
    }

    private void OnCreatePlane(object sender, RoutedEventArgs e)
    {
        var sm = ViewModel.SelectionManager;
        var scene = ViewModel.SceneManager;

        var point = sm.SelectedIds
            .Select(id => scene.GetById(id))
            .OfType<PointEntity>()
            .FirstOrDefault();
        var vector = sm.SelectedIds
            .Select(id => scene.GetById(id))
            .OfType<VectorEntity>()
            .FirstOrDefault();

        var dialog = new CreatePlaneDialog(point, vector) { Owner = this };
        if (dialog.ShowDialog() == true)
            ViewModel.CreatePlaneCommand.Execute(dialog.Result);
    }

    private void OnCreateCuttingPlane(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateCuttingPlaneDialog(ViewModel.SceneManager.Entities) { Owner = this };
        if (dialog.ShowDialog() == true)
            ViewModel.CreateCuttingPlaneCommand.Execute(dialog.Result);
    }

    // Menu: View
    private void OnZoomToFit(object sender, RoutedEventArgs e) => _viewportManager?.ZoomToFit();

    private void OnToggleGrid(object sender, RoutedEventArgs e)
    {
        if (_viewportManager is null) return;
        _viewportManager.SetGridVisible(!_viewportManager.IsGridVisible);
    }

    private void OnToggleAxes(object sender, RoutedEventArgs e)
    {
        if (_viewportManager is null) return;
        _viewportManager.SetAxesVisible(!_viewportManager.IsAxesVisible);
    }

    private void OnTopView(object sender, RoutedEventArgs e) =>
        _viewportManager?.SetCameraView(
            new Point3D(0, 0, 15), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0));

    private void OnFrontView(object sender, RoutedEventArgs e) =>
        _viewportManager?.SetCameraView(
            new Point3D(0, -15, 0), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1));

    private void OnRightView(object sender, RoutedEventArgs e) =>
        _viewportManager?.SetCameraView(
            new Point3D(15, 0, 0), new Vector3D(-1, 0, 0), new Vector3D(0, 0, 1));

    private void OnIsometricView(object sender, RoutedEventArgs e) =>
        _viewportManager?.SetCameraView(
            new Point3D(10, 10, 10), new Vector3D(-1, -1, -1), new Vector3D(0, 0, 1));

    // Menu: Help
    private void OnAbout(object sender, RoutedEventArgs e)
    {
        var dialog = new AboutDialog { Owner = this };
        dialog.ShowDialog();
    }

    // Entity list selection → SelectionManager
    private void OnEntityListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selectionSyncDepth > 0) return;
        _selectionSyncDepth++;
        try
        {
            var sm = ViewModel.SelectionManager;
            // Only process the delta to avoid re-entrancy side-effects
            foreach (IGeometricEntity entity in e.RemovedItems.OfType<IGeometricEntity>())
                if (sm.IsSelected(entity.Id)) sm.ToggleSelect(entity.Id);
            foreach (IGeometricEntity entity in e.AddedItems.OfType<IGeometricEntity>())
                if (!sm.IsSelected(entity.Id)) sm.AddToSelection(entity.Id);
        }
        finally
        {
            _selectionSyncDepth--;
        }
    }
}
