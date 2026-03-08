using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.App.ViewModels.EntityViewModels;

public class TorusEntityViewModel : EntityViewModelBase
{
    private readonly TorusEntity _torus;
    public TorusEntityViewModel(TorusEntity torus) : base(torus) => _torus = torus;

    public double MajorRadius { get => _torus.MajorRadius; set { _torus.MajorRadius = value; OnPropertyChanged(); } }
    public double MinorRadius { get => _torus.MinorRadius; set { _torus.MinorRadius = value; OnPropertyChanged(); } }
}
