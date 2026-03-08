using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.ViewModels.EntityViewModels;

public class PointEntityViewModel : EntityViewModelBase
{
    private readonly PointEntity _point;
    public PointEntityViewModel(PointEntity point) : base(point) => _point = point;

    public float X { get => _point.Position.X; set { _point.Position = _point.Position with { X = value }; OnPropertyChanged(); } }
    public float Y { get => _point.Position.Y; set { _point.Position = _point.Position with { Y = value }; OnPropertyChanged(); } }
    public float Z { get => _point.Position.Z; set { _point.Position = _point.Position with { Z = value }; OnPropertyChanged(); } }
}
