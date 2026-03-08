using System.Numerics;

namespace GeoModeler3D.Core.Animation;

/// <summary>Animates entity translation over time.</summary>
public class TranslationTrack : IAnimationTrack
{
    public string TargetProperty => "Position";
    public Guid TargetEntityId { get; init; }
    public List<Keyframe<Vector3>> Keyframes { get; } = [];
    public double Duration => Keyframes.Count > 0 ? Keyframes[^1].Time : 0;

    public void Apply(double time)
    {
        // TODO: interpolate between keyframes and apply to entity
    }
}
