using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using System;

public class ShadowCastingJobs : MonoBehaviour
{
    [SerializeField] private int range;
    [SerializeField, Range(0, 1)] private float lightStr;

    #region First Job
    private NativeList<JobHandle> lightquadList;
    private QuadrantJob quadrant;

    private NativeList<float3>[] visibleTiles;
    private NativeArray<float3>[] previousTiles;

    #endregion
    private NativeQueue<float3> shadowQueue;

    private bool hasUpdatedLight;
    // private NativeList<float>[] visibleAmount;

    // Start is called before the first frame update
    void Start()
    {
        visibleTiles = new NativeList<float3>[4];
        visibleTiles[0] = new NativeList<float3>(Allocator.Persistent);
        visibleTiles[1] = new NativeList<float3>(Allocator.Persistent);
        visibleTiles[2] = new NativeList<float3>(Allocator.Persistent);
        visibleTiles[3] = new NativeList<float3>(Allocator.Persistent);

        visibleTiles[0].Add(new float3(0, 0, 0));
        visibleTiles[1].Add(new float3(0, 0, 0));
        visibleTiles[2].Add(new float3(0, 0, 0));
        visibleTiles[3].Add(new float3(0, 0, 0));
    }
    private void OnEnable()
    {
        TilemapSyncer.OnTileMapUpdated += TilemapSyncer_OnTileMapUpdated;
    }
    private void OnDisable()
    {
        TilemapSyncer.OnTileMapUpdated -= TilemapSyncer_OnTileMapUpdated;
    }

    private void TilemapSyncer_OnTileMapUpdated(object sender, Vector3 updatedTile)
    {
        if (Vector3.Distance(transform.position, updatedTile) < range)
        {
            ComputeLight();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.hasChanged)
        {
            ComputeLight();
            transform.hasChanged = false;
        }
    }
    private void LateUpdate()
    {
        //if (hasUpdatedLight)
        //{
        //    StartCoroutine(LateComputeLight());
        //    hasUpdatedLight = false;
        //}
    }
    private void ComputeLight()
    {
        for (int i = 0; i < 4; i++)
        {
            //for (int j = 0; j < visibleTiles[i].Length; j++)
            //{
            //    TileMapManager.Instance.shadowArray[Mathf.FloorToInt(visibleTiles[i][j].x) * Worldgeneration.Instance.GetWorldHeight + Mathf.FloorToInt(visibleTiles[i][j].y)] -= visibleTiles[i][j].z;
            //}
            visibleTiles[i].Clear();
        }

        lightquadList = new NativeList<JobHandle>(Allocator.TempJob);

        for (int i = 0; i < 4; i++)
        {
            //visibleTiles[i] = new NativeList<float3>(Allocator.Persistent);
            quadrant = new QuadrantJob(new int2(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)), i);

            JobHandle lightHandle = ComputeLightJob(visibleTiles[i]);
            lightquadList.Add(lightHandle);
        }
        StartCoroutine(LateComputeLight());
    }
    private IEnumerator LateComputeLight()
    {
        JobHandle.CompleteAll(lightquadList);
        lightquadList.Dispose();

        yield return null;

        //for (int i = 0; i < 4; i++)
        //{
        //    for (int j = 0; j < visibleTiles[i].Length; j++)
        //    {
        //        TileMapManager.Instance.shadowArray[Mathf.FloorToInt(visibleTiles[i][j].x) * Worldgeneration.Instance.GetWorldHeight + Mathf.FloorToInt(visibleTiles[i][j].y)] += visibleTiles[i][j].z;
        //    }
        //}

        //NativeList<JobHandle> setLightTiles = new NativeList<JobHandle>(Allocator.TempJob);
        //shadowQueue = new NativeQueue<float3>(Allocator.TempJob);
        //for (int i = 0; i < 4; i++)
        //{
        //    JobHandle setLightJob = SetLitTiles(visibleTiles[i]);
        //    setLightTiles.Add(setLightJob);
        //}
        //yield return null;
        //JobHandle.CompleteAll(setLightTiles);

    }
    public JobHandle ComputeLightJob(NativeList<float3> visibleTileList)
    {
        ShadowGeneration shadowGen = new ShadowGeneration
        {
            row = new RowJob(1, -1, 1),
            quadrant = this.quadrant,
            worldArray = TileMapManager.Instance.nativeWorldArray,
            range = this.range,
            worldHeight = Worldgeneration.Instance.GetWorldHeight,
            visibleTiles = visibleTileList,
            lightStr = this.lightStr
        };
        return shadowGen.Schedule();
    }
    private JobHandle SetLitTiles(NativeList<float3> visibleTiles)
    {

        SetLightJob setLight = new SetLightJob
        {
            visibleTiles = visibleTiles,
            shadowQueue = shadowQueue.AsParallelWriter(),
            shadowArray = TileMapManager.Instance.shadowArray,
            worldHeight = Worldgeneration.Instance.GetWorldHeight

        };
        return setLight.Schedule();
    }

}
#region ShadowGeneration
[BurstCompile]
public struct ShadowGeneration : IJob
{
    [ReadOnly] public NativeArray<int> worldArray;
    public int range;
    public float lightStr;
    public int worldHeight;
    public RowJob row;
    public QuadrantJob quadrant;
    public NativeList<float3> visibleTiles;


