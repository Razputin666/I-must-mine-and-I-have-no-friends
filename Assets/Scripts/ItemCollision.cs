using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemCollision : NetworkBehaviour
{
    //private PlayerController player;
    //private EnemyController enemy;
    //private InventoryObject inventory;

    //public override void OnStartServer()
    //{
    //    Debug.Log("Item");
    //    if (GetComponentInParent<PlayerController>() == null)
    //    {
    //        enemy = GetComponentInParent<EnemyController>();
    //        inventory = enemy.GetInventoryObject;
    //    }
    //    else
    //    {
    //        Debug.Log("Item");
    //        player = GetComponentInParent<PlayerController>();
    //        inventory = player.GetInventoryObject;
    //    }
    //}

    //[Server]
    //public void OnTriggerEnter2D(Collider2D other)
    //{
    //    Debug.Log("collide");
    //    if(other.transform.CompareTag("GroundItem"))
    //    {
    //        GroundItem groundItem = other.GetComponentInParent<GroundItem>();
    //        if (groundItem != null && groundItem.PickupTime <= 0f)
    //        {
    //            if(TryGetComponent<PlayerController>(out PlayerController player))
    //            {
    //                Item newItem = new Item(groundItem.Item);
    //                if (player.GetInventoryObject.AddItem(newItem, newItem.Amount))
    //                {
    //                    if (player != null)
    //                    {
    //                        player.RpcAddItemToPlayer(player.GetComponent<NetworkIdentity>().connectionToClient, other.gameObject);
    //                        NetworkServer.Destroy(other.gameObject);
    //                        //player.RemoveItemFromGround(other.gameObject);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}