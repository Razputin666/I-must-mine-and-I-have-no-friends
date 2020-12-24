using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class PlayerStatesController : NetworkBehaviour
{
    private PlayerController player;
    private LineController line;
    private MiningController miningController;

    Vector3Int targetBlockIntPos;
    // Start is called before the first frame update
    public override void OnStartLocalPlayer()
    {
        player = GetComponent<PlayerController>();
        //tileMapChecker = GetComponentInChildren<TileMapChecker>();
        line = transform.Find("Gubb_arm").GetComponentInChildren<LineController>();
        miningController = GetComponent<MiningController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;
        switch (player.playerStates)
        {
            case PlayerController.PlayerStates.Mining:
                targetBlockIntPos = Vector3Int.FloorToInt(player.mousePosInWorld);
                targetBlockIntPos.z = 0;
                Vector3Int playerIntPos = Vector3Int.FloorToInt(transform.position);
                Vector3Int distanceFromPlayer = targetBlockIntPos - playerIntPos;
                //Debug.Log(targetBlockIntPos + " player mousepos");
                //if(Input.GetMouseButtonDown(0))
                //{
                //    Debug.Log(player.worldPosition);
                //}

                if (Input.GetMouseButton(0) && distanceFromPlayer.x > -5 && distanceFromPlayer.x < 5 && distanceFromPlayer.y > -5 && distanceFromPlayer.y < 5)
                {
                    line.enabled = true;

                    miningController.CmdMineBlockAt(targetBlockIntPos, player.MiningStrength);
                }
                break;
            case PlayerController.PlayerStates.Normal:
                break;
            case PlayerController.PlayerStates.Building:
                break;
            case PlayerController.PlayerStates.Idle:
                break;
            default:
                break;
        }
    }
}