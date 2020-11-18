using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerInputEntity : IComponentData
{
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;
}
