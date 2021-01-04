using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static void SpawnItemAt(Vector3 spawnPos, ItemObject item)
    {
        spawnPos.z = 0;
        GameObject parentObject = GameObject.Find("ItemSpawner");
        GameObject groundItemPrefab = Resources.Load<GameObject>("Prefabs/GroundItemObject") as GameObject;
        GameObject groundObject = Instantiate(groundItemPrefab, spawnPos + Vector3.up, Quaternion.identity, parentObject.transform);

        GroundItem gItem = groundObject.GetComponent<GroundItem>();
        gItem.SetItemObject(item, 0f);
    }

    public static void SpawnItem(Vector3 spawnPos, Sprite sprite)
    {

    }
}
