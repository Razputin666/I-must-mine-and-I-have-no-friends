using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct StatsEntity : IComponentData
{
   public int movespeed;
   public int maxAccell;
}
