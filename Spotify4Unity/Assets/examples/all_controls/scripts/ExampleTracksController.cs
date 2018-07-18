using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ExampleTracksController : SpotifyUIBase
{
    [SerializeField]
    GameObject m_trackListPrefab;

    [SerializeField]
    Transform m_listParent;

    [SerializeField]
    RectTransform m_resizeCanvas;

    [SerializeField]
    ScrollRect m_scrollRect;

    [SerializeField]
    float m_uiSpacing = 0f;

    [SerializeField]
    Button m_sortByTitleBtn;

    [SerializeField]
    Button m_sortByArtistBtn;

    [SerializeField]
    Button m_sortByAlbumBtn;

    private Sort m_currentSort = Sort.Unsorted;
    private bool m_isSortInverted = false;

    private List<Track> m_tracks = null;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if (m_sortByTitleBtn != null)
        {
            m_sortByTitleBtn.onClick.AddListener(OnSortByTitle);
            m_sortByTitleBtn.transform.Find("Icon").gameObject.SetActive(false);
        }
        if (m_sortByArtistBtn != null)
        {
            m_sortByArtistBtn.onClick.AddListener(OnSortByArtist);
            m_sortByArtistBtn.transform.Find("Icon").gameObject.SetActive(false);
        }
        if (m_sortByAlbumBtn != null)
        {
            m_sortByAlbumBtn.onClick.AddListener(OnSortByAlbum);
            m_sortByAlbumBtn.transform.Find("Icon").gameObject.SetActive(false);
        }

        if (m_listParent != null)
            DestroyChildren(m_listParent);

        m_scrollRect.movementType = ScrollRect.MovementType.Clamped;
    }

    protected override void Update()
    {
        base.Update();
    }

    private void UpdateUI()
    {
        DestroyChildren(m_listParent);

        if (m_trackListPrefab == null)
        {
            Debug.LogError("Can't populate tracks list since no prefab specified");
            return;
        }

        
        if (m_tracks == null || m_tracks != null && m_tracks.Count == 0)
            return;

        float yPos = m_resizeCanvas.GetComponent<RectTransform>().rect.height / 2;
        float newCanvasHeight = 0f;
        foreach (Track track in m_tracks)
        {
            GameObject instPrefab = Instantiate(m_trackListPrefab);
            instPrefab.transform.SetParent(m_listParent);
            //Populate children of prefab with information
            SetChildText(instPrefab, "Title", track.Title);
            SetChildText(instPrefab, "Artist", track.Artist);
            SetChildText(instPrefab, "Album", track.Album);
            //Add listener to play button
            instPrefab.transform.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(() => OnPlayTrack(track));

            //Set Y position of instantiated prefab
            RectTransform rect = instPrefab.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(rect.rect.width, yPos, -rect.rect.width);

            //Set width and height to original prefab
            Rect original = m_trackListPrefab.GetComponent<RectTransform>().rect;
            rect.sizeDelta = new Vector2(original.width, original.height);

            float incrementAmount = instPrefab.transform.GetComponent<RectTransform>().rect.height + m_uiSpacing;
            yPos -= incrementAmount;
            newCanvasHeight += incrementAmount;
        }

        //Set canvas new size
        m_resizeCanvas.sizeDelta = new Vector2(m_resizeCanvas.rect.width, newCanvasHeight);
        //Set scrollbar position with canvas position
        m_resizeCanvas.localPosition = new Vector3(m_resizeCanvas.localPosition.x, -(m_resizeCanvas.rect.height / 2), m_resizeCanvas.localPosition.z);
        //Set sensitivity to scroll 1 track every scroll wheel click
        m_scrollRect.scrollSensitivity = m_trackListPrefab.GetComponent<RectTransform>().rect.height;
    }

    private void SetChildText(GameObject parent, string childName, string content)
    {
        parent.transform.Find(childName).GetComponent<Text>().text = content;
    }

    private void OnPlayTrack(Track t)
    {
        m_spotifyService.PlaySong(t.TrackUri, t.AlbumUri);
    }

    private void DestroyChildren(Transform parent)
    {
        List<Transform> children = parent.GetComponentsInChildren<Transform>().ToList();
        if(children.Contains(parent))
            children.Remove(parent);

        if (children.Count > 0)
        {
            foreach (Transform child in children)
                GameObject.Destroy(child.gameObject);
        }
    }

    protected override void OnSavedTracksLoaded(SavedTracksLoaded e)
    {
        base.OnSavedTracksLoaded(e);

        m_tracks = e.SavedTracks;
        UpdateUI();
    }

    public void OnSortByTitle()
    {
        DisableAll();
        GenericSort(Sort.Title, m_sortByTitleBtn);
    }

    public void OnSortByArtist()
    {
        DisableAll();
        GenericSort(Sort.Artist, m_sortByArtistBtn);
    }

    public void OnSortByAlbum()
    {
        DisableAll();
        GenericSort(Sort.Album, m_sortByAlbumBtn);
    }

    private void GenericSort(Sort sortByMode, Button btn)
    {
        if (m_currentSort == sortByMode)
        {
            if (m_isSortInverted)
            {
                //Is inverted & sorted, not restore to unsorted
                m_currentSort = Sort.Unsorted;
                m_isSortInverted = false;
                m_tracks = m_spotifyService.SavedTracks;

                btn.transform.Find("Icon").gameObject.SetActive(false);
            }
            else
            {
                //Is sorted but not inverted
                m_tracks.Reverse();
                m_isSortInverted = true;

                btn.transform.Find("Icon").gameObject.SetActive(true);
                btn.transform.Find("Icon").transform.localRotation = Quaternion.Euler(180, 0, 0);
            }
        }
        else
        {
            if (m_isSortInverted)
                m_isSortInverted = false;

            //Is unsorted, sort list
            m_tracks = m_spotifyService.GetSavedTracksSorted(sortByMode);
            m_currentSort = sortByMode;

            btn.transform.Find("Icon").gameObject.SetActive(true);
            btn.transform.Find("Icon").transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        UpdateUI();
    }

    private void DisableAll()
    {
        m_sortByTitleBtn.transform.Find("Icon").gameObject.SetActive(false);
        m_sortByArtistBtn.transform.Find("Icon").gameObject.SetActive(false);
        m_sortByAlbumBtn.transform.Find("Icon").gameObject.SetActive(false);
    }
}
