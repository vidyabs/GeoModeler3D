using System.Numerics;

namespace GeoModeler3D.Core.Animation;

/// <summary>Animates camera position and look direction over time.</summary>
public class CameraTrack : IAnimationTrack
{
    public string TargetProperty => "Camera";
    public Guid TargetEntityId { get; init; }
    public List<Keyframe<Vector3>> PositionKeyframes { get; } = [];
    public List<Keyframe<Vector3>> LookDirectionKeyframes { get; } = [];
    public double Duration => PositionKeyframes.Count > 0 ? PositionKeyframes[^1].Time : 0;

    public void Apply(double time)
    {
        // TODO: interpolate camera position and look direction
    }
}
