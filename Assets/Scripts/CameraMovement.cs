using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Vector3 _position;

    public float _speed;
    Camera _camera;
    void Start()
    {
        _camera = GetComponent<Camera>();
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            _position.y += _speed * Time.deltaTime; 
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            _position.y -= _speed * Time.deltaTime; 
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _position.x += _speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _position.x -= _speed * Time.deltaTime; 
        }
        _camera.transform.position = _position;
    }
}
