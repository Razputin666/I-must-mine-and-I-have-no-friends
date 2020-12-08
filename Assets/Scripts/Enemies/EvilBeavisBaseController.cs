using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EvilBeavisBaseController : MonoBehaviour
{

    [HideInInspector] public List<Vector3Int> targetedOre = new List<Vector3Int>();
    [SerializeField] private List<Tilemap> chunkList;
    [SerializeField] private Tilemap currentChunk;
    [SerializeField] private TileMapManager tileMapManager;
    private LevelGeneratorLayered mapGen;
    private int width;
    private int height;
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



    private IEnumerator SearchForOres()
    {
        string blockType;
        for (int x = (int)transform.position.x; x > (int)transform.position.x - 35 && x >= 0; x--)
        {
            Tilemap chunk = GetCurrentChunk(x);
            yield return new WaitForSeconds(0.01f);
            
            for (int y = (int)transform.position.y; y > 0; y--)
            {
                
                Vector3Int blockInLocal = chunk.WorldToCell(new Vector3Int(x, y, 0));
                yield return new WaitForSeconds(0.01f);
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
        for (int x = (int)transform.position.x; x < (int)transform.position.x + 35 && x < (chunkList.Count * 250); x++)
        {
          
            Tilemap chunk = GetCurrentChunk(x);
            yield return new WaitForSeconds(0.01f);

            for (int y = (int)transform.position.y; y > 0; y--)
            {
                Vector3Int blockInLocal = chunk.WorldToCell(new Vector3Int(x, y, 0));
                yield return new WaitForSeconds(0.01f);
                if (chunk.HasTile(blockInLocal))
                {
                    
                    blockType = tileMapManager.BlockNameGet(blockInLocal, chunk);
                    if (blockType == "Ore")
                        targetedOre.Add(new Vector3Int(x, y, 0));
                }
            }
        }
    }

    private Tilemap GetCurrentChunk(float positionX)
    {
        Tilemap chunk = chunkList[Mathf.FloorToInt(positionX / width)];
        return chunk;
    }
}
