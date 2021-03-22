using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public enum Directions
{
    N,
    NE,
    E,
    SE,
    S,
    SW,
    W,
    NW

}

public class ShadowCastingEightPath : MonoBehaviour
{
    [SerializeField] private int range;
    [SerializeField, Range(0, 3)] private int lightStr;

    private NativeQueue<LightData> visibleTiles;
    private NativeArray<Octagon> octagons;
    private NativeMultiHashMap<int, LightData> someHashBrownieThing;
    private JobHandle shadowHandle;
    private NativeArray<LightData> lightArray;
    [HideInInspector] public static event EventHandler<Vector2Int> OnlightUpdated;

    private bool isBruh;

    // Start is called before the first frame update
    void Start()
    {
            octagons = new NativeArray<Octagon>(8, Allocator.Persistent);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBruh && transform.hasChanged)
        {
            StartBruh();
            transform.hasChanged = false;
        }
    }

    private void OnEnable()
    {
        TilemapSyncer.OnTileMapUpdated += TilemapSyncer_OnTileMapUpdated;
    }
    private void OnDisable()
    {
        TilemapSyncer.OnTileMapUpdated -= TilemapSyncer_OnTileMapUpdated;
        octagons.Dispose();
    }

    private void TilemapSyncer_OnTileMapUpdated(object sender, Vector3 updatedTile)
    {
        if (Vector3.Distance(transform.position, updatedTile) < range)
        {
            StartBruh();
        }
    }
    private void StartBruh()
    {
        StopCoroutine(bruh());

        for (int i = 0; i < lightArray.Length; i++)
        {
            TileMapManager.Instance.shadowArray[lightArray[i].position.x * Worldgeneration.Instance.GetWorldHeight + lightArray[i].position.y] -= lightArray[i].light;
        }

        if (visibleTiles.IsCreated)
            visibleTiles.Dispose();
            

        if (lightArray.IsCreated)
            lightArray.Dispose();

        StartCoroutine(bruh());
    }

    private IEnumerator bruh()
    {
        isBruh = true;

        visibleTiles = new NativeQueue<LightData>(Allocator.TempJob);

        for (int i = 0; i < octagons.Length; i++)
        {
            octagons[i] = new Octagon(new int2(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)), i);
        }
        ShadowGenerationEight shadowGenJob = new ShadowGenerationEight
        {
            range = range,
            octagon = octagons,
            worldArray = TileMapManager.Instance.nativeWorldArray,
            worldHeight = Worldgeneration.Instance.GetWorldHeight,
            lightTileQueue = visibleTiles.AsParallelWriter(),
            light = lightStr
        };
        shadowHandle = shadowGenJob.Schedule(octagons.Length, 1);

        yield return null;

        shadowHandle.Complete();
        lightArray = visibleTiles.ToArray(Allocator.Persistent);
        //for (int i = 0; i < lightArray.Length; i++)
        //{
        //    if (lightArray[i].slopes > 0)
        //    {
        //        Debug.Log(lightArray[i].slopes + " slope");
        //    }
        //}

        yield return null;

        for (int i = 0; i < lightArray.Length; i++)
        {
            TileMapManager.Instance.shadowArray[lightArray[i].position.x * Worldgeneration.Instance.GetWorldHeight + lightArray[i].position.y] += lightArray[i].light;
            //lightData = visibleTiles.Dequeue();
            //TileMapManager.Instance.shadowArray[lightData.position.x * Worldgeneration.Instance.GetWorldHeight + lightData.position.y] += lightData.light;
        }
        OnlightUpdated?.Invoke(this, Vector2Int.FloorToInt(transform.position));


        //lightArray.Dispose();
        //visibleTiles.Dispose();
        isBruh = false;
    }
    //private JobHandle SlopeHandle(LightData lightdata)
    //{

    //    SlopeShadows slopeShadow = new SlopeShadows
    //    {
    //        lightData = lightdata,
    //        row = new RowJob(1, lightdata.slopes, lightdata.slopes),
    //        range = this.range,
    //        worldArray = TileMapManager.Instance.nativeWorldArray
    //    };
    //    return slopeShadow.Schedule();
    //}
   

}

[BurstCompile]
public struct ShadowGenerationEight : IJobParallelFor
{
    public int range;
    public NativeArray<Octagon> octagon;
    [ReadOnly] public NativeArray<int> worldArray;
    public int worldHeight;
    public NativeQueue<LightData>.ParallelWriter lightTileQueue;
    public float light;

    public void Execute(int index)
    {
        for (int depth = 1; depth < range + 1; depth++)
        {
            for (int column = 0; column < depth; column++)
            {
                int2 tile = new int2(depth, column);

                if (worldArray[octagon[index].Transform(tile).x * worldHeight + octagon[index].Transform(tile).y] >= 1)
                {
                    lightTileQueue.Enqueue(new LightData(octagon[index].Transform(tile), light - ((light / range) * depth), Slope(tile), tile.x));
                }
                else
                {
                    lightTileQueue.Enqueue(new LightData(octagon[index].Transform(tile), light - ((light / range) * depth), 0, tile.x));
                }
            }
        }
    }
    private float Slope(int2 tile)
    {
        return (2f * tile.y - 1f) / (2f * tile.x);
    }
}
//public struct SlopeShadows : IJob
//{
//    public LightData blockedTile;
//    public RowJob row;
//    [ReadOnly] public NativeArray<int> worldArray;
//    public int range;

//    public void Execute()
//    {
//        NativeList<RowJob> rows = new NativeList<RowJob>(0, Allocator.Temp);
//        rows.Add(row);

//        while (rows.Length >= 1 && rows[rows.Length - 1].depth < range - blockedTile.depth)
//        {
//            row = rows[rows.Length - 1];
//            rows.RemoveAt(rows.Length - 1);

