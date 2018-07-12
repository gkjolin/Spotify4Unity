public class TrackTimeChanged : GameEventBase
{
    public float CurrentTime { get; set; }
    public float TotalTime { get; set; }
    public TrackTimeChanged(float current, float total)
    {
        CurrentTime = current;
        TotalTime = total;
    }
}