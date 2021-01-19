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
    private BuildingController buildingController;

    Vector3Int targetBlockIntPos;
    // Start is called before the first frame update
    public override void OnStartLocalPlayer()
    {
        player = GetComponent<PlayerController>();

        line = transform.Find("Gubb_arm").GetComponentInChildren<LineController>();
        miningController = GetComponent<MiningController>();
        buildingController = GetComponent<BuildingController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;
        Vector3 mousePosition = player.mousePosInWorld;
        
        Vector3 playerPosition = transform.position;

        Vector3 distance = mousePosition - playerPosition;

        switch (player.playerStates)
        {
            case PlayerController.PlayerStates.Mining:
                
                if (Input.GetMouseButton(0) && distance.x > -5f && distance.x < 5f && distance.y > -5f && distance.y < 5f)
                {
                    line.enabled = true;

                    miningController.Mine(mousePosition, player.MiningStrength);
                }
                break;
            case PlayerController.PlayerStates.Normal:
                break;
            case PlayerController.PlayerStates.Building:
                if (Input.GetMouseButton(0) && distance.x > -5f && distance.x < 5f && distance.y > -5f && distance.y < 5f)
                {
                    ItemObject itemObj = player.GetActiveItem();
                    if (itemObj != null)
                    {
                        buildingController.Build(mousePosition, itemObj.Data.Name);
                    }
                }
                break;
            case PlayerController.PlayerStates.Idle:
                break;
            default:
                break;
        }
    }
}