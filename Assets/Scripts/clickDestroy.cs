using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clickDestroy : MonoBehaviour
{
    int i = 0;
    void OnMouseDown()
    {
            i++;
        if(i==2)
        Destroy(gameObject);
    }
}
