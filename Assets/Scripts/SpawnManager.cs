using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    //public static void SpawnItemAt(Vector3 spawnPos, ItemObject item)
    //{
    //    spawnPos.z = 0;
    //    GameObject parentObject = GameObject.Find("ItemSpawner");
    //    GameObject groundItemPrefab = Resources.Load<GameObject>("Prefabs/GroundItemObject") as GameObject;
    //    GameObject groundObject = Instantiate(groundItemPrefab, spawnPos + Vector3.up, Quaternion.identity, parentObject.transform);

    //    GroundItem gItem = groundObject.GetComponent<GroundItem>();
    //    gItem.Item = item;
    //    NetworkServer.Spawn(groundObject);
    //}

    [Command(ignoreAuthority = true)]
    public void CmdSpawnItemAt(Vector3 spawnPos, int itemID, int itemAmount)
    {
        ItemDatabaseObject db = Resources.Load("ScriptableObjects/ItemDatabase") as ItemDatabaseObject;

        ItemObject item = Instantiate(db.GetItemAt(itemID));
        item.Data.Amount = itemAmount;

        spawnPos.z = 0;
        GameObject parentObject = GameObject.Find("ItemSpawner");
        GameObject groundItemPrefab = Resources.Load<GameObject>("SpawnablePrefabs/GroundItemObject") as GameObject;
        GameObject groundObject = Instantiate(groundItemPrefab, spawnPos + Vector3.up, Quaternion.identity, parentObject.transform);

        groundObject.GetComponent<Rigidbody2D>().simulated = true;
        GroundItem gItem = groundObject.GetComponent<GroundItem>();
        gItem.SetItemObject(item, 0f);

        NetworkServer.Spawn(groundObject);

        //RpcChangeSprite(groundObject, itemID);
    }

    //[ClientRpc]
    //private void RpcChangeSprite(GameObject groundObject, int itemID)
    //{
    //    //groundObject.GetComponent<Rigidbody2D>().gravityScale = 0f;

    //    ItemDatabaseObject db = Resources.Load("ScriptableObjects/ItemDatabase") as ItemDatabaseObject;

    //    ItemObject item = Instantiate(db.GetItemAt(itemID));
    //    item.Data.Amount = itemAmount;

    //    GroundItem gItem = groundObject.GetComponent<GroundItem>();
    //    gItem.SetItemObject(item, 0f);
    //}
}
