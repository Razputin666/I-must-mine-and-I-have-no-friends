using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollision : MonoBehaviour
{
    private PlayerController player;
    private EnemyController enemy;
    private InventoryObject inventory;
    // Start is called before the first frame update
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        
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
                    Destroy(other.gameObject);
                }
            }
        }
    }
}
