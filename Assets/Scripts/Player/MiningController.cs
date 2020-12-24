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
    [SerializeField] private Transform[] points;
    [SerializeField] private TileMapManager tileMapManager;

    [SerializeField] private Tilemap chunk;
    private SpawnManager spawnManager;
    public Transform endOfGun;
    private Dictionary<Vector3Int, float> blockChecker = new Dictionary<Vector3Int, float>();

    private struct TileUpdateData
    {
        public TileUpdateData(Vector3Int blockPos, string newTilemapName)
        {
            blockCellPos = blockPos;
            tilemapName = newTilemapName;
        }
        public Vector3Int blockCellPos;
        public string tilemapName;
    }

    private List<TileUpdateData> tileUpdateData = new List<TileUpdateData>();

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        if (endOfGun == null)
        {
            Transform arm = gameObject.transform.Find("Gubb_arm");
            Transform heldItem = arm.Find("ItemHeldInHand");
            endOfGun = heldItem.Find("EndOfGun");
        }

        tileMapManager = GameObject.FindWithTag("GameManager").GetComponent<TileMapManager>();
        spawnManager = GameObject.Find("ItemSpawner").GetComponent<SpawnManager>();
        //  player = GetComponentInParent<FaceMouse>().GetComponentInParent<PlayerController>();
        //  tileMapChecker = gameObject.GetComponentInParent<FaceMouse>().gameObject.GetComponentInParent<PlayerController>().gameObject.GetComponentInChildren<TileMapChecker>();

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
            

        if(tileMapManager == null)
            tileMapManager = GameObject.FindWithTag("GameManager").GetComponent<TileMapManager>();
    }

    public Tilemap GetChunk(Vector3Int targetedBlock)
    {
        RaycastHit2D chunkCheck = Physics2D.Linecast(Vector2Int.FloorToInt(transform.position), new Vector2(targetedBlock.x, targetedBlock.y));
        if (chunkCheck.collider != null && chunkCheck.collider.gameObject.CompareTag("TileMap"))
        {
            return chunkCheck.collider.attachedRigidbody.GetComponent<Tilemap>();
        }
        return null;
    }

    [Server]
    public void Mine(Vector3Int blockToMine, float miningStr)
    {
        if(!coolDownSystem.IsOnCoolDown(id))
        {
            chunk = GetChunk(blockToMine);

            if (chunk == null)
                return;

            Vector3Int blockInCell = chunk.WorldToCell(blockToMine);
            float blockStr;
            string blockName = tileMapManager.BlockNameGet(new Vector3Int(blockInCell.x, blockInCell.y, 0), chunk);

            if (!blockChecker.TryGetValue(blockInCell, out blockStr))
            {
                blockStr = tileMapManager.BlockStrengthGet(blockInCell, chunk);
                if(blockStr >= 0)
                {
                    blockStr -= miningStr * Time.deltaTime;
                    blockChecker.Add(blockInCell, blockStr);
                }
            }
            else
            {
                blockStr = blockChecker[blockInCell];
                blockStr -= miningStr * Time.deltaTime;
                blockChecker[blockInCell] = blockStr;
            }
            if (blockStr <= 0)
            {
                //Debug.Log(blockInLocal + " position is " + blockType);
                
                DropItemFromBlock(blockInCell, blockName, chunk);
                CheckBlockRules(blockInCell, blockName, chunk, blockToMine);
                //chunk.SetTile(blockInLocal, null);
                blockChecker.Remove(blockInCell);
                coolDownSystem.PutOnCoolDown(this);

                SendTilemapUpdate(blockInCell, chunk.name);
            }

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
                    string upperBlockType = tileMapManager.BlockNameGet(new Vector3Int(blockPosition.x, blockPosition.y + 1, 0), currentChunk);
                    if(upperBlockType == "Plant")
                    {
                        Vector3Int cellBlockPos = new Vector3Int(blockPosition.x, blockPosition.y + 1, 0);
                        currentChunk.SetTile(cellBlockPos, null);

                        SendTilemapUpdate(cellBlockPos, currentChunk.name);

                        //Drop block here
                    }
                }
                break;

            case "Tree":
                List<String> upperBlocks = new List<string>();
                bool isTree = true;
                
                for (int y = 0; y < 50 && isTree; y++)
                {
                    upperBlocks.Add(tileMapManager.BlockNameGet(new Vector3Int(blockPosition.x, blockPosition.y + y, 0), currentChunk));
                    if (upperBlocks[y] == "Tree")
                    {
                        Vector3Int cellBlockPos = new Vector3Int(blockPosition.x, blockPosition.y + y, 0);
                        currentChunk.SetTile(cellBlockPos, null);
                        DropItemFromBlock(cellBlockPos, blockName, currentChunk);

                        SendTilemapUpdate(cellBlockPos, currentChunk.name);
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
            spawnManager.CmdSpawnItemAt(tilemap.CellToWorld(blockPosition), itemObj.Data.ID, itemObj.Data.Amount);
        }
    }

    [Command]
    public void CmdMineBlockAt(Vector3Int blockPos, float miningStr)
    {
        Mine(blockPos, miningStr);
        Debug.Log("Mining Server");
    }

    private void SendTilemapUpdate(Vector3Int blockPos, string tilemapName)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            MiningController playerMine = player.GetComponent<MiningController>();
            playerMine.RpcUpdateTilemap(player.GetComponent<NetworkIdentity>().connectionToClient, blockPos, tilemapName);
        }
    }

    [TargetRpc]
    public void RpcUpdateTilemap(NetworkConnection conn, Vector3Int blockPos, string tilemapName)
    {
        Debug.Log("Update Tilemap");
        if (!GetComponent<PlayerController>().IsReady)
        {
            Debug.Log("Adding to list: " + tileUpdateData.Count);
            tileUpdateData.Add(new TileUpdateData(blockPos, tilemapName));
            return;
        }

        GameObject grid = GameObject.Find("Grid");

        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();

        for (int i = 0; i < tilemaps.Length; i++)
        {
            if (tilemaps[i].name == tilemapName)
            {
                Debug.Log("setting tiles");
                tilemaps[i].SetTile(blockPos, null);
                return;
            }
        }     
    }

    [Client]
    public void UpdateTiles()
    {
        Debug.Log("Tiles: " + tileUpdateData.Count);
        foreach (TileUpdateData data in tileUpdateData)
        {
            GameObject grid = GameObject.Find("Grid");

            Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();

            for (int i = 0; i < tilemaps.Length; i++)
            {
                if (tilemaps[i].name == data.tilemapName)
                {
                    tilemaps[i].SetTile(data.blockCellPos, null);
                }
            }
        }
        tileUpdateData.Clear();
    }
    
    public int Id => id;

    public float CoolDownDuration => coolDownDuration;
}
