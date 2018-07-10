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
        m_spotify.OnPlayStateChange += OnPlayChanged;
        m_spotify.OnTrackChange += OnTrackChanged;
        m_spotify.OnTrackTimeChange += OnTrackTimeChanged;
        m_spotify.OnVolumeChange += OnVolumeChanged;

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
            m_spotify.ListenForEvents = true;
        }
        else
        {
            Debug.Log("Unable to connect");
        }

        IsConnected = successful;
        return successful;
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

    public SongInfo GetCurrentInfo()
    {
        var r = m_spotify.GetStatus();
        if (r == null)
            return null;

        SongInfo info = new SongInfo()
        {
            Title = r.Track.TrackResource.Name,
            Artist = r.Track.ArtistResource.Name,
            AlbumName = r.Track.AlbumResource.Name,

            CurrentTime = 3,
            TotalDuration = r.Track.Length,
        };
        return info;
    }

    private void OnVolumeChanged(object sender, VolumeChangeEventArgs e)
    {
        
    }

    private void OnTrackTimeChanged(object sender, TrackTimeChangeEventArgs e)
    {
        
    }

    private void OnTrackChanged(object sender, TrackChangeEventArgs e)
    {
        
    }

    private void OnPlayChanged(object sender, PlayStateEventArgs e)
    {
        
    }
}
