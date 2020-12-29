using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class BuildingController : NetworkBehaviour, HasCoolDownInterFace
{
    [SerializeField] private int id = 3;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private CoolDownSystem coolDownSystem;

    public int Id => id;

    public float CoolDownDuration => coolDownDuration;

    [Client]
    public void Build(Vector2 mousePosition, string tileBase)
    {
        if (!coolDownSystem.IsOnCoolDown(id))
        {
            CmdBuildAt(mousePosition, tileBase);
            coolDownSystem.PutOnCoolDown(this);
        }
    }

    [Command]
    private void CmdBuildAt(Vector2 mousePosition, string tileBaseName)
    {
        Tilemap tilemap = GetTilemap(mousePosition);
        
        if (tilemap == null)
        {
            Debug.LogError("Failed to find tilemap, Outside bounds");
            return;
        }

        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);

        if (tilemap.HasTile(cellPosition))
            return;

        if(TilemapSyncManager.Instance.UpdateTilemap(tilemap.name, cellPosition, tileBaseName))
        {
            GetComponent<PlayerController>().RpcRemoveItemFromQuickslot(connectionToClient, 1);
        }
        
    }

    [Server]
    private Tilemap GetTilemap(Vector2 worldPosition)
    {
        List<Tilemap> tilemaps = TilemapSyncManager.Instance.Tilemaps;
        foreach (Tilemap tilemap in tilemaps)
        {
            BoundsInt bounds = tilemap.cellBounds;
            Vector3 tilemapPos = tilemap.transform.position;
            
            if(Inside(tilemapPos, bounds.size, worldPosition))
            {
                return tilemap;
            }
        }

        return null;
    }

    [Server]
    private bool Inside(Vector3 pos, Vector3Int size, Vector2 point)
    {
        //Debug.Log("Tilemap Pos: " + pos);
        //Debug.Log("tilemap Bounds pos: " + new Vector3(pos.x + size.x, pos.y + size.y));
        //Debug.Log("MousePos: " + point);
        if (pos.x <= point.x &&
            point.x <= pos.x + size.x &&
            pos.y <= point.y &&
            point.y <= pos.y + size.y)
        {
            return true;
        }

        return false;
    }
}
