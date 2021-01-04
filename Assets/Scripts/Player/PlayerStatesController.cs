using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerStatesController : MonoBehaviour
{
    private ItemHandler itemHandler;
    private PlayerController player;
    private LineController line;
    private MiningController miningMode;
    private TileMapChecker tileMapChecker;

    private Tilemap targetedBlock;

    Vector3Int targetBlockIntPos;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerController>();
        tileMapChecker = GetComponentInChildren<TileMapChecker>();
        line = GetComponentInChildren<FaceMouse>().GetComponentInChildren<LineController>();
        miningMode = GetComponentInChildren<FaceMouse>().GetComponentInChildren<MiningController>();
        itemHandler = GetComponent<ItemHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (player.playerStates)
        {
            case PlayerController.PlayerStates.Mining:
                targetBlockIntPos = Vector3Int.FloorToInt(player.worldPosition);
                targetBlockIntPos.z = 0;
                Vector3Int playerIntPos = Vector3Int.FloorToInt(transform.position);
                Vector3Int distanceFromPlayer = targetBlockIntPos - playerIntPos;
               // TargetedBlock = tileMapChecker.currentTilemap;
                //Debug.Log(targetBlockIntPos + " player mousepos");

                if (Input.GetMouseButton(0) && distanceFromPlayer.x > -5 && distanceFromPlayer.x < 5 && distanceFromPlayer.y > -5 && distanceFromPlayer.y < 5)
                {
                    line.enabled = true;
                    miningMode.Mine(targetBlockIntPos, player.miningStrength);

                }
                break;
            case PlayerController.PlayerStates.Normal:
                break;
            case PlayerController.PlayerStates.Building:

                break;
            default:
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
