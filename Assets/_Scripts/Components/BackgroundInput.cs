using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundInput : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // Need this because Pointer Up will not work otherwise.. 
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.Instance.CheckForWord();
    }
}
