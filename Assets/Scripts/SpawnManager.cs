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
        GameObject groundObject = Instantiate(groundItemPrefab, spawnPos, Quaternion.identity, parentObject.transform);

        GroundItem gItem = groundObject.GetComponent<GroundItem>();
        gItem.Item = item;
    }
}
