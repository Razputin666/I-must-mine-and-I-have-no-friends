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
    [SerializeField] protected bool useJobs;
    #endregion
    #region Constantvalues
    protected const int height = 50;
    protected const int width = 50;
    #endregion
    protected Vector2Int startPosition;
    [HideInInspector] public List<Tilemap> chunks = new List<Tilemap>();
    protected TileBase[] blocks;
    protected Dictionary<string, TileBase> tilebaseLookup;

    // Start is called before the first frame update
    //public override void OnStartServer()
    //{
    //    //tilebaseLookup = new Dictionary<string, TileBase>();
    //    //blocks = Resources.LoadAll<TileBase>("Tilebase");
    //    //foreach (var tilebase in blocks)
    //    //{
    //    //    tilebaseLookup.Add(tilebase.name, tilebase);
    //    //}
    //    Init();

    //    if (useJobs)
    //    {
    //        Stopwatch stopwatchJobs = new Stopwatch();
    //        stopwatchJobs.Start();
    //        GenerateChunks();
    //        stopwatchJobs.Stop();
    //        System.TimeSpan timeTaken = stopwatchJobs.Elapsed;
    //        UnityEngine.Debug.Log("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
    //    }
    //    else
    //    {
    //        Stopwatch stopwatch = new Stopwatch();
    //        stopwatch.Start();
    //        GenerateChunksNoJobs();
    //        stopwatch.Stop();
    //        System.TimeSpan timeTaken = stopwatch.Elapsed;
    //        UnityEngine.Debug.Log("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
    //    }

    //}

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

    //protected virtual void GenerateJobs(NativeArray<int>[] chunkArray)
    //{
    //  ////  NativeArray<int>[] chunkArray = new NativeArray<int>[horizontalChunks * verticalChunks];
    //  //  for (int i = 0; i < chunkArray.Length; i++)
    //  //  {
    //  //      chunkArray[i] = new NativeArray<int>(width * height, Allocator.TempJob);
    //  //  }
    //    NativeList<JobHandle> jobsHandleList = new NativeList<JobHandle>(Allocator.Temp);

    //    for (int i = 0; i < verticalChunks * horizontalChunks; i++)
    //    {
    //        JobsGenerate generateArray = new JobsGenerate
    //        {
    //            width = width, 
    //            height = height,
    //            map = chunkArray[i]
    //        };
    //        JobHandle jobHandle = generateArray.Schedule();
    //        jobsHandleList.Add(jobHandle);
    //    }


    //    JobHandle.CompleteAll(jobsHandleList);
    //    jobsHandleList.Dispose();
    //    //for (int i = 0; i < chunkArray.Length; i++)
    //    //{
    //    //    RenderMapJobs(chunkArray[i], TileMapManager.Instance.GetTileChunk(i), tilebaseLookup["Dirt Block"]);
    //    //    chunkArray[i].Dispose();
    //    //}
    //    UnityEngine.Debug.Log("GenerateChunk");
    //}

    //protected virtual void GenerateJobs(NativeArray<int> chunkArray)
    //{
    //    NativeList<JobHandle> jobsHandleList = new NativeList<JobHandle>(Allocator.Temp);

    //    for (int i = 0; i < verticalChunks * horizontalChunks; i++)
    //    {
    //        JobsGenerate generateArray = new JobsGenerate
    //        {
    //            width = width,
    //            height = height,
    //            map = chunkArray
    //        };
    //        JobHandle jobHandle = generateArray.Schedule(chunkArray.Length, 100);
    //        jobsHandleList.Add(jobHandle);
    //    }
    //    JobHandle.CompleteAll(jobsHandleList);
    //    jobsHandleList.Dispose();
    //}

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
            RenderMapJobs(chunkArray[i], TileMapManager.Instance.GetTileChunk(i), tilebaseLookup["Dirt Block"]);
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
    public static void RenderMapJobs(NativeArray<int> map, Tilemap chunk, TileBase block)
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
                if (map[index] == 1) // 1 = tile, 0 = no tile
                {
                    // tilemap.SetTile(new Vector3Int(x, y, 0), block);
                    tileArray[index] = block;
                }
                else
                {
                    // tilemap.SetTile(new Vector3Int(x, y, 0), null);
                    tileArray[index] = null;
                }
            }
        }
        chunk.SetTiles(tilepositions, tileArray);
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
