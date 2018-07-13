using System;

public class TrackTimeChanged : GameEventBase
{
    /// <summary>
    /// The current position of the track in seconds
    /// </summary>
    public float CurrentPosition { get; set; }
    /// <summary>
    /// The current position of the track as a time span
    /// </summary>
    public TimeSpan CurrentPositionSpan { get { return TimeSpan.FromSeconds(CurrentPosition); } }

    /// <summary>
    /// The total time of the track in seconds
    /// </summary>
    public float TotalTime { get; set; }
    /// <summary>
    /// The total time of the track as a time span
    /// </summary>
    public TimeSpan TotalTimeSpan { get { return TimeSpan.FromSeconds(TotalTime); } }

    public TrackTimeChanged(float currentPositionSeconds, float totalTrackTimeSeconds)
    {
        CurrentPosition = currentPositionSeconds;
        TotalTime = totalTrackTimeSeconds;
    }
}