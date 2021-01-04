using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollision : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemyBehaviour enemy;
    private InventoryObject inventory;
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponentInParent<PlayerController>() == null)
        {
            inventory = enemy.GetInventory;
        }
        else
        {
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
