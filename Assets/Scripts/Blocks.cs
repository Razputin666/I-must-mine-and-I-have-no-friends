using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    CameraMovement playerCamera;

    private void OnEnable()
    {
        playerCamera = new CameraMovement();

        //   playerCamera.transform.position - gameObject.transform.position;

       
        

    }

}
