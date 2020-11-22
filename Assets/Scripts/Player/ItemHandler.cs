using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandler : MonoBehaviour
{
    [SerializeField]
    private InventoryObject inventory;

    public void OnTriggerEnter2D(Collider2D other)
    {
        ItemNew item = other.GetComponent<ItemNew>();

        if(item)
        {
            inventory.AddItem(item.Item, 1);
            Destroy(other.gameObject);
        }
    }
}
