using System.Numerics;

namespace GeoModeler3D.Core.Animation;

/// <summary>Animates entity rotation over time.</summary>
public class RotationTrack : IAnimationTrack
{
    public string TargetProperty => "Rotation";
    public Guid TargetEntityId { get; init; }
    public List<Keyframe<Quaternion>> Keyframes { get; } = [];
    public double Duration => Keyframes.Count > 0 ? Keyframes[^1].Time : 0;

    public void Apply(double time)
    {
        // TODO: slerp between keyframes and apply to entity
    }
}
