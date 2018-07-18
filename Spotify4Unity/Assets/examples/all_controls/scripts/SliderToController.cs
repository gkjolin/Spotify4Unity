using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Used to detect mouse up and mouse down movements on a slider a send to our controller class
/// </summary>
public class SliderToController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    protected ExamplePlayerController m_controller;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
    }
}
