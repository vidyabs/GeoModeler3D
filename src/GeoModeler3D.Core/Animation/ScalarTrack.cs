namespace GeoModeler3D.Core.Animation;

/// <summary>Animates a scalar property (e.g., radius, height) over time.</summary>
public class ScalarTrack : IAnimationTrack
{
    public string TargetProperty { get; init; } = string.Empty;
    public Guid TargetEntityId { get; init; }
    public List<Keyframe<double>> Keyframes { get; } = [];
    public double Duration => Keyframes.Count > 0 ? Keyframes[^1].Time : 0;

    public void Apply(double time)
    {
        // TODO: interpolate and apply scalar value
    }
}
