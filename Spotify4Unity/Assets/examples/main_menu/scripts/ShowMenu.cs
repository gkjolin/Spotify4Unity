using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMenu : MonoBehaviour
{
    [SerializeField]
    GameObject m_menu;

    [SerializeField]
    Text m_escapeText;

    private void Start()
    {
        m_escapeText.gameObject.SetActive(true);
        m_menu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            m_menu.SetActive(!m_menu.activeInHierarchy);
            m_escapeText.gameObject.SetActive(!m_escapeText.isActiveAndEnabled);
        }
    }
}
