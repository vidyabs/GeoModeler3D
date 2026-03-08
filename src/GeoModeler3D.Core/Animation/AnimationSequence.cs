namespace GeoModeler3D.Core.Animation;

/// <summary>A collection of animation tracks that play together.</summary>
public class AnimationSequence
{
    public string Name { get; set; } = "Animation";
    public List<IAnimationTrack> Tracks { get; } = [];
    public double Duration => Tracks.Count > 0 ? Tracks.Max(t => t.Duration) : 0;
    public double FrameRate { get; set; } = 30;

    public void Apply(double time)
    {
        foreach (var track in Tracks)
            track.Apply(time);
    }
}
