using System.Collections.Generic;

public class SavedTracksLoaded : GameEventBase
{
    public List<Track> SavedTracks { get; set; }
    public SavedTracksLoaded(List<Track> t)
    {
        SavedTracks = t;
    }
}
