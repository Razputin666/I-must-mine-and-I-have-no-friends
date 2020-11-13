using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Vector3 pos;

    public float speed;
    Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            pos.y += speed * Time.deltaTime; 
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            pos.y -= speed * Time.deltaTime; 
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            pos.x += speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            pos.x -= speed * Time.deltaTime; 
        }
        cam.transform.position = pos;
    }
}
