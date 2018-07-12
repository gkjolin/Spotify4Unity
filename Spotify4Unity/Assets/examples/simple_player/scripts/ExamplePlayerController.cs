using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExamplePlayerController : SpotifyUIBase
{
    [SerializeField]
    Text m_playingText;

    [SerializeField]
    Slider m_playingSlider;

    [SerializeField]
    Slider m_volumeSlider;

    [SerializeField]
    Button m_muteBtn;

    [SerializeField]
    Button m_unmuteBtn;

    [SerializeField]
    Button m_previousBtn;

    [SerializeField]
    Button m_nextBtn;

    [SerializeField]
    Button m_playBtn;

    [SerializeField]
    Button m_pauseBtn;

    [SerializeField]
    Image m_albumArt;

    bool m_isDraggingTrackPositionSlider = false;

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
        {
            m_playingSlider.onValueChanged.AddListener(OnSetTrackPosition);
        }

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
            if (m_isDraggingTrackPositionSlider)
                return;

            m_playingSlider.value = e.CurrentTime;
            m_playingSlider.maxValue = e.TotalTime;
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
        
        m_playingText.text = $"{e.NewTrack.Artist} - {e.NewTrack.Title} - {e.NewTrack.Album}";
    }

    protected override void OnAlbumArtLoaded(Sprite s)
    {
        base.OnAlbumArtLoaded(s);

        if (m_albumArt != null)
        {
            m_albumArt.sprite = s;
            //Debug.Log("Album art loaded");
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
        if (m_isDraggingTrackPositionSlider)
        {
            m_spotifyService.SetTrackPosition(sliderValue);
            Debug.Log($"Set track position to {sliderValue} seconds");
        }
    }

    public void OnMouseDownTrackTimeSlider()
    {
        m_isDraggingTrackPositionSlider = true;
    }

    public void OnMouseUpTrackTimeSlider()
    {
        m_isDraggingTrackPositionSlider = false;
    }
}
