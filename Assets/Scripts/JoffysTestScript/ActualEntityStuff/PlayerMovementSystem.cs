using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;


[AlwaysSynchronizeSystem]
public class PlayerMovementSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;
        float xBound = GamemanagerJoffy.main.xBound;

        Entities.ForEach((ref Translation trans, in PlayerMovementEntity data) =>
        {
            trans.Value.x = math.clamp(trans.Value.x + (data.speed * data.horizontalMovement * deltaTime), -xBound, xBound);
        }).Run();

        return default;
    }
}
