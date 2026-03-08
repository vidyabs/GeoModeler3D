using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.ViewModels.EntityViewModels;

public class ConeEntityViewModel : EntityViewModelBase
{
    private readonly ConeEntity _cone;
    public ConeEntityViewModel(ConeEntity cone) : base(cone) => _cone = cone;

    public double BaseRadius { get => _cone.BaseRadius; set { _cone.BaseRadius = value; OnPropertyChanged(); } }
    public double Height { get => _cone.Height; set { _cone.Height = value; OnPropertyChanged(); } }
}
