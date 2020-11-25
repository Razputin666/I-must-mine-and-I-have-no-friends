using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandler : MonoBehaviour
{
    [SerializeField]
    private InventoryObject inventory;
    [SerializeField]
    private InventoryObject equipment;
    public void OnTriggerEnter2D(Collider2D other)
    {
        GroundItem groundItem = other.GetComponent<GroundItem>();

        if(groundItem)
        {
            Item newItem = new Item(groundItem.Item);
            if(inventory.AddItem(newItem, 1))
            {
                Destroy(other.gameObject);
            }           
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            inventory.Save();
            equipment.Save();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            inventory.Load();
            equipment.Load();
        }
    }

    private void OnApplicationQuit()
    {
        inventory.Container.Clear();
        equipment.Container.Clear();
    }
}