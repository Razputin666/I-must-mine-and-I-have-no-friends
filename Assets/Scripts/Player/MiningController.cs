using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class MiningController : NetworkBehaviour, HasCoolDownInterFace
{
    [SerializeField] private ItemDatabaseObject itemDatabase;
    [SerializeField] private int id = 2;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private CoolDownSystem coolDownSystem;
    //[SerializeField] private Transform[] points;

    [SerializeField] private Tilemap chunk;
    private SpawnManager spawnManager;
    public Transform endOfGun;
    private Dictionary<Vector3Int, float> blockChecker = new Dictionary<Vector3Int, float>();

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        //if (endOfGun == null)
        //{
        //    Transform arm = gameObject.transform.Find("Gubb_arm");
        //    Transform heldItem = arm.Find("ItemHeldInHand");
        //    endOfGun = heldItem.Find("EndOfGun");
        //}

        //spawnManager = GameObject.Find("ItemSpawner").GetComponent<SpawnManager>();
        //  player = GetComponentInParent<FaceMouse>().GetComponentInParent<PlayerController>();
        //  tileMapChecker = gameObject.GetComponentInParent<FaceMouse>().gameObject.GetComponentInParent<PlayerController>().gameObject.GetComponentInChildren<TileMapChecker>();

    }

    private void Awake()
    {
        if (endOfGun == null)
        {
            Transform arm = gameObject.transform.Find("Gubb_arm");
            Transform heldItem = arm.Find("ItemHeldInHand");
            endOfGun = heldItem.Find("EndOfGun");
        }

        spawnManager = GameObject.Find("ItemSpawner").GetComponent<SpawnManager>();
    }

    private void OnEnable()
    {
        if (!hasAuthority)
            return;

        if(endOfGun == null)
        {
            Transform arm = gameObject.transform.Find("Gubb_arm");
            Transform heldItem = arm.Find("ItemHeldInHand");
            endOfGun = heldItem.Find("EndOfGun");
        }
    }
    [Server]
    public Tilemap GetChunk(Vector3 targetedBlock)
    {
        //RaycastHit2D chunkCheck = Physics2D.Linecast(Vector2Int.FloorToInt(transform.position), new Vector2(targetedBlock.x, targetedBlock.y));
        //if (chunkCheck.collider != null && chunkCheck.collider.gameObject.CompareTag("TileMap"))
        //{
        //    return chunkCheck.collider.attachedRigidbody.GetComponent<Tilemap>();
        //}
        //return null;

        return GetTilemap(targetedBlock);
    }

    [Server]
    private Tilemap GetTilemap(Vector2 worldPosition)
    {
        List<Tilemap> tilemaps = TileMapManager.Instance.Tilemaps;
        foreach (Tilemap tilemap in tilemaps)
        {
            BoundsInt bounds = tilemap.cellBounds;
            Vector3 tilemapPos = tilemap.transform.position;

            if (Inside(tilemapPos, bounds.size, worldPosition))
                return tilemap;
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
    [Command]
    private void CmdMineBlockAt(Vector3 blockToMine, float miningStr)
    {
        ServerMine(blockToMine, miningStr);
    }

    [Server]
    private void ServerMine(Vector3 blockToMine, float miningStr)
    {
        chunk = GetChunk(blockToMine);

        if (chunk == null)
            return;
        
        Vector3Int blockInCell = chunk.WorldToCell(blockToMine);

        if (chunk.GetTile(Vector3Int.FloorToInt(blockInCell)) == null)
            return;
        
        float blockStr;
        string blockName = TileMapManager.Instance.GetBlockName(new Vector3Int(blockInCell.x, blockInCell.y, 0), chunk);

        if (!blockChecker.TryGetValue(blockInCell, out blockStr))
        {
            blockStr = TileMapManager.Instance.GetBlockStrength(blockInCell, chunk);
            if(blockStr >= 0)
            {
                blockStr -= miningStr * coolDownDuration;
                blockChecker.Add(blockInCell, blockStr);
            }
        }
        else
        {
            blockStr = blockChecker[blockInCell];
            blockStr -= miningStr * coolDownDuration;
            blockChecker[blockInCell] = blockStr;
        }
        if (blockStr <= 0)
        {
            DropItemFromBlock(blockInCell, blockName, chunk);
            CheckBlockRules(blockInCell, blockName, chunk, blockToMine);
            blockChecker.Remove(blockInCell);
            TileMapManager.Instance.UpdateTilemap(chunk.name, blockInCell, string.Empty);
        }
    }

    [Server]
    private void CheckBlockRules(Vector3Int blockPosition, string blockName, Tilemap currentChunk, Vector3 blockWorldPos)
    {
        switch (blockName)
        {
            case "Dirt":
                //Debug.Log("DIRT!");
                //ItemObject itemObj = itemDatabase.GetItemAt(3);
                //if (itemObj != null)
                //{
                //    ItemObject newItemObj = Instantiate(itemObj);
                //    spawnManager.CmdSpawnItemAt(chunkThis.CellToWorld(blockPosition), itemObj.Data.ID, itemObj.Data.Amount);
                //}
                break;
            case "Grass":
                
                if (currentChunk.HasTile(new Vector3Int(blockPosition.x, blockPosition.y + 1, 0)))
                {
                    string upperBlockType = TileMapManager.Instance.GetBlockName(new Vector3Int(blockPosition.x, blockPosition.y + 1, 0), currentChunk);
                    if(upperBlockType == "Plant")
                    {
                        Vector3Int cellBlockPos = new Vector3Int(blockPosition.x, blockPosition.y + 1, 0);

                        TileMapManager.Instance.UpdateTilemap(currentChunk.name, cellBlockPos, string.Empty);

                        //Drop block here
                    }
                }
                break;

            case "Tree":
                List<String> upperBlocks = new List<string>();
                bool isTree = true;
                
                for (int y = 0; y < 50 && isTree; y++)
                {
                    upperBlocks.Add(TileMapManager.Instance.GetBlockName(new Vector3Int(blockPosition.x, blockPosition.y + y, 0), currentChunk));
                    if (upperBlocks[y] == "Tree")
                    {
                        Vector3Int cellBlockPos = new Vector3Int(blockPosition.x, blockPosition.y + y, 0);

                        DropItemFromBlock(cellBlockPos, blockName, currentChunk);

                        TileMapManager.Instance.UpdateTilemap(currentChunk.name, cellBlockPos, string.Empty);
                    }
                    else
                        isTree = false;
                }
                upperBlocks.Clear();
                break;
        }
    }

    [Server]
    private void DropItemFromBlock(Vector3Int blockPosition, string blockName, Tilemap tilemap)
    {
        ItemObject itemObj = itemDatabase.GetItemOfName(blockName);

        if (itemObj != null)
        {
            spawnManager.SpawnItemAt(tilemap.CellToWorld(blockPosition), blockName);
        }
    }

    public void Mine(Vector3 blockWorldPosition, float miningStr)
    {
        if (!coolDownSystem.IsOnCoolDown(id))
        {
            if (isClient)
                CmdMineBlockAt(blockWorldPosition, miningStr);
            else
                ServerMine(blockWorldPosition, miningStr);

            coolDownSystem.PutOnCoolDown(this);
        }  
    }
    
    public int Id => id;

    public float CoolDownDuration => coolDownDuration;
}