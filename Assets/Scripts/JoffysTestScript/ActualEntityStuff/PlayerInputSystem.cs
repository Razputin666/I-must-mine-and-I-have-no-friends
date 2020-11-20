using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

[AlwaysSynchronizeSystem]
public class PlayerInputSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        
        Entities.ForEach((ref PlayerMovementEntity moveData, in PlayerInputEntity inputData) => 
        {
            moveData.horizontalMovement = 0;
            moveData.verticalMovement = 0;


            moveData.horizontalMovement -= Input.GetKey(inputData.leftKey) ? 1 : 0;
            moveData.horizontalMovement += Input.GetKey(inputData.rightKey) ? 1 : 0;
            moveData.verticalMovement += Input.GetKey(inputData.jumpKey) ? 1 : 0;
        }).Run();

        return default;
    }
}
