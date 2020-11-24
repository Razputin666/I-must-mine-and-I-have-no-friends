using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
 Text HowerText;
    AudioSource audioData;

    void Start()
 {
     HowerText = gameObject.GetComponent<Text>();
        HowerText.color = new Color(1, 1, 1, 0.5f);
        audioData = GetComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HowerText.color = new Color(1, 1, 1, 1);
        audioData.Play(0);
        
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        HowerText.color = new Color(1, 1, 1, 0.5f);
    }
}
