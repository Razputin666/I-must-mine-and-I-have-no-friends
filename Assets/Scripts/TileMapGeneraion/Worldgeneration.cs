using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Worldgeneration : MonoBehaviour
{
    #region Serializefields
    [SerializeField] private GameObject grid;
    [SerializeField] private int horizontalChunks;
    [SerializeField] private int verticalChunks;
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private MapSettings defaultChunk;
    [SerializeField] private BlockTypes blockTypes;
    #endregion
    #region Constantvalues
    private const int height = 50;
    private const int width = 50;
    #endregion
    private Vector2Int startPosition;
    [HideInInspector] public List<Tilemap> chunks = new List<Tilemap>();
    // Start is called before the first frame update
    void Start()
    {
        GenerateChunks();
    }

    private void GenerateChunks()
    {
        startPosition = new Vector2Int(0, (-height / 2));

        for (int i = 0; i < horizontalChunks; i++)
        {
            startPosition.y = 0;
            for (int j = 0; j < verticalChunks; j++)
            {
                GameObject chunk = Instantiate(chunkPrefab, grid.transform);
               // chunks.Add(chunk.GetComponent<Tilemap>());
                chunk.transform.position = new Vector2(startPosition.x, startPosition.y);
                GenerateMainMap(chunk.GetComponent<Tilemap>());
                startPosition.y += height;
            }
            startPosition.x += width;
        }
    }

	public void GenerateMainMap(Tilemap chunk)
	{
		int[,] map = new int[width, height];    
        map = GenerateArray();
        RenderMap(map, chunk, blockTypes.GetBlockType("Dirt"));
	}
    public static int[,] GenerateArray()
    {

        int[,] map = new int[width, height];
        for (int x = 0; x < map.GetUpperBound(0); x++)
        {
            for (int y = 0; y < map.GetUpperBound(1); y++)
            {

                    map[x, y] = 1;
                
            }
        }
        return map;
    }
    public static void RenderMap(int[,] map, Tilemap tilemap, TileBase block)
    {
        tilemap.ClearAllTiles(); //Clear the map (ensures we dont overlap)
        for (int x = 0; x < map.GetUpperBound(0); x++) //Loop through the width of the map
        {
            bool isGround = true;
            for (int y = map.GetUpperBound(1); y >= 0; y--) //Loop through the height of the map
            {
                if (map[x, y] == 1) // 1 = tile, 0 = no tile
                {
                    //  Debug.Log(x + "," + y);
                    //if we are at ground level set the tile to Grass else set it to Dirt
                    if (isGround)
                    {
                        isGround = false;
                        tilemap.SetTile(new Vector3Int(x, y, 0), block);
                    }
                    else
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), block);
                    }
                }
            }
        }
    }
}
