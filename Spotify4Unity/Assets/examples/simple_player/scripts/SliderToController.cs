using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderToController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    ExamplePlayerController m_controller;

    public void OnPointerDown(PointerEventData eventData)
    {
        m_controller.OnMouseDownTrackTimeSlider();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_controller.OnMouseUpTrackTimeSlider();
    }
}
