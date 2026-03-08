using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.App.ViewModels;

public partial class PropertiesPanelViewModel : ObservableObject
{
    private readonly SceneManager _sceneManager;
    private readonly SelectionManager _selectionManager;

    [ObservableProperty]
    private IGeometricEntity? _selectedEntity;

    [ObservableProperty]
    private string _entityTypeName = string.Empty;

    public PropertiesPanelViewModel(SceneManager sceneManager, SelectionManager selectionManager)
    {
        _sceneManager = sceneManager;
        _selectionManager = selectionManager;
        _selectionManager.SelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (SelectedEntity is INotifyPropertyChanged oldEntity)
            oldEntity.PropertyChanged -= OnEntityPropertyChanged;

        if (_selectionManager.SelectedIds.Count > 0)
        {
            SelectedEntity = _sceneManager.GetById(_selectionManager.SelectedIds[0]);
            EntityTypeName = SelectedEntity?.GetType().Name.Replace("Entity", "") ?? string.Empty;
        }
        else
        {
            SelectedEntity = null;
            EntityTypeName = string.Empty;
        }

        if (SelectedEntity is INotifyPropertyChanged newEntity)
            newEntity.PropertyChanged += OnEntityPropertyChanged;
    }

    private void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedEntity));
    }
}
