using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct AABBComponent : IComponentData
{
    //AABB box;
    public float3 min;
    public float3 max;
}