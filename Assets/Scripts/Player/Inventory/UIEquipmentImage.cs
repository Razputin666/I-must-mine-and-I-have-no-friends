using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEquipmentImage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.Find("Buttface-guy");
        if(playerObject != null)
        {
            Debug.Log(playerObject);
            PlayerController player = playerObject.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log(player);
                gameObject.GetComponent<Image>().sprite = player.gameObject.GetComponent<SpriteRenderer>().sprite;
            }
        }
    }
}
