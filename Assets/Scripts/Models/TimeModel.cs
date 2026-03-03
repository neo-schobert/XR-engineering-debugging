using System;

public class TimeModel
{
    public DateTime CurrentTime { get; private set; }

    public float TimeScale { get; private set; } = 1f;

    public bool IsPlaying { get; private set; } = true;

    public event Action<DateTime> OnTimeChanged;

    public void SetTime(DateTime t)
    {
        CurrentTime = t;
        OnTimeChanged?.Invoke(CurrentTime);
    }

    public void SetScale(float scale)
    {
        TimeScale = scale;
    }

    public void Play()
    {
        IsPlaying = true;
    }

    public void Pause()
    {
        IsPlaying = false;
    }
}
