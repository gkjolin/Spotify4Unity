using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExamplePlayerController : SpotifyUIBase
{
    [SerializeField, Tooltip("Text to display the current track's artists")]
    Text m_artistText;

    [SerializeField, Tooltip("Text to display the current track name")]
    Text m_trackText;

    [SerializeField, Tooltip("Text to display the current track's album name")]
    Text m_albumText;

    [SerializeField, Tooltip("Slider to control and display current track's position")]
    Slider m_playingSlider;

    [SerializeField, Tooltip("Text to display the current track position")]
    Text m_trackPositionText;

    [SerializeField, Tooltip("Slider to display and control the current volume")]
    Slider m_volumeSlider;

    [SerializeField, Tooltip("Button used to mute Spotify sound")]
    Button m_muteBtn;

    [SerializeField, Tooltip("Button to unmute Spotify's sound")]
    Button m_unmuteBtn;

    [SerializeField, Tooltip("Button to change the track to the previous track")]
    Button m_previousBtn;

    [SerializeField, Tooltip("Button to change the track to the next track")]
    Button m_nextBtn;

    [SerializeField, Tooltip("Button to Play the track when paused")]
    Button m_playBtn;

    [SerializeField, Tooltip("Button to pause the track when playing")]
    Button m_pauseBtn;

    [SerializeField, Tooltip("Image to display the current track's album art")]
    Image m_albumArt;

    [SerializeField]
    Button m_shuffleBtn;

    [SerializeField]
    Button m_repeatBtn;

    [SerializeField]
    Sprite[] m_repeatSprites;

    [SerializeField]
    Sprite[] m_shuffleSprites;

    private bool m_isDraggingTrackPositionSlider = false;
    private float m_lastTrackPosSliderValue = -1f;

    #region MonoBehavious
    protected override void Awake()
    {
        base.Awake();

        if (m_nextBtn != null)
            m_nextBtn.onClick.AddListener(OnPlayMedia);

        if (m_pauseBtn != null)
            m_pauseBtn.onClick.AddListener(OnPauseMedia);

        if (m_playBtn != null)
            m_playBtn.onClick.AddListener(OnPlayMedia);

        if (m_previousBtn != null)
            m_previousBtn.onClick.AddListener(OnPreviousMedia);

        if (m_nextBtn != null)
            m_nextBtn.onClick.AddListener(OnNextMedia);

        if (m_muteBtn != null)
            m_muteBtn.onClick.AddListener(OnMuteSound);
        if (m_unmuteBtn != null)
            m_unmuteBtn.onClick.AddListener(OnUnmuteSound);

        if (m_volumeSlider != null)
            m_volumeSlider.onValueChanged.AddListener(OnSetVolumeChanged);

        if (m_playingSlider != null)
            m_playingSlider.onValueChanged.AddListener(OnSetTrackPosition);

        if (m_repeatBtn != null)
            m_repeatBtn.onClick.AddListener(OnClickRepeat);

        if (m_shuffleBtn != null)
            m_shuffleBtn.onClick.AddListener(OnClickShuffle);

        //m_albumArtResolution = Track.Resolution.Large;
    }

    protected override void Update ()
    {
    }
    #endregion

    public void Connect()
    {
        m_spotifyService.Connect();
    }

    private void OnNextMedia()
    {
        m_spotifyService.NextSong();
    }

    private void OnPreviousMedia()
    {
        m_spotifyService.PreviousSong();
    }

    private void OnPauseMedia()
    {
        m_spotifyService.Pause();
    }

    private void OnPlayMedia()
    {
        m_spotifyService.Play();
    }

    protected override void OnTrackTimeChanged(TrackTimeChanged e)
    {
        base.OnTrackTimeChanged(e);

        if (m_playingSlider != null)
        {
            //Dont update when dragging slider
            if (m_isDraggingTrackPositionSlider)
                return;

            m_playingSlider.value = e.CurrentPosition;
            m_playingSlider.maxValue = e.TotalTime;
        }

        if(m_trackPositionText != null)
        {
            string currentPosFormat = e.CurrentPositionSpan.ToString(@"mm\:ss");
            string totalTimeFormat = e.TotalTimeSpan.ToString(@"mm\:ss");
            m_trackPositionText.text = $"{currentPosFormat}/{totalTimeFormat}";
        }
    }

    protected override void OnPlayStatusChanged(PlayStatusChanged e)
    {
        base.OnPlayStatusChanged(e);
        
        if (m_playBtn != null && m_playBtn.isActiveAndEnabled != !e.IsPlaying)
        {
            m_playBtn.gameObject.SetActive(!e.IsPlaying);
        }

        if (m_pauseBtn != null && m_pauseBtn.isActiveAndEnabled != e.IsPlaying)
        {
            m_pauseBtn.gameObject.SetActive(e.IsPlaying);
        }
    }

    protected override void OnTrackChanged(TrackChanged e)
    {
        base.OnTrackChanged(e);

        if (m_artistText != null)
            m_artistText.text = e.NewTrack.Artist;

        if (m_trackText != null)
            m_trackText.text = e.NewTrack.Title;

        if (m_albumText != null)
            m_albumText.text = e.NewTrack.Album;
    }

    protected override void OnAlbumArtLoaded(Sprite s)
    {
        base.OnAlbumArtLoaded(s);

        if (m_albumArt != null)
        {
            m_albumArt.sprite = s;
        }
    }

    protected override void OnVolumeChanged(VolumeChanged e)
    {
        base.OnVolumeChanged(e);

        if(m_volumeSlider != null)
        {
            m_volumeSlider.value = e.Volume;
            m_volumeSlider.maxValue = e.MaxVolume;
        }
    }

    private void OnSetVolumeChanged(float value)
    {
        SetVolume(value);
    }

    private void OnUnmuteSound()
    {
        if (m_spotifyService.IsMuted)
        {
            m_spotifyService.SetMute(false);
        }
    }

    private void OnMuteSound()
    {
        if (!m_spotifyService.IsMuted)
        {
            m_spotifyService.SetMute(true);
        }
    }

    protected override void OnMuteChanged(MuteChanged e)
    {
        base.OnMuteChanged(e);

        m_muteBtn.gameObject.SetActive(!e.IsMuted);
        m_unmuteBtn.gameObject.SetActive(e.IsMuted);
    }

    private void OnSetTrackPosition(float sliderValue)
    {
        m_lastTrackPosSliderValue = sliderValue;
    }

    public void OnMouseDownTrackTimeSlider()
    {
        m_isDraggingTrackPositionSlider = true;
    }

    public void OnMouseUpTrackTimeSlider()
    {
        if(m_lastTrackPosSliderValue > 0f)
        {
            m_spotifyService.SetTrackPosition(m_lastTrackPosSliderValue);
        }

        m_isDraggingTrackPositionSlider = false;
        m_lastTrackPosSliderValue = -1f;
    }

    private void OnClickShuffle()
    {
        Shuffle state = m_spotifyService.ShuffleState;
        if (state == 0)
            state = (Shuffle)1;
        else
            state = (Shuffle)0;

        m_spotifyService.SetShuffle(state);
    }

    private void OnClickRepeat()
    {
        //Repeat button acts as a toggle through 3 items
        Repeat state = m_spotifyService.RepeatState;
        if (m_spotifyService.RepeatState == Repeat.Disabled)
            state = Repeat.Playlist;
        else if (m_spotifyService.RepeatState == Repeat.Playlist)
            state = Repeat.Track;
        else if (m_spotifyService.RepeatState == Repeat.Track)
            state = Repeat.Disabled;

        m_spotifyService.SetRepeat(state);
    }

    protected override void OnRepeatChanged(RepeatChanged e)
    {
        base.OnRepeatChanged(e);

        Image img = m_repeatBtn.transform.Find("Icon").GetComponent<Image>();
        img.sprite = m_repeatSprites[(int)e.State];
    }

    protected override void OnShuffleChanged(ShuffleChanged e)
    {
        base.OnShuffleChanged(e);

        Image img = m_shuffleBtn.transform.Find("Icon").GetComponent<Image>();
        img.sprite = m_shuffleSprites[(int)e.State];
    }
}
