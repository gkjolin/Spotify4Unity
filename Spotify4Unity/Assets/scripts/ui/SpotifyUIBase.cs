using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotifyUIBase : MonoBehaviour
{
    public bool AutoConnect = false;

    protected SpotifyService m_spotifyService = null;
    protected EventManager m_eventManager = null;

    protected virtual void Awake()
    {
        m_spotifyService = new SpotifyService();
        if (AutoConnect && !m_spotifyService.IsConnected)
            m_spotifyService.Connect();

        m_eventManager = gameObject.AddComponent<EventManager>();
        m_eventManager.AddListener<PlayStatusChanged>(OnPlayStatusChanged);

        m_spotifyService.OnPlayStatusChanged += OnPlayChanged;
    }

    protected virtual void Start ()
    {
    }

    protected virtual void Update ()
    {
    }

    protected virtual void OnDestroy()
    {
        m_spotifyService.Disconnect();
    }

    /// <summary>
    /// Gets the current volume information with current and max volume level 
    /// </summary>
    /// <returns>The current volume information</returns>
    protected VolumeInfo GetVolume()
    {
        if (m_spotifyService.IsConnected)
            return m_spotifyService.Volume;
        else
            return null;
    }

    /// <summary>
    /// Sets the volume of Spotify to a new amount. Number between 0-1
    /// </summary>
    /// <param name="newVolume">New volume amount between 0-1</param>
    protected void SetVolume(float newVolume)
    {
        m_spotifyService.SetVolume(newVolume);
    }

    /// <summary>
    /// Gets information on the currently playing track like title, arists, album name, etc
    /// </summary>
    /// <returns>All information on the current track</returns>
    protected Track GetCurrentSongInfo()
    {
        if (m_spotifyService.IsConnected)
            return m_spotifyService.CurrentTrack;
        else
            return null;
    }

    /// <summary>
    /// Gets if Spotify is currently playing a song or not
    /// </summary>
    /// <returns></returns>
    protected bool GetPlayingStatus()
    {
        if (m_spotifyService.IsConnected)
            return m_spotifyService.IsPlaying;
        else
            return false;
    }

    protected virtual void OnTrackTimeChanged(float currentTime, float totalTime)
    {

    }

    private void OnPlayChanged(bool isPlaying)
    {
        Debug.Log("IsPlaying changed to " + isPlaying);
        //OnPlayStatusChanged(isPlaying);

        m_eventManager.QueueEvent(new PlayStatusChanged(isPlaying));
    }

    protected virtual void OnPlayStatusChanged(PlayStatusChanged e)
    {
        
    }
}
