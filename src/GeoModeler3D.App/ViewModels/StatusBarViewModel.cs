using CommunityToolkit.Mvvm.ComponentModel;
using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.App.ViewModels;

public partial class StatusBarViewModel : ObservableObject
{
    private readonly SceneManager _sceneManager;
    private readonly SelectionManager _selectionManager;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private int _entityCount;

    [ObservableProperty]
    private int _selectedCount;

    public StatusBarViewModel(SceneManager sceneManager, SelectionManager selectionManager)
    {
        _sceneManager = sceneManager;
        _selectionManager = selectionManager;

        _sceneManager.EntityAdded += _ => EntityCount = _sceneManager.Entities.Count;
        _sceneManager.EntityRemoved += _ => EntityCount = _sceneManager.Entities.Count;
        _selectionManager.SelectionChanged += () => SelectedCount = _selectionManager.SelectedIds.Count;
    }
}
