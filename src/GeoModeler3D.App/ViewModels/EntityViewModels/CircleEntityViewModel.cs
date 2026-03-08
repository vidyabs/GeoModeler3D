using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.ViewModels.EntityViewModels;

public class CircleEntityViewModel : EntityViewModelBase
{
    private readonly CircleEntity _circle;
    public CircleEntityViewModel(CircleEntity circle) : base(circle) => _circle = circle;

    public double Radius { get => _circle.Radius; set { _circle.Radius = value; OnPropertyChanged(); } }
}
