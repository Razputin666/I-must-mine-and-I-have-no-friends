using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour
{

    PlayerController player;
    TileMapChecker tileMapChecker;

    Vector3 worldPosition;
    private bool inPrecisionMode;
    private bool inFreeMode;

    private Tilemap blockTile;
    private Tilemap targetedBlock;

    Vector3Int targetBlockIntPos;

    float timer;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerController>();
        tileMapChecker = gameObject.GetComponentInChildren<TileMapChecker>();
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 mousePos = Input.mousePosition;
        worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        targetBlockIntPos = Vector3Int.FloorToInt(worldPosition);
        targetBlockIntPos.z = 0;
        Vector3Int playerIntPos = Vector3Int.FloorToInt(transform.position);
        

        switch (player.unitMode)
        {
            case PlayerController.UnitMode.Mining:

                TargetedBlock = tileMapChecker.currentTilemap;
                Vector3Int distanceFromPlayer = targetBlockIntPos - playerIntPos;
                
                timer += Time.deltaTime;

                if (Input.GetMouseButton(0) && distanceFromPlayer.x > -5 && distanceFromPlayer.x < 5 && distanceFromPlayer.y > -5 && distanceFromPlayer.y < 5)
                {
                   
                    if (timer >= 0.2f) 
                    { 
                    timer = 0f;
                    TargetedBlock.SetTile(targetBlockIntPos, null);
                    }
                }
                break;
        }  
    }


public Tilemap TargetedBlock
    {
        get
        {
            return targetedBlock;

        }

        set
        {
            targetedBlock = value;
        }
    }
}
