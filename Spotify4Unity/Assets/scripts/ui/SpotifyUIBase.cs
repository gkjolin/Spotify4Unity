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
        m_eventManager = gameObject.AddComponent<EventManager>();
        m_eventManager.AddListener<PlayStatusChanged>(OnPlayStatusChanged);
        m_eventManager.AddListener<TrackChanged>(OnTrackChanged);
        m_eventManager.AddListener<TrackTimeChanged>(OnTrackTimeChanged);
        m_eventManager.AddListener<VolumeChanged>(OnVolumeChanged);

        m_spotifyService = new SpotifyService();
        m_spotifyService.OnPlayStatusChanged += OnPlayChanged;
        m_spotifyService.OnTrackChanged += OnTrackChanged;
        m_spotifyService.OnTrackTimeChanged += OnTrackTimeChanged;
        m_spotifyService.OnVolumeChanged += OnVolumeChanged;

        if (AutoConnect && !m_spotifyService.IsConnected)
            m_spotifyService.Connect();
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

    private void OnPlayChanged(bool isPlaying)
    {
        m_eventManager.QueueEvent(new PlayStatusChanged(isPlaying));
    }

    protected virtual void OnPlayStatusChanged(PlayStatusChanged e)
    {
        
    }

    private void OnTrackChanged(Track track)
    {
        m_eventManager.QueueEvent(new TrackChanged(track));
    }

    protected virtual void OnTrackChanged(TrackChanged e)
    {
        LoadAlbumArt(e.NewTrack);
    }

    private void OnTrackTimeChanged(float currentTime, float totalTime)
    {
        m_eventManager.QueueEvent(new TrackTimeChanged(currentTime, totalTime));
    }

    protected virtual void OnTrackTimeChanged(TrackTimeChanged e)
    {

    }

    private IEnumerator LoadAlbumArt(string url)
    {
        WWW imageArtWWW = new WWW(url);
        yield return imageArtWWW;
        Sprite s = Sprite.Create(imageArtWWW.texture, new Rect(0, 0, imageArtWWW.texture.width, imageArtWWW.texture.height), new Vector2(0, 0));
        OnAlbumArtLoaded(s);
    }

    protected virtual void OnAlbumArtLoaded(Sprite s)
    {

    }

    private void LoadAlbumArt(Track t, Track.Resolution resolution = Track.Resolution.Small)
    {
        string url = t.GetAlbumArtUrl(resolution);
        StartCoroutine(LoadAlbumArt(url));
    }

    private void OnVolumeChanged(VolumeInfo info)
    {
        m_eventManager.QueueEvent(new VolumeChanged(info.CurrentVolume, info.MaxVolume));
    }

    protected virtual void OnVolumeChanged(VolumeChanged e)
    {

    }
}
