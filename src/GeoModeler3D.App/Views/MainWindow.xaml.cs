using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using GeoModeler3D.App.ViewModels;
using GeoModeler3D.App.Views.Dialogs;
using GeoModeler3D.Core.SceneGraph;
using GeoModeler3D.Rendering;

namespace GeoModeler3D.App.Views;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private ViewportManager? _viewportManager;

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

    // Entity list selection -> SelectionManager
    private void OnEntityListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EntityList.SelectedItem is Core.Entities.IGeometricEntity entity)
        {
            var sm = ViewModel.SelectionManager;
            if (!sm.IsSelected(entity.Id))
                sm.Select(entity.Id);
        }
    }
}
