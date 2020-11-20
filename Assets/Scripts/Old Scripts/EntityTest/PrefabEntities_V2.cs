using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

//Third method add prefab to reference list
public class PrefabEntities_V2 : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public static Entity prefabEntity;

    public GameObject prefabGameObject;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //Ask conversion system to get the primary entity that matches gameobject
        Entity prefabEntity = conversionSystem.GetPrimaryEntity(prefabGameObject);
        PrefabEntities_V2.prefabEntity = prefabEntity;
    }

    //Add Prefab as a reference
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(prefabGameObject);
    }
}
