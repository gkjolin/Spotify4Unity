using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;
using System;
using SpotifyAPI;
using System.Reflection;

public class SpotifyService
{
    public bool IsPlaying = false;
    public bool IsConnected = false;

    public Track CurrentTrack = null;
    public float CurrentTrackTime = 0f;

    public VolumeInfo Volume = null;

    private SpotifyLocalAPI m_spotify;

    public SpotifyService()
    {
    }

    public bool Connect()
    {
        var config = new SpotifyLocalAPIConfig
        {
            ProxyConfig = new ProxyConfig()
            {
                Port = 80,
            }
        };

        m_spotify = new SpotifyLocalAPI(config);
        m_spotify.OnPlayStateChange += OnPlayChangedInternal;
        m_spotify.OnTrackChange += OnTrackChangedInternal;
        m_spotify.OnTrackTimeChange += OnTrackTimeChangedInternal;
        m_spotify.OnVolumeChange += OnVolumeChangedInternal;

        if (!SpotifyLocalAPI.IsSpotifyRunning())
        {
            Debug.Log(@"Spotify isn't running!");
            return false;
        }
        if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
        {
            Debug.Log(@"SpotifyWebHelper isn't running!");
            return false;
        }

        bool successful = false;
        try
        {
            successful = m_spotify.Connect();
        }
        catch(Exception e)
        {
            Debug.Log(e.ToString());

            Type type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (displayName != null)
                    Debug.Log(displayName.Invoke(null, null));
            }
        }

        if (successful)
        {
            Initialize();
        }
        else
        {
            Debug.Log("Unable to connect");
        }

        IsConnected = successful;
        return successful;
    }

    public void Disconnect()
    {
        if (m_spotify == null)
            return;

        m_spotify.Dispose();

        m_spotify.OnPlayStateChange -= OnPlayChangedInternal;
        m_spotify.OnTrackChange -= OnTrackChangedInternal;
        m_spotify.OnTrackTimeChange -= OnTrackTimeChangedInternal;
        m_spotify.OnVolumeChange -= OnVolumeChangedInternal;

        m_spotify = null;
        IsConnected = false;
        IsPlaying = false;
        CurrentTrack = null;
        CurrentTrackTime = 0f;
        Volume = null;
    }

    public void Play()
    {
        m_spotify.Play();
        IsPlaying = false;
    }

    public void Pause()
    {
        m_spotify.Pause();
        IsPlaying = true;
    }

    public SongInfo GetSongInfo()
    {
        StatusResponse r = m_spotify.GetStatus();
        if (r == null)
            return null;

        SongInfo info = new SongInfo()
        {
            Title = r.Track.TrackResource.Name,
            Artist = r.Track.ArtistResource.Name,
            AlbumName = r.Track.AlbumResource.Name,

            IsPlaying = r.Playing,
            CurrentTime = r.PlayingPosition,
            TotalDuration = r.Track.Length,
        };
        return info;
    }

    public void NextSong()
    {
        m_spotify.Previous();
    }

    public void PreviousSong()
    {
        m_spotify.Skip();
    }

    private void Initialize()
    {
        m_spotify.ListenForEvents = true;

        StatusResponse status = m_spotify.GetStatus();
        CurrentTrack = new Track(status.Track);
        IsPlaying = status.Playing;
        Volume = new VolumeInfo()
        {
            CurrentVolume = (float)status.Volume,
            MaxVolume = 1f,
        };
    }

    private void OnVolumeChangedInternal(object sender, VolumeChangeEventArgs e)
    {
        Volume = new VolumeInfo()
        {
            CurrentVolume = (float)e.NewVolume,
            OldVolume = (float)e.OldVolume,
            MaxVolume = 1f,
        };
    }

    private void OnTrackTimeChangedInternal(object sender, TrackTimeChangeEventArgs e)
    {
        CurrentTrackTime = (float)e.TrackTime;
    }

    private void OnTrackChangedInternal(object sender, TrackChangeEventArgs e)
    {
        CurrentTrack = new Track(e.NewTrack);
    }

    private void OnPlayChangedInternal(object sender, PlayStateEventArgs e)
    {
        IsPlaying = e.Playing;
    }
}
