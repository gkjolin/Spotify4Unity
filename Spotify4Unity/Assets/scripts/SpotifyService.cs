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
    /// <summary>
    /// Is the user currently paying for Spotify Premium
    /// </summary>
    public bool IsPremium = false;
    /// <summary>
    /// All tracks saved to the users Spotify library
    /// </summary>
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

    #region LocalEvents
    public event Action<bool> OnPlayStatusChanged;
    public event Action<Track> OnTrackChanged;
    public event Action<float, float> OnTrackTimeChanged;
    public event Action<VolumeInfo> OnVolumeChanged;
    public event Action<bool> OnMuteChanged;
    public event Action<List<Track>> OnLoadedSavedTracks;
    public event Action<UserInfo> OnUserInfoLoaded;
    #endregion

    private SpotifyLocalAPI m_spotify = null;
    private SpotifyWebAPI m_webAPI = null;

    private ProxyConfig m_proxyConfig = null;
    private SpotifyLocalAPIConfig m_localProxyConfig = null;
    private UserInfo m_userInfo = null;

    private int m_lastVolumeLevel = 0;

    /// <summary>
    /// The max number for volume to be set
    /// </summary>
    const float MAX_VOLUME_AMOUNT = 100f;
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
            bool isConnected = Connect();
            if (isConnected)
                Analysis.Log("Successfully connected to Spotify");
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

        if(!localSpotifySuccessfulConnect && !webHelperSuccessfulConnect)
        {
            Analysis.Log("Unable to connect to any Spotify API - Local or Web");
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
            Analysis.Log("Local Spotify isn't running!");
            return false;
        }

        bool successful = false;
        try
        {
            successful = m_spotify.Connect();
        }
        catch (Exception e)
        {
            Analysis.Log($"Unable to connect to Local Spotify - {e}");
        }
        return successful;
    }

    private bool ConnectSpotifyWebHelper()
    {
        if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
        {
            Analysis.Log("SpotifyWebHelper isn't running!");
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
            Analysis.LogError($"Unable to connect to WebAPI - {ex}");
        }

        return m_webAPI != null;
    }

    private void InitializeLocalSpotify()
    {
        m_spotify.ListenForEvents = true;

        StatusResponse status = m_spotify.GetStatus();
        SetTrack(status.Track);
        SetPlaying(status.Playing);
        SetVolumeInternal(new VolumeInfo()
        {
            //Times 100 since GetStatus Volume value is between 0-1
            CurrentVolume = (float)status.Volume * 100,
            MaxVolume = MAX_VOLUME_AMOUNT,
        });
        SetMuted(status.Volume == 0.0);
    }

    private void InitalizeWebHelper()
    {
        Thread t = new Thread(LoadTracks);
        t.Start();

        //Times 100 since GetStatus Volume value is between 0-1
        m_lastVolumeLevel = (int)m_spotify.GetStatus().Volume * 100;

        LoadUserInformation();
    }

    private void LoadUserInformation()
    {
        PrivateProfile privateProfile = m_webAPI.GetPrivateProfile();
        IsPremium = privateProfile.Product == "premium";

        string profilePicture = privateProfile.Images.Count > 0 ? privateProfile.Images.FirstOrDefault().Url : null;
        m_userInfo = new UserInfo()
        {
            Username = privateProfile.DisplayName,
            DisplayName = privateProfile.DisplayName,

            Followers = privateProfile.Followers.Total,
            IsPremium = IsPremium,

            ProfilePictureURL = profilePicture,
            Country = privateProfile.Country,
            UserID = privateProfile.Id,
            Birthdate = ParseBirthdate(privateProfile.Birthdate),
        };
        OnUserInfoLoaded?.Invoke(m_userInfo);
    }

    private DateTime ParseBirthdate(string birthdate)
    {
        //Format should come through as "Year-Month-Day". Simple parse
        string[] split = birthdate.Split('-');
        if(split.Length >= 3)
        {
            int year = int.Parse(split[0]);
            int month = int.Parse(split[1]);
            int day = int.Parse(split[2]);
            return new DateTime(year, month, day);
        }
        else
        {
            return DateTime.MinValue;
        }
    }

    private void LoadTracks()
    {
        SavedTracks = GetSavedTracks();
        OnLoadedSavedTracks?.Invoke(SavedTracks);
        Analysis.Log("All saved tracks loaded");
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

            Analysis.Log($"Resuming song '{CurrentTrack.Artist} - {CurrentTrack.Title}'");
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

            Analysis.Log($"Pausing song '{CurrentTrack.Artist} - {CurrentTrack.Title}'");
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

        if (isMuted)
            m_webAPI.SetVolume(0);
        else
            m_webAPI.SetVolume(m_lastVolumeLevel);

        IsMuted = isMuted;
        OnMuteChanged?.Invoke(isMuted);
    }

    /// <summary>
    /// Move the current track position using the total seconds
    /// </summary>
    /// <param name="totalSeconds">The total seconds to move the track position to</param>
    public void SetTrackPosition(float totalSeconds)
    {
        if (totalSeconds > CurrentTrack.TotalTime)
        {
            Analysis.LogError("Can't set current track position since given number is higher than track time");
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

        Analysis.Log($"Set '{CurrentTrack.Artist} - {CurrentTrack.Title}' position to {minutes}:{seconds}");
    }

    public void NextSong()
    {
        m_spotify.Skip();
        Analysis.Log($"Playing next song '{CurrentTrack.Artist} - {CurrentTrack.Title}'");
    }

    public void PreviousSong()
    {
        m_spotify.Previous();

        Analysis.Log($"Playing previous song '{CurrentTrack.Artist} - {CurrentTrack.Title}'");
    }

    /// <summary>
    /// Sets the volume of Spotify
    /// </summary>
    /// <param name="newVolume">New volume amount. Should be between 0 - 100</param>
    public void SetVolume(float newVolume)
    {
        int newVolPercent = (int)newVolume;
        SetVolume(newVolPercent);
    }

    /// <summary>
    /// Sets the volume of Spotify
    /// </summary>
    /// <param name="newVolume">The new volume to set to. Should ne a number between 0 - 100</param>
    public void SetVolume(int newVolume)
    {
        if (newVolume > 100)
            newVolume = 100;

        //Only set restore value when not muted
        if(!IsMuted)
            m_lastVolumeLevel = newVolume;

        Volume = new VolumeInfo()
        {
            CurrentVolume = m_lastVolumeLevel,
            MaxVolume = MAX_VOLUME_AMOUNT,
        };
        m_webAPI.SetVolume(newVolume);
        Analysis.Log($"Set Spotify volume to {newVolume}");
    }

    private void SetVolumeInternal(VolumeInfo info)
    {
        Volume = info;
        OnVolumeChanged?.Invoke(Volume);
    }

    private void SetTrack(SpotifyAPI.Local.Models.Track t)
    {
        CurrentTrack = new Track(t);
        OnTrackChanged?.Invoke(CurrentTrack);

        Analysis.Log($"Set current track to '{CurrentTrack.Artist} - {CurrentTrack.Title}'");
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
        SetVolumeInternal(new VolumeInfo()
        {
            //Times 100 since Volume value is between 0-1
            CurrentVolume = (float)e.NewVolume * 100f,
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
    /// Gets the latest song information
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Gets all tracks saved to the users library in Spotify in the order they were added to their library
    /// </summary>
    /// <returns></returns>
    public List<Track> GetSavedTracks()
    {
        if(m_webAPI == null)
        {
            Analysis.LogError("Can't get saved tracks since WebAPI isn't connected");
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

    /// <summary>
    /// Gets all saved tracks and sorts the list by an option
    /// </summary>
    /// <param name="sortType">The sort order the list should be in</param>
    /// <returns>The list of saved tracks sorted by the order</returns>
    public List<Track> GetSavedTracksSorted(Sort sortType)
    {
        List<Track> allSavedTracks = GetSavedTracks();
        switch (sortType)
        {
            case Sort.Title:
                return allSavedTracks.OrderBy(x => x.Title).ToList();
            case Sort.Artist:
                return allSavedTracks.OrderBy(x => x.Artist).ToList();
            case Sort.Album:
                return allSavedTracks.OrderBy(x => x.Album).ToList();
            case Sort.Unsorted:
                return allSavedTracks;
            default:
                throw new NotImplementedException("Unimplemented sort type to Saved Tracks");
        }
    }

    /// <summary>
    /// Gets the currently loaded user information
    /// </summary>
    /// <returns></returns>
    public UserInfo GetProfileInfo()
    {
        return m_userInfo;
    }
}
