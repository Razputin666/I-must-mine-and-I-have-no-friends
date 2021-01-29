using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;

public class Worldgeneration : NetworkBehaviour
{
    #region Serializefields
    [SerializeField] protected GameObject grid;
    [SerializeField] protected int horizontalChunks;
    [SerializeField] protected int verticalChunks;
    [SerializeField] protected GameObject chunkPrefab;
    [SerializeField] protected MapSettings defaultChunk;
    [Range(100, 1)]
    [SerializeField] protected int oreAmount;
    [SerializeField] protected bool useJobs;
    
    #endregion
    #region Constantvalues
    public const int height = 50;
    public const int width = 50;
    protected const int copperModifier = 3000;
    protected const int ironModifier = 1800;
    protected const int coalModifier = 3300;
    protected const int goldModifier = 1200;
    #endregion
    protected Vector2Int startPosition;
    [HideInInspector] public List<Tilemap> chunks = new List<Tilemap>();
    [HideInInspector] public TileBase[] blocks;
    [HideInInspector] public Dictionary<string, TileBase> tilebaseLookup;
    private BlockTypeConversion blockTypeConversion;


    protected virtual void Init()
    {
        tilebaseLookup = new Dictionary<string, TileBase>();
        blocks = Resources.LoadAll<TileBase>("Tilebase");
        foreach (var tilebase in blocks)
        {
            tilebaseLookup.Add(tilebase.name, tilebase);
        }

        for (int x = 0; x < horizontalChunks; x++)
        {
            startPosition.y = 0;
            for (int y = 0; y < verticalChunks; y++)
            {
                int index = x * verticalChunks + y;
                GameObject chunk = Instantiate(chunkPrefab, grid.transform);
                chunk.name = "Chunk_" + index;
                TileMapManager.Instance.AddTileChunk(chunk.GetComponent<Tilemap>());
                chunk.transform.position = new Vector2(startPosition.x, startPosition.y);
                startPosition.y += height;
            }
            startPosition.x += width;
        }
        UnityEngine.Debug.Log("Init");
    }

    protected virtual void GenerateJobs(NativeArray<int> chunkArray)
    {

        JobsGenerate generateArray = new JobsGenerate
        {
            width = width,
            height = height,
            map = chunkArray
        };
        JobHandle jobHandle = generateArray.Schedule(chunkArray.Length, 100);
        jobHandle.Complete();
    }


    protected virtual void Render(NativeArray<int>[] chunkArray)
    {
        //Loop through all chunks and render on the Tilemap
        
        for (int i = 0; i < chunkArray.Length; i++)
        {
            RenderMapJobs(chunkArray[i], TileMapManager.Instance.GetTileChunk(i));
        }
    }
    #region NotJobs
    protected void GenerateChunksNoJobs()
    {
        startPosition = new Vector2Int(0, (-height / 2));
        for (int x = 0; x < horizontalChunks; x++)
        {
            startPosition.y = 0;
            for (int y = 0; y < verticalChunks; y++)
            {
                int index = y * horizontalChunks + x;
                GameObject chunk = Instantiate(chunkPrefab, grid.transform);
                chunk.name = "Chunk_" + index;
                TileMapManager.Instance.AddTileChunk(chunk.GetComponent<Tilemap>());
                chunk.transform.position = new Vector2(startPosition.x, startPosition.y);
                GenerateMainMap(chunk.GetComponent<Tilemap>());
                startPosition.y += height;
            }
            startPosition.x += width;
        }
    }

    private void GenerateMainMap(Tilemap chunk)
    {
        int[,] mapTemp = new int[width, height];

        mapTemp = GenerateArray();
        RenderMap(mapTemp, chunk, tilebaseLookup["Dirt Block"]);
    }

    public static int[,] GenerateArray()
    {

        int[,] mapTemp = new int[width, height];
        for (int x = 0; x < mapTemp.GetUpperBound(0); x++)
        {
            for (int y = 0; y < mapTemp.GetUpperBound(1); y++)
            {

                mapTemp[x, y] = 1;

            }
        }
        return mapTemp;
    }

    public static void RenderMap(int[,] map, Tilemap tilemap, TileBase block)
    {
        tilemap.ClearAllTiles(); //Clear the map (ensures we dont overlap)
        for (int x = 0; x < map.GetUpperBound(0); x++) //Loop through the width of the map
        {
            for (int y = map.GetUpperBound(1); y >= 0; y--) //Loop through the height of the map
            {
                if (map[x, y] == 1) // 1 = tile, 0 = no tile
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), block);
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
    }
    #endregion
    public void RenderMapJobs(NativeArray<int> map, Tilemap chunk)
    {
        chunk.ClearAllTiles(); //Clear the map (ensures we dont overlap)
        Vector3Int[] tilepositions = new Vector3Int[width * height];
        TileBase[] tileArray = new TileBase[width * height];
        for (int x = 0; x < width; x++) //Loop through the width of the map
        {
            for (int y = 0; y < height; y++) //Loop through the height of the map
            {
                int index = x * height + y;
                tilepositions[index] = new Vector3Int(x, y, 0);
                if (map[index] >= 1) // 1+ = tile, 0 = no tile
                {
                    tileArray[index] = tilebaseLookup[BlockType(map[index]) + " Block"];
                }
                else
                {
                    tileArray[index] = null;
                }
            }
        }
        chunk.SetTiles(tilepositions, tileArray);
    }
    protected virtual void RemoveLoneBlocks(Tilemap chunk)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height + (height / 2); y++)
            {

                if (chunk.HasTile(new Vector3Int(x, y, 0)) && !chunk.HasTile(new Vector3Int(x + 1, y, 0)) && !chunk.HasTile(new Vector3Int(x - 1, y, 0)) && !chunk.HasTile(new Vector3Int(x, y + 1, 0)) && !chunk.HasTile(new Vector3Int(x, y - 1, 0)))
                {
                    chunk.SetTile(new Vector3Int(x, y, 0), null);
                }
            }

        }
    }

    private string BlockType(int block)
    {
        switch (blockTypeConversion = (BlockTypeConversion)block)
        {
            case BlockTypeConversion.Empty:
                return "";
            case BlockTypeConversion.Dirt:
                return BlockTypeConversion.Dirt.ToString();
            case BlockTypeConversion.Stone:
                return BlockTypeConversion.Stone.ToString();
            case BlockTypeConversion.Copper:
                return BlockTypeConversion.Copper.ToString();
            case BlockTypeConversion.Iron:
                return BlockTypeConversion.Iron.ToString();
            case BlockTypeConversion.Coal:
                return BlockTypeConversion.Coal.ToString();
            case BlockTypeConversion.Gold:
                return BlockTypeConversion.Gold.ToString();
            default:
                return "";
        }
    }

}

public struct JobsGenerate : IJobParallelFor
{
    public NativeArray<int> map;
    public int width;
    public int height;
    public void Execute(int index)
    {
        map[index] = 1;
    }
}

public struct JobsTopTerrain : IJobParallelFor
{
    public void Execute(int index)
    {
        throw new System.NotImplementedException();
    }
}

public enum BlockTypeConversion
{
    Empty, 
    Dirt,
    Stone,
    Copper,
    Iron,
    Coal,
    Gold,


}