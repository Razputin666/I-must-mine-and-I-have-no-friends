using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Tilemaps;
public class FakeCasting : MonoBehaviour
{


   // private NativeMultiHashMap<int, TileRay> rayValueMap;
    private int prevLightAmount;
    //private JobHandle fakeHandle;


    public const float circle = 360f;

    //Skjuter rays i en cirkel och får tillbaka en struct med position och hur många gångar den colliderat
    public static NativeMultiHashMap<int, TileRay> TileMapRayCast(int range, int2 position, int rayAmount)
    {
        NativeMultiHashMap<int, TileRay> rayValueMap = new NativeMultiHashMap<int, TileRay>(range * range * rayAmount, Allocator.TempJob);

        TileMapRayCastGeneration fakeGen = new TileMapRayCastGeneration
        {
            position = position,
            range = range,
            rayAmount = rayAmount,
            worldArray = TileMapManager.Instance.nativeWorldArray,
            worldHeight = Worldgeneration.Instance.GetWorldHeight,
            rayValueMap = rayValueMap.AsParallelWriter()
        };

        JobHandle fakeHandle = fakeGen.Schedule(range * rayAmount, range);
        fakeHandle.Complete();

        return rayValueMap; // De som kallar på funktionen är responsible för att disposa värdet senare.
    }
}

[BurstCompile] 
public struct TileMapRayCastGeneration : IJobParallelFor
{
    public int2 position;
    public int range;
    public int rayAmount;
    [ReadOnly] public NativeArray<int> worldArray;
    public int worldHeight;
    public NativeMultiHashMap<int, TileRay>.ParallelWriter rayValueMap; //X, Y = Position av ljuskälla, Z = range och W = timesBlocked

    public void Execute(int index)
    {
        float2 angleVector = GetVectorFromAngle(index * (360f / (range * rayAmount)));
        float2 currentPos = position;
        int timesBlocked = 0;

        for (int i = 0; i < range; i++)
        {
            currentPos += angleVector;

            if ((worldArray[Mathf.RoundToInt(currentPos.x) * worldHeight + Mathf.RoundToInt(currentPos.y)] >= 1) && !((i + 1) >= range))
            {
                timesBlocked++;
            }
            rayValueMap.Add(Mathf.RoundToInt(currentPos.x) * worldHeight + Mathf.RoundToInt(currentPos.y), 
                new TileRay(position, new int2(Mathf.RoundToInt(currentPos.x), Mathf.RoundToInt(currentPos.y)), timesBlocked, range));

            //if (timesBlocked > rayPierce)
            //    break;
        }
    }
    private float2 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new float2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
}


public struct TileRay
{
    public int2 origin;
    public int2 position;
    public int collisions;
    public int range;

    public TileRay(int2 origin, int2 position, int collisions, int range)
    {
        this.origin = origin;
        this.position = position;
        this.collisions = collisions;
        this.range = range;
    }
}
