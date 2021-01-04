using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class EvilBeavisBaseController : MonoBehaviour, HasCoolDownInterFace
{

    [HideInInspector] public List<Vector3Int> targetedOre = new List<Vector3Int>();
    [SerializeField] private List<Tilemap> chunkList;
    [SerializeField] private Tilemap currentChunk;
    [SerializeField] private TileMapManager tileMapManager;
    [SerializeField] private GameObject minion;
    [SerializeField] private int id = 5;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private CoolDownSystem coolDownSystem;

    private LevelGeneratorLayered mapGen;
    private int width;
    private int height;
    private float timer;

    private int blockAmount;
    private int oreAmount;
    private FrogBehaviour targetedMinion;
    private List<GameObject> minions = new List<GameObject>();

    public int Id => id;

    public float CoolDownDuration => coolDownDuration;

    // Start is called before the first frame update
    void Start()
    {
        tileMapManager = GameObject.FindWithTag("GameManager").GetComponent<TileMapManager>();
        mapGen = GameObject.Find("LevelGeneration").GetComponent<LevelGeneratorLayered>();
        chunkList = mapGen.chunks;
        width = mapGen.width;
        height = mapGen.height;
        currentChunk = GetCurrentChunk(transform.position.x);
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
        string blockType;
        for (int x = (int)transform.position.x; x > (int)transform.position.x - 35 && x >= 0; x--)
        {
            Tilemap chunk = GetCurrentChunk(x);
           // yield return new WaitForSeconds(0.01f);
            
            for (int y = (int)transform.position.y; y > 0; y--)
            {
                
                Vector3Int blockInLocal = chunk.WorldToCell(new Vector3Int(x, y, 0));
               // yield return new WaitForSeconds(0.01f);
                if (chunk.HasTile(blockInLocal))
                {
                    
                    blockType = tileMapManager.BlockTypeGet(blockInLocal, chunk);
                    if(blockType == "Ore")
                    {
                        targetedOre.Add(new Vector3Int(x, y, 0));
                    }
                    
                }
            }
        }
        yield return new WaitForSeconds(0.01f);
        for (int x = (int)transform.position.x; x < (int)transform.position.x + 35 && x < (chunkList.Count * 250); x++)
        {
          
            Tilemap chunk = GetCurrentChunk(x);
          //  yield return new WaitForSeconds(0.01f);

            for (int y = (int)transform.position.y; y > 0; y--)
            {
                Vector3Int blockInLocal = chunk.WorldToCell(new Vector3Int(x, y, 0));
             //   yield return new WaitForSeconds(0.01f);
                if (chunk.HasTile(blockInLocal))
                {
                    
                    blockType = tileMapManager.BlockNameGet(blockInLocal, chunk);
                    if (blockType == "Ore")
                        targetedOre.Add(new Vector3Int(x, y, 0));
                }
            }
        }
        targetedOre = targetedOre.OrderBy(x => Vector3Int.Distance(Vector3Int.FloorToInt(transform.position), x)).ToList();
        targetedOre.Reverse();
        minions.Add(Instantiate(minion, new Vector3(transform.position.x , transform.position.y), Quaternion.identity));
        
    }

    private Tilemap GetCurrentChunk(float positionX)
    {
        Tilemap chunk = chunkList[Mathf.FloorToInt(positionX / width)];
        return chunk;
    }


    private void MinionDropOff(GameObject minion)
    {

        targetedMinion = minion.GetComponent<FrogBehaviour>();
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
            Instantiate(minion, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
            oreAmount = 0;
        }
        if (blockAmount >= 500)
        {
            Instantiate(minion, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
            blockAmount = 0;
        }
    }
}
