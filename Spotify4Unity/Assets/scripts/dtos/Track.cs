using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track
{
    public string Artist { get; set; }
    public string Title { get; set; }
    public string Album { get; set; }
    public float TotalTime { get; set; }

    public Track()
    {

    }

    public Track(SpotifyAPI.Local.Models.Track t)
    {
        Artist = t.ArtistResource.Name;
        Title = t.TrackResource.Name;
        Album = t.AlbumResource.Name;
        TotalTime = t.Length;
    }
}
