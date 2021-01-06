using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stats", menuName = "Stats")]
public class Stats : ScriptableObject
{

    public int hitPoints;
    public float moveSpeed;
    public int strength;
    public float mineStrength;
    public float aggroRange;
    public float maxSpeed;  // 10f på default
    public float maxFallSpeed;  // -25f på default
    public float range;

}
