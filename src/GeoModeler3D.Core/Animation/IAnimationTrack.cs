namespace GeoModeler3D.Core.Animation;

/// <summary>A track that animates a single property over time.</summary>
public interface IAnimationTrack
{
    string TargetProperty { get; }
    Guid TargetEntityId { get; }
    double Duration { get; }
    void Apply(double time);
}
