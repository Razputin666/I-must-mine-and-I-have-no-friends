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

    public Transform endOfGun;
    private Dictionary<Vector3Int, float> blockChecker = new Dictionary<Vector3Int, float>();

    // Start is called before the first frame update
    void Start()
    {
        tileMapManager = GameObject.FindWithTag("GameManager").GetComponent<TileMapManager>();
        endOfGun = transform.Find("EndOfGun");
      //  player = GetComponentInParent<FaceMouse>().GetComponentInParent<PlayerController>();
      //  tileMapChecker = gameObject.GetComponentInParent<FaceMouse>().gameObject.GetComponentInParent<PlayerController>().gameObject.GetComponentInChildren<TileMapChecker>();
    }

    private void OnEnable()
    {
        if(itemDatabase == null)
            itemDatabase = GetComponent<ItemDatabaseObject>();
    }


    public void Mine(Vector3Int blockToMine, float miningStr)
    {
        if(!coolDownSystem.IsOnCoolDown(id))
        {
            RaycastHit2D chunkCheck = Physics2D.Linecast(transform.position, new Vector2(blockToMine.x, blockToMine.y));
            if(chunkCheck.collider != null && chunkCheck.collider.gameObject.CompareTag("TileMap"))
            {
                chunk = chunkCheck.collider.attachedRigidbody.GetComponent<Tilemap>();
            }
            if (chunk == null)
                return;
            Vector3Int blockInLocal = chunk.WorldToCell(blockToMine);
            float blockStr = 0;
            string blockType = tileMapManager.BlockTypeGet(new Vector3Int(blockInLocal.x, blockInLocal.y, 0), chunk);
            


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
            Debug.Log(blockType);
            if (blockStr <= 0)
            {
                
                CheckBlockRules(blockInLocal, blockType, chunk);
                chunk.SetTile(blockInLocal, null);
                blockChecker.Remove(blockInLocal);
                coolDownSystem.PutOnCoolDown(this);
            }

        }
    }

    private void CheckBlockRules(Vector3Int blockPosition, string blockType, Tilemap chunkThis)
    {
        switch (blockType)
        {
            case "Dirt":
                //Debug.Log("DIRT!");
                ItemObject itemObj = itemDatabase.GetItemAt(3);
                Debug.Log(itemObj);
                if (itemObj != null)
                {
                    ItemObject newItemObj = Instantiate(itemObj);
                    SpawnManager.SpawnItemAt(chunkThis.CellToWorld(blockPosition), newItemObj);
                }
                break;
            case "Grass":
                
                if (chunkThis.HasTile(new Vector3Int(blockPosition.x, blockPosition.y + 1, 0)))
                {
                    string upperBlockType = tileMapManager.BlockTypeGet(new Vector3Int(blockPosition.x, blockPosition.y + 1, 0), chunkThis);
                    if(upperBlockType == "Plant")
                    chunkThis.SetTile(new Vector3Int(blockPosition.x, blockPosition.y + 1, 0), null);
                    //Drop block here
                }
                break;

            case "Tree":
                List<String> upperBlocks = new List<string>();
                
                for (int y = 0; y < 15; y++)
                {
                    upperBlocks.Add(tileMapManager.BlockTypeGet(new Vector3Int(blockPosition.x, blockPosition.y + y, 0), chunkThis));
                    if (upperBlocks[y] == "Tree")
                    {
                        chunkThis.SetTile(new Vector3Int(blockPosition.x, blockPosition.y + y, 0), null);
                    }
                }
                upperBlocks.Clear();
                break;


        }
    }


    public int Id => id;

    public float CoolDownDuration => coolDownDuration;



}
