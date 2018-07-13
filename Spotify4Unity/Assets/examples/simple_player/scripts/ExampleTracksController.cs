using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleTracksController : SpotifyUIBase
{
    [SerializeField]
    GameObject m_trackListPrefab;

    [SerializeField]
    Transform m_listParent;

    [SerializeField]
    RectTransform m_resizeCanvas;

    [SerializeField]
    float m_yStartPosition = 0f;

    [SerializeField]
    float m_uiSpacing = 0f;

    private List<Track> m_savedTracks = null;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if(m_listParent != null)
            DestroyChildren(m_listParent);

        m_savedTracks = m_spotifyService.GetSavedTracks();
        UpdateUI();
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

        Debug.Log(m_savedTracks.Count);
        float yPos = m_resizeCanvas.GetComponent<RectTransform>().rect.height / 2;
        float newCanvasHeight = 0f;
        Debug.Log("YPos = " + yPos);
        foreach (Track track in m_savedTracks)
        {
            GameObject instPrefab = Instantiate(m_trackListPrefab);
            instPrefab.transform.SetParent(m_listParent);
            instPrefab.transform.Find("Track").GetComponent<UnityEngine.UI.Text>().text = $"{track.Title} - {track.Artist} - {track.Album}";
            RectTransform rect = instPrefab.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(0f, yPos, 0f);

            float incrementAmount = instPrefab.transform.GetComponent<RectTransform>().rect.height + m_uiSpacing;
            yPos -= incrementAmount;
            newCanvasHeight += incrementAmount;
        }

        //Set canvas new size
        m_resizeCanvas.sizeDelta = new Vector2(m_resizeCanvas.rect.width, newCanvasHeight);
        //Set scrollbar position with canvas position
        m_resizeCanvas.localPosition = new Vector3(m_resizeCanvas.localPosition.x, -(m_resizeCanvas.rect.height / 2), m_resizeCanvas.localPosition.z);
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
}
