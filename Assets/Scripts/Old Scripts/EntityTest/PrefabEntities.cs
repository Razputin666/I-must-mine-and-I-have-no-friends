using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

//Second Method using convert gameobject to entity using conversionUtility to convert gameobject to entity
public class PrefabEntities : MonoBehaviour, IConvertGameObjectToEntity
{
    public static Entity prefabEntity;
    public GameObject prefabGameObject;
    
    //Convert Gameobject to entity
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        using (BlobAssetStore blobAssetStore = new BlobAssetStore())
        {
            Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabGameObject, 
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));

            PrefabEntities.prefabEntity = prefabEntity;
        }
    }
}
