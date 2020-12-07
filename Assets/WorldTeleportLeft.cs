using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTeleportLeft : MonoBehaviour
{
    private LevelGeneratorLayered mapSize;

    private void Start()
    {
        mapSize = GameObject.Find("LevelGeneration").GetComponent<LevelGeneratorLayered>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.position.x > 0)
        {
            collision.transform.position = new Vector3(-mapSize.width, collision.transform.position.y, 0);
        }
        else
        {
            collision.transform.position = new Vector3(mapSize.width, collision.transform.position.y, 0);
        }
    }
}
