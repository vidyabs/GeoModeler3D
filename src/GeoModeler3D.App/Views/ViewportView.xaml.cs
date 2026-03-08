using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.SceneGraph;
using GeoModeler3D.Rendering;
using HelixToolkit.Wpf;

namespace GeoModeler3D.App.Views;

public partial class ViewportView : UserControl
{
    private IRenderingService? _renderingService;
    private ViewportManager? _viewportManager;
    private SelectionManager? _selectionManager;

    public ViewportView()
    {
        InitializeComponent();
        Viewport.MouseLeftButtonDown += OnViewportMouseDown;
    }

    public void Initialize(
        IRenderingService renderingService,
        ViewportManager viewportManager,
        SelectionManager selectionManager)
    {
        _renderingService = renderingService;
        _viewportManager = viewportManager;
        _selectionManager = selectionManager;

        _viewportManager.Initialize(Viewport);
        _renderingService.Initialize(Viewport);

        _selectionManager.SelectionChanged += () =>
            _renderingService.HighlightEntities(_selectionManager.SelectedIds);
    }

    public HelixViewport3D HelixViewport => Viewport;

    private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_renderingService is null || _selectionManager is null) return;

        var pos = e.GetPosition(Viewport);
        var hits = Viewport.Viewport.FindHits(pos);

        if (hits.Count > 0)
        {
            foreach (var hit in hits)
            {
                var entityId = _renderingService.GetEntityIdFromVisual(hit.Visual);
                if (entityId.HasValue)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        _selectionManager.ToggleSelect(entityId.Value);
                    else
                        _selectionManager.Select(entityId.Value);
                    return;
                }
            }
        }

        _selectionManager.ClearSelection();
    }
}
