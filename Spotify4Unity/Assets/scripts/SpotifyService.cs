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
    /// <summary>
    /// Is Spotify currently playing music?
    /// </summary>
    public bool IsPlaying = false;
    /// <summary>
    /// Are we connected to Spotify and able to control it
    /// </summary>
    public bool IsConnected = false;
    /// <summary>
    /// Is the sounds from Spotify muted
    /// </summary>
    public bool IsMuted = false;

    /// <summary>
    /// The current track being played
    /// </summary>
    public Track CurrentTrack = null;
    /// <summary>
    /// The current position in seconds the track has played
    /// </summary>
    public float CurrentTrackTime = 0f;
    public VolumeInfo Volume = null;

    public event Action<bool> OnPlayStatusChanged;
    public event Action<Track> OnTrackChanged;
    public event Action<float, float> OnTrackTimeChanged;
    public event Action<VolumeInfo> OnVolumeChanged;
    public event Action<bool> OnMuteChanged;

    private SpotifyLocalAPI m_spotify;

    /// <summary>
    /// The max number for volume to be set
    /// </summary>
    const float MAX_VOLUME_AMOUNT = 1f;

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

    private void Initialize()
    {
        m_spotify.ListenForEvents = true;

        StatusResponse status = m_spotify.GetStatus();
        SetTrack(status.Track);
        SetPlaying(status.Playing);
        SetVolume(new VolumeInfo()
        {
            CurrentVolume = (float)status.Volume,
            MaxVolume = MAX_VOLUME_AMOUNT,
        });
        SetMuted(status.Volume == 0.0);
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
        IsMuted = false;

        CurrentTrack = null;
        CurrentTrackTime = 0f;
        Volume = null;
    }

    /// <summary>
    /// Plays the song currently in Spotify
    /// </summary>
    public void Play()
    {
        if (!IsPlaying)
        {
            m_spotify.Play();
            IsPlaying = true;
        }
    }

    /// <summary>
    /// Pauses the current song
    /// </summary>
    public void Pause()
    {
        if(IsPlaying)
        {
            m_spotify.Pause();
            IsPlaying = false;
        }
    }

    /// <summary>
    /// Sets if Spotify should be muted or not
    /// </summary>
    /// <param name="isMuted">true is muted. False is Unmuted</param>
    public void SetMute(bool isMuted)
    {
        if (IsMuted == isMuted)
            return;

        /* NOTE: Broken. InvalidCastException internal to SpotifyAPI
        if (isMuted)
            m_spotify.Mute();
        else
            m_spotify.UnMute();
        
        IsMuted = isMuted;
        OnMuteChanged?.Invoke(isMuted);
        */
    }

    /// <summary>
    /// Move the current track position using the total seconds
    /// </summary>
    /// <param name="totalSeconds">The total seconds to move the track position to</param>
    public void SetTrackPosition(float totalSeconds)
    {
        if (totalSeconds > CurrentTrack.TotalTime)
        {
            Debug.LogError("Can't set current track position since given number is higher than track time!");
            return;
        }

        int minutes = (int)totalSeconds / 60;
        int seconds = (int)totalSeconds % 60;
        SetTrackPosition(minutes, seconds);
    }

    /// <summary>
    /// Move the current track position using minutes and seconds
    /// </summary>
    /// <param name="minutes">The amount of minutes into the track to move to</param>
    /// <param name="seconds">The amount of seconds into the track to move to</param>
    public void SetTrackPosition(int minutes, int seconds)
    {
        //Requires an encoded # inbetween URI and minutes & seconds
        string hash = "%23";
        m_spotify.PlayURL(CurrentTrack.InternalCode + $"{hash}{minutes}:{seconds}");
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
        m_spotify.Skip();
    }

    public void PreviousSong()
    {
        m_spotify.Previous();
    }

    public void SetVolume(float newVolume)
    {
        /* NOTE: Broken. InvalidCastException internal to SpotifyAPI*/
        //m_spotify.SetSpotifyVolume(newVolume * 100);
    }

    private void SetVolume(VolumeInfo info)
    {
        Volume = info;
        OnVolumeChanged?.Invoke(Volume);
    }

    private void SetTrack(SpotifyAPI.Local.Models.Track t)
    {
        CurrentTrack = new Track(t);
        OnTrackChanged?.Invoke(CurrentTrack);
    }

    private void SetPlaying(bool isPlaying)
    {
        IsPlaying = isPlaying;
        OnPlayStatusChanged?.Invoke(IsPlaying);
    }

    private void SetMuted(bool isMuted)
    {
        IsMuted = isMuted;
        OnMuteChanged?.Invoke(IsMuted);
    }

    private void OnVolumeChangedInternal(object sender, VolumeChangeEventArgs e)
    {
        SetVolume(new VolumeInfo()
        {
            CurrentVolume = (float)e.NewVolume,
            //OldVolume = (float)e.OldVolume,
            MaxVolume = MAX_VOLUME_AMOUNT,
        });
    }

    private void OnTrackTimeChangedInternal(object sender, TrackTimeChangeEventArgs e)
    {
        CurrentTrackTime = (float)e.TrackTime;
        OnTrackTimeChanged?.Invoke(CurrentTrackTime, CurrentTrack.TotalTime);
    }

    private void OnTrackChangedInternal(object sender, TrackChangeEventArgs e)
    {
        SetTrack(e.NewTrack);
    }

    private void OnPlayChangedInternal(object sender, PlayStateEventArgs e)
    {
        SetPlaying(e.Playing);
    }
}
