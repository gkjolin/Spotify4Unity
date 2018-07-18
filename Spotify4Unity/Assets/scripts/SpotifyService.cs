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

/// <summary>
/// Constant class service for controlling and retrieving information to/from Spotify
/// </summary>
public sealed class SpotifyService : MonoBehaviour
{
    /// <summary>
    /// Your Client ID for your app.
    /// You must register your application to use the Spotify API online at https://developer.spotify.com/documentation/general/guides/app-settings/#register-your-app
    /// </summary>
    [Tooltip("Your Client ID for your app. You must register your application to use the Spotify API online at https://developer.spotify.com/documentation/general/guides/app-settings/#register-your-app")]
    public string WebAPIClientId = "";
    /// <summary>
    /// The port to use when authenticating. Should be the same as your "Redirect URI" in your application's Spotify Dashboard
    /// </summary>
    [Tooltip("The port to use when authenticating. Should be the same as your 'Redirect URI(s)' in your application's Spotify Dashboard")]
    public int ConnectionPort = 8000;

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
    /// The current state of shuffle
    /// </summary>
    public Shuffle ShuffleState = Shuffle.Disabled;
    /// <summary>
    /// Current state of repeat
    /// </summary>
    public Repeat RepeatState = Repeat.Disabled;

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
    public event Action<Repeat> OnRepeatChanged;
    public event Action<Shuffle> OnShuffleChanged;
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
    private const float MAX_VOLUME_AMOUNT = 100f;
    /// <summary>
    /// The id for premium on the users profile
    /// </summary>
    private const string PREMIUM_ID = "premium";
    /// <summary>
    /// The id for a user (non-premium) on the users profile
    /// </summary>
    private const string USER_ID = "user";

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
        if (string.IsNullOrEmpty(WebAPIClientId))
        {
            Analysis.LogError("Won't be able to connect to WebAPI since no 'Client ID' has been set on SpotifyService");
        }