    public void Execute()
    {
        NativeList<RowJob> rows = new NativeList<RowJob>(0, Allocator.Temp);
        rows.Add(row);

        while (rows.Length >= 1 && rows[rows.Length - 1].depth < range)
        {
            row = rows[rows.Length - 1];
            rows.RemoveAt(rows.Length - 1);

            int2 prevTile = new int2(-100, -100);
            NativeArray<int2> tiles = row.Tiles(row);

            for (int i = 0; i < tiles.Length; i++)
            {
                if (IsWall(tiles[i]) || IsSymmetric(row, tiles[i]))
                {

                    float2 distance = tiles[i] - quadrant.source;
                    float diminish = math.clamp(math.sqrt(distance.x * distance.x + distance.y * distance.y), 0, range * lightStr) / range;
                    int2 tile = quadrant.QuadTransform(tiles[i]);

                    visibleTiles.Add(new float3(tile.x, tile.y, diminish));
                }
                if (IsWall(prevTile) && IsFloor(tiles[i]))
                {
                    row.startSlope = Slope(tiles[i]);
                }
                if (IsFloor(prevTile) && IsWall(tiles[i]))
                {
                    RowJob nextRow = row.Next();
                    nextRow.endSlope = Slope(tiles[i]);
                    rows.Add(nextRow);
                }
                prevTile = tiles[i];
            }
            if (IsFloor(prevTile))
            {
                rows.Add(row.Next());
            }
        }
    }

    private bool IsSymmetric(RowJob row, int2 tile)
    {
        return tile.y >= row.depth * row.startSlope && tile.y <= row.depth * row.endSlope;
    }
    private bool IsWall(int2 tile)
    {
        if (tile.Equals(new int2(-100, -100)))
        {
            return false;
        }
        int2 tileToCheck = quadrant.QuadTransform(tile);
        return worldArray[tileToCheck.x * worldHeight + tileToCheck.y] >= 1;

    }
    private bool IsFloor(int2 tile)
    {
        if (tile.Equals(new int2(-100, -100)))
        {
            return false;
        }
        int2 tileToCheck = quadrant.QuadTransform(tile);
        return !(worldArray[tileToCheck.x * worldHeight + tileToCheck.y] >= 1);

    }
    private float Slope(int2 tile)
    {
        return (2f * tile.y - 1f) / (2f * tile.x);
    }


}

public struct QuadrantJob
{
    public int2 source;
    public int direction;

    public QuadrantJob(int2 source, int direction)
    {
        this.source = source;
        this.direction = direction;
    }

    public int2 QuadTransform(int2 tile)
    {
        switch (direction)
        {
            case 0:
                return new int2(source.x + tile.y, source.y - tile.x);
            case 1:
                return new int2(source.x + tile.y, source.y + tile.x);
            case 2:
                return new int2(source.x + tile.x, source.y + tile.y);
            case 3:
                return new int2(source.x - tile.x, source.y + tile.y);
            default:
                return tile;
        }
    }
}
#endregion

[BurstCompile]
public struct SetLightJob : IJob
{
    public NativeList<float3> visibleTiles;
    public NativeQueue<float3>.ParallelWriter shadowQueue;
    [ReadOnly] public NativeArray<float3> shadowArray;

    public int worldHeight;
    public void Execute()
    {
        for (int i = 0; i < visibleTiles.Length; i++)
        {
            //shadowArray[Mathf.FloorToInt(visibleTiles[i].x) * worldHeight + Mathf.FloorToInt(visibleTiles[i].y)] += visibleTiles[i].z;
        }
    }
}