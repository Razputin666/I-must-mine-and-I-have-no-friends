using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;


//[DisableAutoCreation] Flag so it doesnt gets auto created
//public class EntityTileCreationJob : SystemBase
//{
//    protected override void OnUpdate()
//    {
//        Debug.Log("Updating");
//        base.OnStartRunning();
//        float dt = Time.DeltaTime;
//        Entities.ForEach((ref Translation translation, ref BoxCollider boxCollider) =>
//        {
//            if(boxCollider.minPosition.x < 50)
//                translation.Value += new float3(-1.2f * dt, 0, 0);
//        }).Run();
//    }

//    protected override void OnStartRunning()
//    {
//        Debug.Log("Created");
//        //float dt = Time.DeltaTime;
//        //Entities.ForEach((ref Translation translation, ref BoxCollider boxCollider) =>
//        //{
//        //    if (boxCollider.minPosition.x < 50)
//        //        translation.Value += new float3(-1.2f * dt, 0, 0);
//        //}).Run();
//    }

//    protected override void OnDestroy()
//    {
//        base.OnDestroy();
//        Debug.Log("Destroyed");
//    }
//}

public class AABBCollisionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, in AABBComponent aabb) =>
        {

        }).Run();
    }
}