        if (AutoConnect && !string.IsNullOrEmpty(WebAPIClientId) && ConnectionPort > 0)
        {
            bool isConnected = Connect(WebAPIClientId, ConnectionPort);
            if (isConnected)
                Analysis.Log("Successfully connected to Spotify");
        }
    }
    #endregion

    /// <summary>
    /// Attempts a connection to a local Spotify Client & WebAPI
    /// </summary>
    /// <param name="webApiClientId">Your client id for your app</param>
    /// <returns>If the connection was successful or not to either client or WebAPI</returns>
    public bool Connect(string webApiClientId, int port = 8000)
    {
        m_spotify = new SpotifyLocalAPI(m_localProxyConfig);
        m_spotify.OnPlayStateChange += OnPlayChangedInternal;
        m_spotify.OnTrackChange += OnTrackChangedInternal;
        m_spotify.OnTrackTimeChange += OnTrackTimeChangedInternal;
        m_spotify.OnVolumeChange += OnVolumeChangedInternal;

        bool localSpotifySuccessfulConnect = ConnectSpotifyLocal();
        bool webHelperSuccessfulConnect = ConnectSpotifyWebHelper(webApiClientId, port);
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
    /// <param name="contextUri">The context to play the song within. For example, the Artist Uri or Album Uri. Leave blank to just play the one song</param>
    public void PlaySong(string songUri, string contextUri = "")
    {
        m_spotify.PlayURL(songUri, contextUri);

        //Force event since LocalAPI doesn't always call "OnTrackChange" event for setting our own song
        StatusResponse status = m_spotify.GetStatus();
        SetTrack(status?.Track);
    }

    private bool ConnectSpotifyLocal()
    {
        if (!SpotifyLocalAPI.IsSpotifyRunning())
        {
            Analysis.LogError("Local Spotify isn't running!");
            return false;
        }

        bool successful = false;
        try
        {
            successful = m_spotify.Connect();
        }
        catch (Exception e)
        {
            Analysis.LogError($"Unable to connect to Local Spotify - {e}");
        }
        return successful;
    }

    /// <summary>
    /// Connectes to the WebHelper with your ClientId
    /// </summary>
    /// <param name="clientId">Custom client id</param>
    /// <returns></returns>
    private bool ConnectSpotifyWebHelper(string clientId, int port = 8000)
    {
        if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
        {
            Analysis.LogError("SpotifyWebHelper isn't running!");
            return false;
        }

        WebAPIFactory webApiFactory = new WebAPIFactory(
            "http://localhost",
            port,
            clientId,
            Scope.UserReadPrivate | Scope.UserReadEmail | Scope.PlaylistReadPrivate | Scope.UserLibraryRead |
            Scope.UserReadPrivate | Scope.UserFollowRead | Scope.UserReadBirthdate | Scope.UserTopRead | Scope.PlaylistReadCollaborative |
            Scope.UserReadRecentlyPlayed | Scope.UserReadPlaybackState | Scope.UserModifyPlaybackState | Scope.Streaming,
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

        StatusResponse currentStatus = m_spotify.GetStatus();
        SetTrack(currentStatus.Track);
        SetPlaying(currentStatus.Playing);

        float currentVolume = (float)currentStatus.Volume * 100f;
        SetVolumeInternal(new VolumeInfo()
        {
            //Times 100 since GetStatus Volume value is between 0-1
            CurrentVolume = currentVolume,
            MaxVolume = MAX_VOLUME_AMOUNT,
        });

        //Times 100 since GetStatus Volume value is between 0-1
        m_lastVolumeLevel = (int)currentVolume;
        //If Spotify is muted on start, then when Unmute is called, set to 50 volume
        if (m_lastVolumeLevel == 0)
            m_lastVolumeLevel = 50;

        SetMute(m_lastVolumeLevel == 0);

        SetShuffleInternal(currentStatus.Shuffle ? Shuffle.Enabled : Shuffle.Disabled);
        //ToDo: Check if repeat state is on song, playlist or disabled. Currently only able to know from boolean
        SetRepeatInternal(currentStatus.Repeat ? Repeat.Playlist : Repeat.Disabled);
    }

    private void InitalizeWebHelper()
    {
        Thread t = new Thread(LoadTracks);
        t.Start();

        LoadUserInformation();
    }

    /// <summary>
    /// Loads the latest user information about the user
    /// </summary>
    private void LoadUserInformation()
    {
        PrivateProfile privateProfile = m_webAPI.GetPrivateProfile();
        IsPremium = privateProfile.Product == PREMIUM_ID;

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
        Analysis.Log($"{SavedTracks.Count} saved tracks loaded");
    }

    /// <summary>
    /// Disconnects and removes any information from the service
    /// </summary>
    public void Disconnect()
    {
        if (m_spotify != null)
        {
            m_spotify.OnPlayStateChange -= OnPlayChangedInternal;
            m_spotify.OnTrackChange -= OnTrackChangedInternal;
            m_spotify.OnTrackTimeChange -= OnTrackTimeChangedInternal;
            m_spotify.OnVolumeChange -= OnVolumeChangedInternal;

            m_spotify.Dispose();
            m_spotify = null;
        }

        if (m_webAPI != null)
        {
            m_webAPI.Dispose();
            m_webAPI = null;
        }

        IsConnected = false;
        IsPlaying = false;
        IsMuted = false;

        CurrentTrack = null;
        CurrentTrackTime = 0f;
        Volume = null;
        SavedTracks = null;
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
        {
            m_webAPI.SetVolume(0);
            Analysis.Log($"Muted volume");
        }
        else
        {
            m_webAPI.SetVolume(m_lastVolumeLevel);
            Analysis.Log($"Unmuted volume & set to '{m_lastVolumeLevel}'");
        }

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
        PlaySong(CurrentTrack.TrackUri + $"{hash}{minutes}:{seconds}");

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

    private void SetRepeatInternal(Repeat state)
    {
        RepeatState = state;
        OnRepeatChanged?.Invoke(RepeatState);
        Analysis.Log($"Set Repeat mode to {state.ToString()}");
    }

    private void SetShuffleInternal(Shuffle state)
    {
        ShuffleState = state;
        OnShuffleChanged?.Invoke(ShuffleState);
        Analysis.Log($"Set Shuffle mode to {state.ToString()}");
    }

    private void SetTrack(SpotifyAPI.Local.Models.Track t)
    {
        if (t == null)
            return;

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
        if (CurrentTrack.TrackUri == e.NewTrack.TrackResource.Uri)
            return;

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
                TrackUri = t.Uri,
                TotalTime = t.DurationMs / 1000,
            });
        }

        SavedTracks = tracks;
        return tracks;
    }

    /// <summary>
    /// Gets all saved tracks and sorts the list by an option
    /// </summary>
    /// <param name="sortType">The sort order the list should be in</param>
    /// <returns>The list of saved tracks sorted by the order</returns>
    public List<Track> GetSavedTracksSorted(Sort sortType)
    {
        List<Track> allSavedTracks = null;
        if (SavedTracks != null)
            allSavedTracks = SavedTracks;
        else
            allSavedTracks = GetSavedTracks();

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

    /// <summary>
    /// Set the repeat state in Spotify.
    /// </summary>
    /// <param name="state">The repeat state to set to</param>
    public void SetRepeat(Repeat state)
    {
        RepeatState repeatState = SpotifyAPI.Web.Enums.RepeatState.Off;
        switch (state)
        {
            case Repeat.Disabled:
                repeatState = SpotifyAPI.Web.Enums.RepeatState.Off;
                break;
            case Repeat.Playlist:
                repeatState = SpotifyAPI.Web.Enums.RepeatState.Context;
                break;
            case Repeat.Track:
                repeatState = SpotifyAPI.Web.Enums.RepeatState.Track;
                break;
        }

        RepeatState = state;
        m_webAPI.SetRepeatMode(repeatState);
        OnRepeatChanged?.Invoke(RepeatState);

        Analysis.Log($"Set Repeat state to {state.ToString()}");
    }

    /// <summary>
    /// Sets the shuffle state of Spotify
    /// </summary>
    /// <param name="state">The shuffle state to set to</param>
    public void SetShuffle(Shuffle state)
    {
        ShuffleState = state;
        m_webAPI.SetShuffle(state == Shuffle.Enabled);
        OnShuffleChanged?.Invoke(ShuffleState);

        Analysis.Log($"Set Shuffle state to {state.ToString()}");
    }
}
