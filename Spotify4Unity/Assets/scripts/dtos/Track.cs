using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track
{
    public enum Resolution
    {
        /// <summary>
        /// 160x160
        /// </summary>
        Small,
        /// <summary>
        /// 320x320
        /// </summary>
        Medium,
        /// <summary>
        /// 640x640
        /// </summary>
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
    /// Url of the song
    /// </summary>
    public string TrackURL { get; set; }

    /// <summary>
    /// The URI of the track
    /// </summary>
    public string TrackUri { get; set; }
    /// <summary>
    /// The URI of the album
    /// </summary>
    public string AlbumUri { get; set; }
    /// <summary>
    /// The URI of the artist
    /// </summary>
    public string ArtistUri { get; set; }

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

        if (t.TrackResource != null)
        {
            Title = t.TrackResource.Name;
            TrackURL = t.TrackResource.Location.Og;
            TrackUri = t.TrackResource.Uri;
        }
        if (t.ArtistResource != null)
        {
            Artist = t.ArtistResource.Name;
            ArtistUri = t.ArtistResource.Uri;
        }
        if (t.AlbumResource != null)
        {
            Album = t.AlbumResource.Name;
            AlbumUri = t.AlbumResource.Uri;
        }

        TotalTime = t.Length;
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
