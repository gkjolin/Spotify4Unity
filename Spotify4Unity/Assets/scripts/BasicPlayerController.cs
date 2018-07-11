using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicPlayerController : MonoBehaviour {

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

    SpotifyService m_spotifyService;

    public BasicPlayerController()
    {
    }

    #region MonoBehavious
    private void Awake()
    {
        if(m_nextBtn != null)
            m_nextBtn.onClick.AddListener(OnPlayMedia);
        else
            Debug.LogError("No Next button assigned!");

        if (m_pauseBtn != null)
            m_pauseBtn.onClick.AddListener(OnPauseMedia);
        else
            Debug.LogError("No Pause button assigned!");

        if (m_playBtn != null)
            m_playBtn.onClick.AddListener(OnPlayMedia);
        else
            Debug.LogError("No Play button assigned!");

        if (m_previousBtn != null)
            m_previousBtn.onClick.AddListener(OnPreviousMedia);
        else
            Debug.LogError("No Previous (<) button assigned!");

        if (m_nextBtn != null)
            m_nextBtn.onClick.AddListener(OnNextMedia);
        else
            Debug.LogError("No Next (>) button assigned!");
    }

    private void Start ()
    {
        m_spotifyService = new SpotifyService();
    }

    private void Update ()
    {
        if (m_spotifyService.IsConnected)
        {
            if (m_playingSlider != null)
            {
                m_playingSlider.value = m_spotifyService.CurrentTrackTime;
                m_playingSlider.maxValue = m_spotifyService.CurrentTrack.TotalTime;

                if(m_spotifyService.Volume != null)
                {
                    m_volumeSlider.value = m_spotifyService.Volume.CurrentVolume;
                    m_volumeSlider.maxValue = m_spotifyService.Volume.MaxVolume;
                }

                m_playingText.text = $"{m_spotifyService.CurrentTrack.Artist} - {m_spotifyService.CurrentTrack.Title} - {m_spotifyService.CurrentTrack.Album}";

                if (m_playBtn.isActiveAndEnabled != !m_spotifyService.IsPlaying)
                    m_playBtn.gameObject.SetActive(!m_spotifyService.IsPlaying);

                if (m_pauseBtn.isActiveAndEnabled != m_spotifyService.IsPlaying)
                    m_pauseBtn.gameObject.SetActive(m_spotifyService.IsPlaying);
            }
        }
    }

    private void OnDestroy()
    {
        m_spotifyService.Disconnect();
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
}
