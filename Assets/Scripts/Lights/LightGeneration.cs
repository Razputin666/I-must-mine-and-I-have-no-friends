using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Tilemaps;
using Mirror;

public class LightGeneration : NetworkBehaviour
{
    [SerializeField] private int range;
    [SerializeField, Range(1, 3)] private int rayAmount;
    [SerializeField, Range(0, 0.2f)] private float lightDiminish = 0.07f;

    private int prevRay;
    private float prevDiminish;
    //[SerializeField] private int rayPierce;
    //[SerializeField, Range(0, 1f)] private float lightStr;
    

    private JobHandle lightHandle;
    private JobHandle prevLightHandle;
    private NativeHashMap<int, PrevLight> prevLightMap;

    public static event EventHandler<Vector2Int> OnlightUpdated;

    private void Update()
    {
        if (prevRay != rayAmount || prevDiminish != lightDiminish)
            LightCreation();

        if (transform.hasChanged)
        {
            LightCreation();
            transform.hasChanged = false;
        }
        prevRay = rayAmount;
        prevDiminish = lightDiminish;
    }
    private void TilemapSyncer_OnTileMapUpdated(object sender, Vector3 updatedTile)
    {
        if (Vector3.Distance(transform.position, updatedTile) < range)
        {
            LightCreation();
        }
    }
    private void OnEnable()
    {
        TilemapSyncer.OnTileMapUpdated += TilemapSyncer_OnTileMapUpdated;
        prevLightMap = new NativeHashMap<int, PrevLight>(0, Allocator.TempJob);
    }
    private void OnDisable()
    {
        TilemapSyncer.OnTileMapUpdated -= TilemapSyncer_OnTileMapUpdated;
    }
    private void OnApplicationQuit()
    {
        prevLightMap.Dispose();
    }
    private void LightCreation()
    {
        PrevLightRemoval();
        NativeMultiHashMap<int, TileRay> tileMapRayCast = FakeCasting.TileMapRayCast(range, new int2(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)), rayAmount * 8);
        LightGenHandle(tileMapRayCast);
    }
    private void PrevLightRemoval()
    {
        TestJob3 lightRemoval = new TestJob3
        {
            shadowArray = TileMapManager.Instance.shadowArray,
            worldHeight = Worldgeneration.Instance.GetWorldHeight,
            prevLightMap = prevLightMap
        };
        prevLightHandle = lightRemoval.Schedule(prevLightMap.Capacity, 50);
        prevLightHandle.Complete();
        prevLightMap.Dispose();

    }
    private void LightGenHandle(NativeMultiHashMap<int, TileRay> tileMapRayCast)
    {
        (NativeArray<int>, int) uniqueKeyTuple = NativeHashMapExtensions.GetUniqueKeyArray(tileMapRayCast, Allocator.TempJob);
        NativeArray<int> rayMapKeys = uniqueKeyTuple.Item1;

        prevLightMap = new NativeHashMap<int, PrevLight>(uniqueKeyTuple.Item2 + 1, Allocator.Persistent);

        TestJob lightJob = new TestJob
        {
            tileMapRayCast = tileMapRayCast,
            worldHeight = Worldgeneration.Instance.GetWorldHeight,
            rayMapKeys = rayMapKeys,
            shadowArray = TileMapManager.Instance.shadowArray,
            lightDiminish = lightDiminish,
            prevLightMap = prevLightMap.AsParallelWriter()
        };
        lightHandle = lightJob.Schedule(uniqueKeyTuple.Item2, 100);
        lightHandle.Complete();

        tileMapRayCast.Dispose();
        rayMapKeys.Dispose();
        OnlightUpdated?.Invoke(this, Vector2Int.FloorToInt(transform.position));
    }

}
[BurstCompile]
public struct TestJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NativeMultiHashMap<int, TileRay> tileMapRayCast;
    public NativeHashMap<int, PrevLight>.ParallelWriter prevLightMap;
    public int worldHeight;
    public float lightDiminish;
    [ReadOnly] public NativeArray<int> rayMapKeys;
    [NativeDisableParallelForRestriction] public NativeArray<float3> shadowArray;

    public void Execute(int index)
    {

        NativeMultiHashMap<int, TileRay>.Enumerator tileValues = tileMapRayCast.GetValuesForKey(rayMapKeys[index]);

        tileValues.MoveNext();

        int2 origin = tileValues.Current.origin;
        int2 position = tileValues.Current.position;
        int range = tileValues.Current.range;
        int collisions = tileValues.Current.collisions;

        while (tileValues.MoveNext())
        {
            if (collisions < tileValues.Current.collisions)
                collisions = tileValues.Current.collisions;
        }
        float light = Mathf.Clamp(1f - ((Length(position - origin)) / range) - (lightDiminish * collisions), 0, 1f);


        shadowArray[position.x * worldHeight + position.y] += light;
        // shadowArray[prevLightMap[index].position.x * worldHeight + prevLightMap[index].position.y] -= prevLightMap[index].light;
        //prevLightMap.TryAdd(position.x * worldHeight + position.y, new PrevLight(position, light));
        prevLightMap.TryAdd(index + 1, new PrevLight(position, light));

        if (index == 0)
        {
            shadowArray[origin.x * worldHeight + origin.y] += 1f;
            prevLightMap.TryAdd(0, new PrevLight(origin, 1f));
        }

    }
    private float Length(float2 v)
    {
        return Mathf.Sqrt(v.x * v.x + v.y * v.y);
    }
}
[BurstCompile]
public struct TestJob3 : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NativeHashMap<int, PrevLight> prevLightMap;
    public int worldHeight;
    [NativeDisableParallelForRestriction] public NativeArray<float3> shadowArray;
    public void Execute(int index)
    {
        shadowArray[prevLightMap[index].position.x * worldHeight + prevLightMap[index].position.y] -= prevLightMap[index].light;
        // shadowArray[prevLightMap[index].position.x * worldHeight + prevLightMap[index].position.y] = 0;
    }
}
public struct PrevLight
{
    public int2 position;
    public float light;

    public PrevLight(int2 position, float light)
    {
        this.position = position;
        this.light = light;
    }
}