public class TrackChanged : GameEventBase
{
    public Track NewTrack { get; set; }
    public TrackChanged(Track newT)
    {
        NewTrack = newT;
    }
}
