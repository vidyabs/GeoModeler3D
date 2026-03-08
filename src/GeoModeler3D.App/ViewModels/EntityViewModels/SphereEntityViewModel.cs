using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.ViewModels.EntityViewModels;

public class SphereEntityViewModel : EntityViewModelBase
{
    private readonly SphereEntity _sphere;
    public SphereEntityViewModel(SphereEntity sphere) : base(sphere) => _sphere = sphere;

    public float CenterX { get => _sphere.Center.X; set { _sphere.Center = _sphere.Center with { X = value }; OnPropertyChanged(); } }
    public float CenterY { get => _sphere.Center.Y; set { _sphere.Center = _sphere.Center with { Y = value }; OnPropertyChanged(); } }
    public float CenterZ { get => _sphere.Center.Z; set { _sphere.Center = _sphere.Center with { Z = value }; OnPropertyChanged(); } }
    public double Radius { get => _sphere.Radius; set { _sphere.Radius = value; OnPropertyChanged(); } }
}
