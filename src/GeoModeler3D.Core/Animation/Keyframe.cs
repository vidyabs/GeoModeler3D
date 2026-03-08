namespace GeoModeler3D.Core.Animation;

/// <summary>A single keyframe with a time and value.</summary>
public record Keyframe<T>(double Time, T Value, EasingType Easing = EasingType.Linear);
