using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    public static void SpawnItemAt(Vector3 spawnPos, ItemObject item)
    {
        spawnPos.z = 0;
        GameObject parentObject = GameObject.Find("ItemSpawner");
        GameObject groundItemPrefab = Resources.Load<GameObject>("Prefabs/GroundItemObject") as GameObject;
        GameObject groundObject = Instantiate(groundItemPrefab, spawnPos + Vector3.up, Quaternion.identity, parentObject.transform);

        GroundItem gItem = groundObject.GetComponent<GroundItem>();
        gItem.Item = item;
        NetworkServer.Spawn(groundObject);
    }

    [Command(ignoreAuthority = true)]
    public void CmdSpawnItemAt(Vector3 spawnPos, int itemID, int itemAmount)
    {
        ItemDatabaseObject db = Resources.Load("ScriptableObjects/ItemDatabase") as ItemDatabaseObject;

        ItemObject item = db.GetItemAt(itemID);
        item.Data.Amount = itemAmount;

        spawnPos.z = 0;
        GameObject parentObject = GameObject.Find("ItemSpawner");
        GameObject groundItemPrefab = Resources.Load<GameObject>("Prefabs/GroundItemObject") as GameObject;
        GameObject groundObject = Instantiate(groundItemPrefab, spawnPos + Vector3.up, Quaternion.identity, parentObject.transform);

        GroundItem gItem = groundObject.GetComponent<GroundItem>();
        gItem.Item = item;
        NetworkServer.Spawn(groundObject);
    }

    public static void SpawnItem(Vector3 spawnPos, Sprite sprite)
    {

    }
}
