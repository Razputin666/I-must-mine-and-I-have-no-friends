using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
 Text HowerText;

 void Start()
 {
     HowerText = gameObject.GetComponent<Text>();
        HowerText.color = new Color(1, 1, 1, 0.5f);
 }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HowerText.color = new Color(1, 1, 1, 1);
        Debug.Log("Mouse is over GameObject.");
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        HowerText.color = new Color(1, 1, 1, 0.5f);
    }
}
