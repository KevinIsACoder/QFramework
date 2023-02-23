using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISound : MonoBehaviour,
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    string _clickSound = "ui_click";

    public void PlayClick()
    {
        // if (!string.IsNullOrEmpty(_clickSound)) { SoundPlayer.Play(_clickSound); }
    }

    [SerializeField]
    string _downSound;
    public void PlayDown()
    {
        // if (!string.IsNullOrEmpty(_downSound)) { SoundPlayer.Play(_downSound); }
    }

    [SerializeField]
    string _upSound;
    public void PlayUp()
    {
        // if (!string.IsNullOrEmpty(_upSound)) { SoundPlayer.Play(_upSound); }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        PlayClick();
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        PlayDown();
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        PlayUp();
    }
}