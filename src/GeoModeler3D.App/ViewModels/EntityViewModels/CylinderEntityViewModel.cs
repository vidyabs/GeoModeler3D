using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.ViewModels.EntityViewModels;

public class CylinderEntityViewModel : EntityViewModelBase
{
    private readonly CylinderEntity _cyl;
    public CylinderEntityViewModel(CylinderEntity cyl) : base(cyl) => _cyl = cyl;

    public double Radius { get => _cyl.Radius; set { _cyl.Radius = value; OnPropertyChanged(); } }
    public double Height { get => _cyl.Height; set { _cyl.Height = value; OnPropertyChanged(); } }
}
