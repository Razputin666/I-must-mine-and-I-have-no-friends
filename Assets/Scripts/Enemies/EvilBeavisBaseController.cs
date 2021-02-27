using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Mirror;

public class EvilBeavisBaseController : NetworkBehaviour, HasCoolDownInterFace
{
    [HideInInspector] public List<Vector3Int> targetedOre = new List<Vector3Int>();
    [SerializeField] private Tilemap currentChunk;
    [SerializeField] private TileMapManager tileMapManager;
    [SerializeField] private GameObject minion;
    [SerializeField] private int id = 5;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private CoolDownSystem coolDownSystem;
    [SerializeField] private float oreSearchDistance;

    private float timer;

    private int blockAmount;
    private int oreAmount;
    private List<GameObject> minions = new List<GameObject>();

    public int Id => id;

    public float CoolDownDuration => coolDownDuration;

    // Start is called before the first frame update
    void Start()
    {
        currentChunk = GetCurrentChunk(transform.position);
        StartCoroutine(SearchForOres());
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        if (timer > 1f)
        {
            for (int i = 0; i < minions.Count; i++)
            {
                if (Vector3.Distance(minions[i].transform.position, transform.position) < 10f)
                {
                    MinionDropOff(minions[i]);
                }
            }
            timer = 0;
        }
    }

    private IEnumerator SearchForOres()
    {
        int startSearchX = (int)(transform.position.x - (oreSearchDistance / 2));
        int stopSearchX = (int)(transform.position.x + (oreSearchDistance / 2));

        int startSearchY = (int)(transform.position.y - (oreSearchDistance / 2));
        int stopSearchY = (int)(transform.position.y + (oreSearchDistance / 2));

        for (int x = startSearchX; x <= stopSearchX; x++)
        {
            yield return null;
            for (int y = startSearchY; y <= stopSearchY; y++)
            {
                Tilemap chunk = GetCurrentChunk(new Vector3(x, y));

                Vector3Int cellPosition = chunk.WorldToCell(new Vector3(x, y));

                if(chunk.HasTile(cellPosition))
                {
                    string blockType = TileMapManager.Instance.GetBlockType(cellPosition, chunk);

                    if(blockType == "Ore")
                    {
                        targetedOre.Add(new Vector3Int(x, y, 0));
                    }
                }
            }
        }

        targetedOre = targetedOre.OrderBy(x => Vector3Int.Distance(Vector3Int.FloorToInt(transform.position), x)).ToList();
        targetedOre.Reverse();
        minions.Add(Instantiate(minion, new Vector3(transform.position.x , transform.position.y), Quaternion.identity));

    }

    //private IEnumerator SearchForOres()
    //{
    //    string blockType;
    //    for (int x = (int)transform.position.x; x > (int)transform.position.x - 35 && x >= 0; x--)
    //    {
    //        Tilemap chunk = GetCurrentChunk(x);
    //       // yield return new WaitForSeconds(0.01f);

    //        for (int y = (int)transform.position.y; y > 0; y--)
    //        {

    //            Vector3Int blockInLocal = chunk.WorldToCell(new Vector3Int(x, y, 0));
    //           // yield return new WaitForSeconds(0.01f);
    //            if (chunk.HasTile(blockInLocal))
    //            {

    //                blockType = TileMapManager.Instance.GetBlockType(blockInLocal, chunk);
    //                if(blockType == "Ore")
    //                {
    //                    targetedOre.Add(new Vector3Int(x, y, 0));
    //                }

    //            }
    //        }
    //    }
    //    yield return new WaitForSeconds(0.01f);
    //    for (int x = (int)transform.position.x; x < (int)transform.position.x + 35 && x < (chunkList.Count * 250); x++)
    //    {

    //        Tilemap chunk = GetCurrentChunk(x);
    //      //  yield return new WaitForSeconds(0.01f);

    //        for (int y = (int)transform.position.y; y > 0; y--)
    //        {
    //            Vector3Int blockInLocal = chunk.WorldToCell(new Vector3Int(x, y, 0));
    //         //   yield return new WaitForSeconds(0.01f);
    //            if (chunk.HasTile(blockInLocal))
    //            {

    //                blockType = tileMapManager.GetBlockName(blockInLocal, chunk);
    //                if (blockType == "Ore")
    //                    targetedOre.Add(new Vector3Int(x, y, 0));
    //            }
    //        }
    //    }
    //    targetedOre = targetedOre.OrderBy(x => Vector3Int.Distance(Vector3Int.FloorToInt(transform.position), x)).ToList();
    //    targetedOre.Reverse();
    //    minions.Add(Instantiate(minion, new Vector3(transform.position.x , transform.position.y), Quaternion.identity));

    //}

    private Tilemap GetCurrentChunk(Vector3 worldPosition)
    {
        return TileMapManager.Instance.GetTileChunk(worldPosition);
    }


    private void MinionDropOff(GameObject minion)
    {

        FrogBehaviour targetedMinion = minion.GetComponent<FrogBehaviour>();
        oreAmount += targetedMinion.OreAmount;
        blockAmount += targetedMinion.BlockAmount;
        targetedMinion.BlockAmount = 0;
        targetedMinion.OreAmount = 0;
        targetedMinion.GetInventory.RemoveAllItems();
        TrySpawnMinion();
        coolDownSystem.PutOnCoolDown(this);

    }

    private void TrySpawnMinion()
    {
        if (oreAmount >= 50)
        {
            GameObject newMinion = Instantiate(minion, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
            NetworkServer.Spawn(newMinion);
            oreAmount = 0;
        }
        if (blockAmount >= 500)
        {
            GameObject newMinion = Instantiate(minion, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
            NetworkServer.Spawn(newMinion);
            blockAmount = 0;
        }
    }
}
