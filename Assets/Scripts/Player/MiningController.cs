using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour, HasCoolDownInterFace
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

    // Start is called before the first frame update
    void Start()
    {
        if(endOfGun == null)
            endOfGun = transform.Find("EndOfGun");

        tileMapManager = GameObject.FindWithTag("GameManager").GetComponent<TileMapManager>();
        spawnManager = GameObject.Find("ItemSpawner").GetComponent<SpawnManager>();
        //  player = GetComponentInParent<FaceMouse>().GetComponentInParent<PlayerController>();
        //  tileMapChecker = gameObject.GetComponentInParent<FaceMouse>().gameObject.GetComponentInParent<PlayerController>().gameObject.GetComponentInChildren<TileMapChecker>();
    }

    private void OnEnable()
    {
        if(endOfGun == null)
            endOfGun = transform.Find("EndOfGun");

        if (itemDatabase == null)
            itemDatabase = GetComponent<ItemDatabaseObject>();

        if(tileMapManager == null)
            tileMapManager = GameObject.FindWithTag("GameManager").GetComponent<TileMapManager>();
    }

    public Tilemap GetChunk(Vector3Int targetedBlock)
    {
        RaycastHit2D chunkCheck = Physics2D.Linecast(Vector2Int.FloorToInt(transform.position), new Vector2(targetedBlock.x, targetedBlock.y));
        if (chunkCheck.collider != null && chunkCheck.collider.gameObject.CompareTag("TileMap"))
        {
            chunk = chunkCheck.collider.attachedRigidbody.GetComponent<Tilemap>();
            return chunk;
        }
        return null;
    }

    public void Mine(Vector3Int blockToMine, float miningStr)
    {
        if(!coolDownSystem.IsOnCoolDown(id))
        {
            GetChunk(blockToMine);
            if(chunk == null)
                return;

            Vector3Int blockInLocal = chunk.WorldToCell(blockToMine);
            float blockStr;
            string blockName = tileMapManager.BlockNameGet(new Vector3Int(blockInLocal.x, blockInLocal.y, 0), chunk);

            if (!blockChecker.TryGetValue(blockInLocal, out blockStr))
            {
                blockStr = tileMapManager.BlockStrengthGet(blockInLocal, chunk);
                if(blockStr >= 0)
                {
                    blockStr -= miningStr * Time.deltaTime;
                    blockChecker.Add(blockInLocal, blockStr);
                }
            }
            else
            {
                blockStr = blockChecker[blockInLocal];
                blockStr -= miningStr * Time.deltaTime;
                blockChecker[blockInLocal] = blockStr;
            }
            if (blockStr <= 0)
            {
                //Debug.Log(blockInLocal + " position is " + blockType);
                
                DropItemFromBlock(blockInLocal, blockName, chunk);
                CheckBlockRules(blockInLocal, blockName, chunk);
                chunk.SetTile(blockInLocal, null);
                blockChecker.Remove(blockInLocal);
                coolDownSystem.PutOnCoolDown(this);
            }

        }
    }

    private void CheckBlockRules(Vector3Int blockPosition, string blockName, Tilemap chunkThis)
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
                
                if (chunkThis.HasTile(new Vector3Int(blockPosition.x, blockPosition.y + 1, 0)))
                {
                    string upperBlockType = tileMapManager.BlockNameGet(new Vector3Int(blockPosition.x, blockPosition.y + 1, 0), chunkThis);
                    if(upperBlockType == "Plant")
                    chunkThis.SetTile(new Vector3Int(blockPosition.x, blockPosition.y + 1, 0), null);
                    
                    //Drop block here
                }
                break;

            case "Tree":
                List<String> upperBlocks = new List<string>();
                bool isTree = true;
                for (int y = 0; y < 50 && isTree; y++)
                {
                    upperBlocks.Add(tileMapManager.BlockNameGet(new Vector3Int(blockPosition.x, blockPosition.y + y, 0), chunkThis));
                    if (upperBlocks[y] == "Tree")
                    {
                        chunkThis.SetTile(new Vector3Int(blockPosition.x, blockPosition.y + y, 0), null);
                        DropItemFromBlock(new Vector3Int(blockPosition.x, blockPosition.y + y, 0), blockName, chunkThis);
                    }
                    else
                        isTree = false;
                }
                upperBlocks.Clear();
                break;
        }
    }

    private void DropItemFromBlock(Vector3Int blockPosition, string blockName, Tilemap tilemap)
    {
        ItemObject itemObj = itemDatabase.GetItemOfName(blockName);
        if (itemObj != null)
        {
            ItemObject newItemObj = Instantiate(itemObj);
            //SpawnManager.SpawnItemAt(tilemap.CellToWorld(blockPosition), newItemObj);
            spawnManager.CmdSpawnItemAt(tilemap.CellToWorld(blockPosition), itemObj.Data.ID, itemObj.Data.Amount);
        }
    }
    
    public int Id => id;

    public float CoolDownDuration => coolDownDuration;
}
