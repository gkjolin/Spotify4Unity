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
    Button m_previousBtn;

    [SerializeField]
    Button m_nextBtn;

    [SerializeField]
    Button m_playBtn;

    [SerializeField]
    Button m_pauseBtn;

    [SerializeField]
    Image m_albumArt;

    #region MonoBehavious
    protected override void Awake()
    {
        base.Awake();

        if(m_nextBtn != null)
            m_nextBtn.onClick.AddListener(OnPlayMedia);

        if (m_pauseBtn != null)
            m_pauseBtn.onClick.AddListener(OnPauseMedia);

        if (m_playBtn != null)
            m_playBtn.onClick.AddListener(OnPlayMedia);

        if (m_previousBtn != null)
            m_previousBtn.onClick.AddListener(OnPreviousMedia);

        if (m_nextBtn != null)
            m_nextBtn.onClick.AddListener(OnNextMedia);
    }

    protected override void Update ()
    {
        if (m_spotifyService.IsConnected)
        {
            //Get current track information and set text
            Track currentTrack = GetCurrentSongInfo();
            m_playingText.text = $"{m_spotifyService.CurrentTrack.Artist} - {m_spotifyService.CurrentTrack.Title} - {m_spotifyService.CurrentTrack.Album}";

            //m_albumArt = currentTrack.Album;
        }
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
    }

    protected override void OnAlbumArtLoaded(Sprite s)
    {
        base.OnAlbumArtLoaded(s);

        if (m_albumArt != null)
        {
            m_albumArt.sprite = s;
            Debug.Log("Album art loaded");
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
}
