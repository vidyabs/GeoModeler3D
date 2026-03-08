using CommunityToolkit.Mvvm.ComponentModel;
using GeoModeler3D.Core.Animation;

namespace GeoModeler3D.App.ViewModels;

/// <summary>ViewModel for the timeline panel (animation playback control).</summary>
public partial class TimelineViewModel : ObservableObject
{
    private readonly AnimationPlayer _player = new();

    [ObservableProperty]
    private double _currentTime;

    [ObservableProperty]
    private double _duration;

    [ObservableProperty]
    private bool _isPlaying;

    public void Play() { _player.Play(); IsPlaying = true; }
    public void Pause() { _player.Pause(); IsPlaying = false; }
    public void Stop() { _player.Stop(); IsPlaying = false; CurrentTime = 0; }
}
