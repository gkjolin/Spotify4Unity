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
    float m_yStartPosition = 0f;

    [SerializeField]
    float m_uiSpacing = 0f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if(m_listParent != null)
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

        if (m_savedTracks == null || m_savedTracks != null && m_savedTracks.Count == 0)
            return;

        float yPos = m_resizeCanvas.GetComponent<RectTransform>().rect.height / 2;
        float newCanvasHeight = 0f;
        foreach (Track track in m_savedTracks)
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
        m_spotifyService.PlaySong(t.InternalUri);
        Debug.Log($"Playing song '{t.Title} - {t.Artist} - {t.Album}'");
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

        m_savedTracks = e.SavedTracks;
        UpdateUI();
    }
}
