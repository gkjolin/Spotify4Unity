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
        {
            m_nextBtn.onClick.AddListener(OnPlayMedia);
        }
        else
        {
            Debug.LogError("Unable to listen for Spotify Play. No Play button assigned!");
        }

        if (m_pauseBtn != null)
        {
            m_pauseBtn.onClick.AddListener(OnPauseMedia);
        }
        else
        {
            Debug.LogError("Unable to listen for Spotify Play. No Pause button assigned!");
        }

        if (m_previousBtn != null)
        {
            m_previousBtn.onClick.AddListener(OnPreviousMedia);
        }
        else
        {
            Debug.LogError("Unable to listen for Spotify Play. No Previous (<) button assigned!");
        }

        if (m_nextBtn != null)
        {
            m_nextBtn.onClick.AddListener(OnNextMedia);
        }
        else
        {
            Debug.LogError("Unable to listen for Spotify Next. No Next (>) button assigned!");
        }
    }

    private void Start ()
    {
        m_spotifyService = new SpotifyService();
    }

    private void Update ()
    {
        if(m_spotifyService.IsConnected)
        {
            SongInfo currentInfo = m_spotifyService.GetCurrentInfo();
            bool isPlaying = currentInfo != null;
            if (currentInfo != null)
            {
                m_playingSlider.value = (float)currentInfo.CurrentTime;
                m_playingSlider.maxValue = (float)currentInfo.TotalDuration;

                m_playingText.text = $"{currentInfo.Artist} - {currentInfo.Title} - {currentInfo.AlbumName}";
            }

            if (m_playBtn.isActiveAndEnabled != !isPlaying)
                m_playBtn.gameObject.SetActive(!isPlaying);

            if (m_pauseBtn.isActiveAndEnabled != isPlaying)
                m_pauseBtn.gameObject.SetActive(isPlaying);
        }
    }
    #endregion

    public void Connect()
    {
        m_spotifyService.Connect();
    }

    private void OnNextMedia()
    {
    }

    private void OnPreviousMedia()
    {
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
