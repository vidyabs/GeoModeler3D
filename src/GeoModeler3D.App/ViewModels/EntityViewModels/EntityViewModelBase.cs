using CommunityToolkit.Mvvm.ComponentModel;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.ViewModels.EntityViewModels;

/// <summary>Base ViewModel wrapping an entity for property editing.</summary>
public abstract class EntityViewModelBase : ObservableObject
{
    public IGeometricEntity Entity { get; }

    protected EntityViewModelBase(IGeometricEntity entity)
    {
        Entity = entity;
    }

    public string Name
    {
        get => Entity.Name;
        set { Entity.Name = value; OnPropertyChanged(); }
    }

    public bool IsVisible
    {
        get => Entity.IsVisible;
        set { Entity.IsVisible = value; OnPropertyChanged(); }
    }
}
