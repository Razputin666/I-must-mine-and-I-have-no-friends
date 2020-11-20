using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapChecker : MonoBehaviour
{
    public Tilemap currentTilemap;
    int timer;
    Collider2D sphereCheck;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "TileMap")
        {
            currentTilemap = collision.gameObject.GetComponent<Tilemap>();
            Debug.Log("NewTileMap");
        }
    }

}
