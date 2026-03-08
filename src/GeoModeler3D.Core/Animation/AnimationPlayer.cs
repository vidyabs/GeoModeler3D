namespace GeoModeler3D.Core.Animation;

/// <summary>Controls playback of an animation sequence.</summary>
public class AnimationPlayer
{
    public AnimationSequence? CurrentSequence { get; private set; }
    public double CurrentTime { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsLooping { get; set; }

    public event Action<double>? TimeChanged;

    public void Load(AnimationSequence sequence)
    {
        CurrentSequence = sequence;
        CurrentTime = 0;
    }

    public void Play() { IsPlaying = true; }
    public void Pause() { IsPlaying = false; }
    public void Stop() { IsPlaying = false; CurrentTime = 0; }

    public void Seek(double time)
    {
        CurrentTime = time;
        CurrentSequence?.Apply(time);
        TimeChanged?.Invoke(time);
    }

    public void Update(double deltaTime)
    {
        if (!IsPlaying || CurrentSequence is null) return;
        CurrentTime += deltaTime;
        if (CurrentTime > CurrentSequence.Duration)
            CurrentTime = IsLooping ? 0 : CurrentSequence.Duration;
        CurrentSequence.Apply(CurrentTime);
        TimeChanged?.Invoke(CurrentTime);
    }
}
