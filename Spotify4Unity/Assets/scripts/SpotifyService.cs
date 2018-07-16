using SpotifyAPI;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Threading;

public class SpotifyService : MonoBehaviour
{
    /// <summary>
    ///Should the control automatically connect to Spotify when not connected?
    /// </summary>
    public bool AutoConnect = false;
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
    public List<Track> SavedTracks = new List<Track>();

    /// <summary>
    /// The current track being played
    /// </summary>
    public Track CurrentTrack = null;
    /// <summary>
    /// The current position in seconds the track has played
    /// </summary>
    public float CurrentTrackTime = 0f;
    /// <summary>
    /// The current volume levels
    /// </summary>
    public VolumeInfo Volume = null;

    public event Action<bool> OnPlayStatusChanged;
    public event Action<Track> OnTrackChanged;
    public event Action<float, float> OnTrackTimeChanged;
    public event Action<VolumeInfo> OnVolumeChanged;
    public event Action<bool> OnMuteChanged;
    public event Action<List<Track>> OnLoadedSavedTracks;

    private SpotifyLocalAPI m_spotify = null;
    private SpotifyWebAPI m_webAPI = null;

    private ProxyConfig m_proxyConfig = null;
    private SpotifyLocalAPIConfig m_localProxyConfig = null;
    
    /// <summary>
    /// The max number for volume to be set
    /// </summary>
    const float MAX_VOLUME_AMOUNT = 1f;
    const string CLIENT_ID = "26d287105e31491889f3cd293d85bfea";

    public SpotifyService()
    {
        m_proxyConfig = new ProxyConfig();

        m_localProxyConfig = new SpotifyLocalAPIConfig
        {
            ProxyConfig = new ProxyConfig()
            {
                Port = 80,
            }
        };
    }

    #region MonoBehavious
    private void Awake()
    {
        if (AutoConnect)
        {
            Connect();
        }
    }
    #endregion

    /// <summary>
    /// Attempts a connection to a local Spotify Client & WebAPI
    /// </summary>
    /// <returns>If the connection was successful or not to either client or WebAPI</returns>
    public bool Connect()
    {
        m_spotify = new SpotifyLocalAPI(m_localProxyConfig);
        m_spotify.OnPlayStateChange += OnPlayChangedInternal;
        m_spotify.OnTrackChange += OnTrackChangedInternal;
        m_spotify.OnTrackTimeChange += OnTrackTimeChangedInternal;
        m_spotify.OnVolumeChange += OnVolumeChangedInternal;

        bool localSpotifySuccessfulConnect = ConnectSpotifyLocal();
        bool webHelperSuccessfulConnect = ConnectSpotifyWebHelper();
        if (localSpotifySuccessfulConnect)
        {
            InitializeLocalSpotify();
        }
        if (webHelperSuccessfulConnect)
        {
            InitalizeWebHelper();
        }
        else
        {
            Debug.Log("Unable to connect");
        }

        IsConnected = localSpotifySuccessfulConnect || webHelperSuccessfulConnect;
        return IsConnected;
    }

    /// <summary>
    /// Plays a song in Spotify from it's URI
    /// </summary>
    /// <param name="songUri">The URI of the song</param>
    public void PlaySong(string songUri)
    {
        m_spotify.PlayURL(songUri);
    }

    private bool ConnectSpotifyLocal()
    {
        if (!SpotifyLocalAPI.IsSpotifyRunning())
        {
            Debug.Log("Spotify isn't running!");
            return false;
        }

        bool successful = false;
        try
        {
            successful = m_spotify.Connect();
        }
        catch (Exception e)
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
        return successful;
    }

    private bool ConnectSpotifyWebHelper()
    {
        if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
        {
            Debug.Log("SpotifyWebHelper isn't running!");
            return false;
        }

        WebAPIFactory webApiFactory = new WebAPIFactory(
            "http://localhost",
            8000,
            CLIENT_ID,
            Scope.UserReadPrivate | Scope.UserReadEmail | Scope.PlaylistReadPrivate | Scope.UserLibraryRead |
            Scope.UserReadPrivate | Scope.UserFollowRead | Scope.UserReadBirthdate | Scope.UserTopRead | Scope.PlaylistReadCollaborative |
            Scope.UserReadRecentlyPlayed | Scope.UserReadPlaybackState | Scope.UserModifyPlaybackState,
            m_proxyConfig);

        try
        {
            m_webAPI = webApiFactory.GetWebApi().Result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unable to connect to WebAPI - {ex}");
        }

        return m_webAPI != null;
    }

    private void InitializeLocalSpotify()
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

    private void InitalizeWebHelper()
    {
        Thread t = new Thread(LoadTracks);
        t.Start();
    }

    private void LoadTracks()
    {
        List<Track> tracks = GetSavedTracks();
        OnLoadedSavedTracks?.Invoke(tracks);
    }

    public void Disconnect()
    {
        if (m_spotify == null)
            return;

        m_spotify.OnPlayStateChange -= OnPlayChangedInternal;
        m_spotify.OnTrackChange -= OnTrackChangedInternal;
        m_spotify.OnTrackTimeChange -= OnTrackTimeChangedInternal;
        m_spotify.OnVolumeChange -= OnVolumeChangedInternal;

        m_spotify.Dispose();

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
        PlaySong(CurrentTrack.InternalCode + $"{hash}{minutes}:{seconds}");
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

    /// <summary>
    /// Gets all tracks saved to the users library in Spotify
    /// </summary>
    /// <returns></returns>
    public List<Track> GetSavedTracks()
    {
        if(m_webAPI == null)
        {
            Debug.LogError("Can't get saved tracks since WebAPI isn't connected");
            return null;
        }

        List<Track> tracks = new List<Track>();

        Paging<SavedTrack> savedTracks = m_webAPI.GetSavedTracks();
        List<FullTrack> list = savedTracks.Items.Select(x => x.Track).ToList();

        while (savedTracks.Next != null)
        {
            savedTracks = m_webAPI.GetSavedTracks(20, savedTracks.Offset + savedTracks.Limit);
            list.AddRange(savedTracks.Items.Select(t => t.Track));
        }

        foreach (FullTrack t in list)
        {
            string arists = String.Join(", ", t.Artists.Select(x => x.Name));
            tracks.Add(new Track()
            {
                Title = t.Name,
                Artist = arists,
                Album = t.Album.Name,
                ShareURL = t.PreviewUrl,
                InternalCode = t.Uri,
                TotalTime = t.DurationMs / 1000,
            });
        }

        return tracks;
    }
}
