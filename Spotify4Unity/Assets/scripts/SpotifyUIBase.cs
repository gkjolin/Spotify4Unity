using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotifyUIBase : MonoBehaviour
{
    /// <summary>
    /// Sets the different type of album art resolution to be used
    /// </summary>
    protected Track.Resolution m_albumArtResolution = Track.Resolution.Small;

    protected List<Track> m_savedTracks = null;

    [SerializeField]
    protected SpotifyService m_spotifyService = null;
    protected EventManager m_eventManager = null;

    protected virtual void Awake()
    {
        m_eventManager = gameObject.AddComponent<EventManager>();
        m_eventManager.AddListener<PlayStatusChanged>(OnPlayStatusChanged);
        m_eventManager.AddListener<TrackChanged>(OnTrackChanged);
        m_eventManager.AddListener<TrackTimeChanged>(OnTrackTimeChanged);
        m_eventManager.AddListener<VolumeChanged>(OnVolumeChanged);
        m_eventManager.AddListener<MuteChanged>(OnMuteChanged);
        m_eventManager.AddListener<SavedTracksLoaded>(OnSavedTracksLoaded);

        if (m_spotifyService == null)
        {
            SpotifyService service = GameObject.FindObjectOfType<SpotifyService>();
            if(service == null)
            {
                Debug.LogError($"No SpotifyService set on GameObject {this.gameObject.name}");
                service = this.gameObject.AddComponent<SpotifyService>();
            }
            m_spotifyService = service;
        }

        if (m_spotifyService != null)
        {
            m_spotifyService.OnPlayStatusChanged += OnPlayChanged;
            m_spotifyService.OnTrackChanged += OnTrackChanged;
            m_spotifyService.OnTrackTimeChanged += OnTrackTimeChanged;
            m_spotifyService.OnVolumeChanged += OnVolumeChanged;
            m_spotifyService.OnMuteChanged += OnMuteChanged;
            m_spotifyService.OnLoadedSavedTracks += OnSavedTracksLoaded;
        }
    }

    protected virtual void Start ()
    {
    }

    protected virtual void Update ()
    {
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            //ToDo: Update UI
        }
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
    /// Sets the current track to a position in seconds
    /// </summary>
    /// <param name="positionSeconds"></param>
    protected void SetCurrentTrackTime(float positionSeconds)
    {
        if (positionSeconds > m_spotifyService.CurrentTrackTime)
            return;

        if(positionSeconds != m_spotifyService.CurrentTrackTime)
        {
            m_spotifyService.SetTrackPosition(positionSeconds);
        }
    }

    /// <summary>
    /// Sets the current track position using minutes and seconds
    /// </summary>
    /// <param name="minutes"></param>
    /// <param name="seconds"></param>
    protected void SetCurrentTrackTime(int minutes, int seconds)
    {
        float totalSeconds = (minutes * 60) + seconds;
        if (totalSeconds > m_spotifyService.CurrentTrackTime)
            return;

        m_spotifyService.SetTrackPosition(minutes, seconds);
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

    private void OnTrackChanged(Track track)
    {
        m_eventManager.QueueEvent(new TrackChanged(track));
    }

    private void OnTrackTimeChanged(float currentTime, float totalTime)
    {
        m_eventManager.QueueEvent(new TrackTimeChanged(currentTime, totalTime));
    }

    /// <summary>
    /// Loads album art from a string URL using WWW. Calls OnAlbumArtLoaded(Sprite s) when complete
    /// </summary>
    /// <param name="url">The url of the image</param>
    /// <returns></returns>
    private IEnumerator LoadAlbumArt(string url)
    {
        WWW imageArtWWW = new WWW(url);
        yield return imageArtWWW;
        Sprite s = Sprite.Create(imageArtWWW.texture, new Rect(0, 0, imageArtWWW.texture.width, imageArtWWW.texture.height), new Vector2(0, 0));
        OnAlbumArtLoaded(s);
    }

    private void LoadAlbumArt(Track t, Track.Resolution resolution = Track.Resolution.Small)
    {
        string url = t.GetAlbumArtUrl(resolution);
        StartCoroutine(LoadAlbumArt(url));
    }

    private void OnVolumeChanged(VolumeInfo info)
    {
        if(info != null)
            m_eventManager.QueueEvent(new VolumeChanged(info.CurrentVolume, info.MaxVolume));
    }

    private void OnMuteChanged(bool isMuted)
    {
        m_eventManager.QueueEvent(new MuteChanged(isMuted));
    }

    protected virtual void OnTrackChanged(TrackChanged e)
    {
        LoadAlbumArt(e.NewTrack, m_albumArtResolution);
    }

    private void OnSavedTracksLoaded(List<Track> t)
    {
        m_eventManager.QueueEvent(new SavedTracksLoaded(t));
    }

    protected virtual void OnPlayStatusChanged(PlayStatusChanged e)
    {

    }

    protected virtual void OnTrackTimeChanged(TrackTimeChanged e)
    {

    }

    protected virtual void OnVolumeChanged(VolumeChanged e)
    {

    }
    
    protected virtual void OnMuteChanged(MuteChanged e)
    {
        
    }

    protected virtual void OnAlbumArtLoaded(Sprite s)
    {

    }

    protected virtual void OnSavedTracksLoaded(SavedTracksLoaded e)
    {

    }
}
