using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemCollision : NetworkBehaviour
{
    private PlayerController player;
    private EnemyController enemy;
    private InventoryObject inventory;

    // Start is called before the first frame update
    public void Start()
    {
        if(GetComponentInParent<PlayerController>() == null)
        {
            enemy = GetComponentInParent<EnemyController>();
            inventory = enemy.GetInventoryObject;
        }
        else
        {
            player = GetComponentInParent<PlayerController>();
            inventory = player.GetInventoryObject;
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.CompareTag("GroundItem"))
        {
            GroundItem groundItem = other.GetComponentInParent<GroundItem>();
            if (groundItem != null && groundItem.PickupTime <= 0f)
            {
                
                Item newItem = new Item(groundItem.Item);
                if (inventory.AddItem(newItem, newItem.Amount))
                {
                    if(player != null)
                        player.RemoveItemFromGround(other.gameObject);
                }
            }
        }
    }
}