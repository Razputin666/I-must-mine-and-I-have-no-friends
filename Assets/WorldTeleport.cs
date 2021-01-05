using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTeleport : MonoBehaviour
{
    private LevelGeneratorLayered mapSize;

    private void Start()
    {
        mapSize = GameObject.Find("LevelGeneration").GetComponent<LevelGeneratorLayered>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.position.x > 1)
        {

             RaycastHit2D unitMover = Physics2D.Raycast(new Vector2(1.5f, mapSize.height * 2), Vector2.down);    
             
             collision.transform.position = new Vector3(1.5f, unitMover.point.y + 2f, 0);
                
            
        }
        else
        {

             RaycastHit2D unitMover = Physics2D.Raycast(new Vector2(mapSize.startPosition.x, mapSize.height * 2), Vector2.down);
            collision.transform.position = new Vector3(mapSize.startPosition.x - 1.5f, unitMover.point.y + 2f, 0);
                
            
        }
    }

}