//            int2 prevTile = new int2(-100, -100);
//            NativeArray<int2> tiles = row.Tiles(row);

//            for (int i = 0; i < tiles.Length; i++)
//            {
//                //if (IsWall(tiles[i]) || IsSymmetric(row, tiles[i]))
//                //{

//                //    float2 distance = tiles[i] - quadrant.source;
//                //    float diminish = math.clamp(math.sqrt(distance.x * distance.x + distance.y * distance.y), 0, range * lightStr) / range;
//                //    int2 tile = quadrant.QuadTransform(tiles[i]);

//                //    visibleTiles.Add(new float3(tile.x, tile.y, diminish));
//                //}
//                //if (IsWall(prevTile) && IsFloor(tiles[i]))
//                //{
//                //    row.startSlope = Slope(tiles[i]);
//                //}
//                //if (IsFloor(prevTile) && IsWall(tiles[i]))
//                //{
//                //    RowJob nextRow = row.Next();
//                //    nextRow.endSlope = Slope(tiles[i]);
//                //    rows.Add(nextRow);
//                //}
//                //prevTile = tiles[i];

//            }
//           // if (IsFloor(prevTile))
//            //{
//                rows.Add(row.Next());
//           // }
//        }
//    }
//    private bool IsSymmetric(RowJob row, int2 tile)
//    {
//        return tile.y >= row.depth * row.startSlope && tile.y <= row.depth * row.endSlope;
//    }
//    private bool IsWall(int2 tile)
//    {
//        if (tile.Equals(new int2(-100, -100)))
//        {
//            return false;
//        }
//        int2 tileToCheck = quadrant.QuadTransform(tile);
//        return worldArray[tileToCheck.x * worldHeight + tileToCheck.y] >= 1;

//    }
//    private bool IsFloor(int2 tile)
//    {
//        if (tile.Equals(new int2(-100, -100)))
//        {
//            return false;
//        }
//        int2 tileToCheck = quadrant.QuadTransform(tile);
//        return !(worldArray[tileToCheck.x * worldHeight + tileToCheck.y] >= 1);

//    }
//    private float Slope(int2 tile)
//    {
//        return (2f * tile.y - 1f) / (2f * tile.x);
//    }
//}
#region Structs
public struct LightData
{
    public int2 position;
    public float light;
    public float slopes;
    public int depth;

    public LightData(int2 position, float light, float slopes, int depth)
    {
        this.position = position;
        this.light = light;
        this.slopes = slopes;
        this.depth = depth;
        
    }

}

public struct Octagon
{
    public int2 source;
    public int direction;

    public Octagon(int2 source, int direction)
    {
        this.source = source;
        this.direction = direction;
    }

    public int2 Transform(int2 tile)
    {
        int2 newTile;
        int2 cornerTile;
        switch ((Directions)direction)
        {
            case Directions.N:
                return new int2(source.x + tile.y, source.y + tile.x);
            case Directions.NE:
                cornerTile = new int2(1, 1);
                newTile = new int2(source + (cornerTile * tile.x));
                return new int2(newTile.x, newTile.y - tile.y);
            case Directions.E:
                return new int2(source.x + tile.x, source.y - tile.y);
            case Directions.SE:
                 cornerTile = new int2(1, -1);
                newTile = new int2(source + (cornerTile * tile.x));
                return new int2(newTile.x - tile.y, newTile.y);
            case Directions.S:
                return new int2(source.x - tile.y, source.y - tile.x);
            case Directions.SW:
                cornerTile = new int2(-1, -1);
                newTile = new int2(source + (cornerTile * tile.x));
                return new int2(newTile.x, newTile.y + tile.y);
            case Directions.W:
                return new int2(source.x - tile.x, source.y + tile.y);
            case Directions.NW:
                cornerTile = new int2(-1, 1);
                newTile = new int2(source + (cornerTile * tile.x));
                return new int2(newTile.x + tile.y, newTile.y);
            default:
                return 0;
        }
    }
    //public float DistanceNormalize(float light, int range, int depth)
    //{
    //    switch ((Directions)direction)
    //    {
    //        case Directions.N:
    //            return (light - ((light / range) * depth)) * 1.04f;
    //        case Directions.NE:
    //            return light - ((light / range) * depth);
    //        case Directions.E:
    //            return (light - ((light / range) * depth)) * 1.04f;
    //        case Directions.SE:
    //            return light - ((light / range) * depth);
    //        case Directions.S:
    //            return (light - ((light / range) * depth)) * 1.04f;
    //        case Directions.SW:
    //            return light - ((light / range) * depth);
    //        case Directions.W:
    //            return (light - ((light / range) * depth)) * 1.04f;
    //        case Directions.NW:
    //            return light - ((light / range) * depth);
    //        default:
    //            return 0;
    //    }
    //}

}

public struct RowJob
{
    // RowJob LOL

    public float startSlope;
    public float endSlope;
    public int depth;


    public RowJob(int depth, float startSlope, float endSlope)
    {
        this.depth = depth;
        this.startSlope = startSlope;
        this.endSlope = endSlope;
    }
    public RowJob Next()
    {
        RowJob nextRow = new RowJob(depth + 1, startSlope, endSlope); 
        return nextRow;
    }
    public NativeArray<int2> Tiles(RowJob row)
    {
        int minColumn = Mathf.FloorToInt((row.depth * row.startSlope) + 0.5f);
        int maxColumn = Mathf.CeilToInt((row.depth * row.endSlope) - 0.5f);

        NativeArray<int2> tiles = new NativeArray<int2>(maxColumn + 1 - minColumn, Allocator.Temp);
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new int2(row.depth, minColumn + i);
        }
        return tiles;
    }
}
#endregion