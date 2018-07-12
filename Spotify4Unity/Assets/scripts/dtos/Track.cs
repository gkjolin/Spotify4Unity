using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track
{
    /// <summary>
    /// Name of the artist(s) that created the track
    /// </summary>
    public string Artist { get; set; }
    /// <summary>
    /// Title of the track
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Name of the album the song is from
    /// </summary>
    public string Album { get; set; }
    /// <summary>
    /// URl of the song
    /// </summary>
    public string ShareURl { get; set; }

    /// <summary>
    /// Total time in seconds the song is
    /// </summary>
    public float TotalTime { get; set; }

    public Track()
    {

    }

    public Track(SpotifyAPI.Local.Models.Track t)
    {
        Artist = t.ArtistResource.Name;
        Title = t.TrackResource.Name;
        Album = t.AlbumResource.Name;
        ShareURl = t.TrackResource.Location.Og;

        TotalTime = t.Length;
    }
}
