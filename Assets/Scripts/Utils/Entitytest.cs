using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Entitytest : MonoBehaviour
{
    private void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.CreateEntity(typeof(StatsEntity));
    }
}
