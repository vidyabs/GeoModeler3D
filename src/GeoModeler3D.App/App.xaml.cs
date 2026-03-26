using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using GeoModeler3D.Core.Commands;
using GeoModeler3D.Core.SceneGraph;
using GeoModeler3D.Core.Serialization;
using GeoModeler3D.Rendering;
using GeoModeler3D.Rendering.EntityRenderers;
using GeoModeler3D.App.Services;
using GeoModeler3D.App.ViewModels;
using GeoModeler3D.App.Views;
using Serilog;

namespace GeoModeler3D.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/geomodeler3d-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("GeoModeler3D starting up");

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        var propertiesViewModel = _serviceProvider.GetRequiredService<PropertiesPanelViewModel>();

        mainWindow.DataContext = mainViewModel;
        mainWindow.PropertiesPanelControl.DataContext = propertiesViewModel;

        // Initialize rendering pipeline
        var renderingService = _serviceProvider.GetRequiredService<IRenderingService>();
        var viewportManager = _serviceProvider.GetRequiredService<ViewportManager>();
        var selectionManager = _serviceProvider.GetRequiredService<SelectionManager>();
        var sceneManager = _serviceProvider.GetRequiredService<SceneManager>();

        mainWindow.InitializeServices(renderingService, viewportManager, selectionManager, sceneManager);

        mainWindow.Show();
        Log.Information("GeoModeler3D started successfully");
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core singletons
        services.AddSingleton<SceneManager>();
        services.AddSingleton<SelectionManager>();
        services.AddSingleton<LayerManager>();
        services.AddSingleton<UndoManager>();
        services.AddSingleton<ProjectSerializer>();

        // Rendering
        services.AddSingleton<EntityRendererRegistry>(sp =>
        {
            var registry = new EntityRendererRegistry();
            registry.Register(new PointEntityRenderer());
            registry.Register(new SphereEntityRenderer());
            registry.Register(new CylinderEntityRenderer());
            registry.Register(new ConeEntityRenderer());
            registry.Register(new TorusEntityRenderer());
            registry.Register(new CircleEntityRenderer());
            registry.Register(new TriangleEntityRenderer());
            registry.Register(new MeshEntityRenderer());
            registry.Register(new CuttingPlaneEntityRenderer());
            registry.Register(new ContourCurveEntityRenderer());
            return registry;
        });
        services.AddSingleton<SelectionHighlighter>();
        services.AddSingleton<IRenderingService, RenderingService>();
        services.AddSingleton<ViewportManager>();

        // App services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IFileDialogService, FileDialogService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<PropertiesPanelViewModel>();
        services.AddSingleton<StatusBarViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("GeoModeler3D shutting down");
        Log.CloseAndFlush();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
