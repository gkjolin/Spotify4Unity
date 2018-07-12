using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track
{
    public enum Resolution
    {
        Small,
        Medium,
        Large
    }

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
    public string ShareURL { get; set; }
    public string InternalCode { get; set; }

    /// <summary>
    /// Total time in seconds the song is
    /// </summary>
    public float TotalTime { get; set; }

    private SpotifyAPI.Local.Models.Track m_originalTrack = null;

    public Track()
    {
        
    }

    public Track(SpotifyAPI.Local.Models.Track t)
    {
        m_originalTrack = t;

        Artist = t.ArtistResource.Name;
        Title = t.TrackResource.Name;
        Album = t.AlbumResource.Name;
        ShareURL = t.TrackResource.Location.Og;

        TotalTime = t.Length;
        InternalCode = t.TrackResource.Uri;
    }

    public string GetAlbumArtUrl(Resolution resolution)
    {
        if(m_originalTrack != null)
        {
            SpotifyAPI.Local.Enums.AlbumArtSize size = SpotifyAPI.Local.Enums.AlbumArtSize.Size160;
            switch (resolution)
            {
                case Resolution.Small:
                    size = SpotifyAPI.Local.Enums.AlbumArtSize.Size160;
                    break;
                case Resolution.Medium:
                    size = SpotifyAPI.Local.Enums.AlbumArtSize.Size320;
                    break;
                case Resolution.Large:
                    size = SpotifyAPI.Local.Enums.AlbumArtSize.Size640;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return m_originalTrack.GetAlbumArtUrl(size);
        }
        return null;
    }
}
