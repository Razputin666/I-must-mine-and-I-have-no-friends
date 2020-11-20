using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerMovementEntity : IComponentData
{
    public float horizontalMovement;
    public float verticalMovement;
    public float speed;
}
