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
    // Start is called before the first frame update
    void Start()
    {
        tileMapManager = GameObject.FindWithTag("GameManager").GetComponent<TileMapManager>();
        chunkList = GameObject.Find("LevelGeneration").GetComponent<LevelGeneratorLayered>().chunks;
        currentChunk = GetCurrentChunk(transform.position.x);
        StartCoroutine(SearchForOres());

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(targetedOre.Count);
    }

    private IEnumerator SearchForOres()
    {
        string blockType;
        for (int x = (int)transform.position.x; x > (int)transform.position.x - 35; x--)
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
                    targetedOre.Add(new Vector3Int(x, y, 0));
                }
            }
        }
        for (int x = (int)transform.position.x; x < (int)transform.position.x + 35; x++)
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
        float chunkPos = positionX * (chunkList.Count) / 10;
        Tilemap chunk = chunkList[Mathf.FloorToInt(chunkPos / 100)];
        return chunk;
    }
}
