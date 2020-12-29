using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class SpawnManager : NetworkBehaviour
{
    [Command(ignoreAuthority = true)]
    public void CmdSpawnItemFromIDAt(Vector3 position, int itemID, int itemAmount)
    {
        SpawnItemAt(position, itemID, itemAmount);
    }
    [Command(ignoreAuthority = true)]
    private void CmdSpawnItemFromNameAt(Vector3 position, string itemName)
    {
        SpawnItemAt(position, itemName);
    }
    public void SpawnItemAt(Vector3 position, string itemName)
    {
        if (!isServer)
            CmdSpawnItemFromNameAt(position, itemName);
        else
        {
            ItemDatabaseObject db = Resources.Load("ScriptableObjects/ItemDatabase") as ItemDatabaseObject;

            ItemObject item = Instantiate(db.GetItemOfName(itemName));

            position.z = 0;
            GameObject parentObject = GameObject.Find("ItemSpawner");
            GameObject groundItemPrefab = Resources.Load<GameObject>("SpawnablePrefabs/GroundItemObject") as GameObject;
            GameObject groundObject = Instantiate(groundItemPrefab, position + Vector3.up, Quaternion.identity, parentObject.transform);

            groundObject.GetComponent<Rigidbody2D>().simulated = true;
            GroundItem gItem = groundObject.GetComponent<GroundItem>();
            gItem.SetItemObject(item, 0f);

            NetworkServer.Spawn(groundObject);
        }
    }

    public void SpawnItemAt(Vector3 position, int itemID, int itemAmount)
    {
        if (!isServer)
            CmdSpawnItemFromIDAt(position, itemID, itemAmount);
        else
        {
            ItemDatabaseObject db = Resources.Load("ScriptableObjects/ItemDatabase") as ItemDatabaseObject;

            ItemObject item = Instantiate(db.GetItemAt(itemID));
            item.Data.Amount = itemAmount;

            position.z = 0;
            GameObject parentObject = GameObject.Find("ItemSpawner");
            GameObject groundItemPrefab = Resources.Load<GameObject>("SpawnablePrefabs/GroundItemObject") as GameObject;
            GameObject groundObject = Instantiate(groundItemPrefab, position + Vector3.up, Quaternion.identity, parentObject.transform);

            groundObject.GetComponent<Rigidbody2D>().simulated = true;
            GroundItem gItem = groundObject.GetComponent<GroundItem>();
            gItem.SetItemObject(item, 0f);

            NetworkServer.Spawn(groundObject);
        }
    }
}
