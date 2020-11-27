using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandler : MonoBehaviour
{
    [SerializeField]
    private InventoryObject inventory;
    [SerializeField]
    private InventoryObject equipment;

    private void Start()
    {
        for (int i = 0; i < equipment.GetSlots.Length; i++)
        {
            equipment.GetSlots[i].OnBeforeUpdate += OnBeforeSlotUpdate;
            equipment.GetSlots[i].OnAfterUpdate += OnAfterSlotUpdate;
        }
    }

    public void OnBeforeSlotUpdate(InventorySlot slot)
    {
        if (slot.ItemObject == null)
            return;

        switch (slot.parent.Inventory.InterfaceType)
        {
            case INTERFACE_TYPE.Inventory:
                break;
            case INTERFACE_TYPE.Equipment:
                print(string.Concat("Removed ", slot.ItemObject, " on ", slot.parent.Inventory.InterfaceType, ", Allowed Items: ", string.Join(", ", slot.itemTypeAllowed)));
                //Ta bort variablar från spelaren när man unequippar object

                break;
            case INTERFACE_TYPE.Chest:
                break;
            default:
                break;
        }
    }

    public void OnAfterSlotUpdate(InventorySlot slot)
    {
        if (slot.ItemObject == null)
            return;

        switch (slot.parent.Inventory.InterfaceType)
        {
            case INTERFACE_TYPE.Inventory:
                break;
            case INTERFACE_TYPE.Equipment:
                print(string.Concat("Placed ", slot.ItemObject, " on ", slot.parent.Inventory.InterfaceType, ", Allowed Items: ", string.Join(", ", slot.itemTypeAllowed)));
                //Lägg till variablar till spelaren när man equippar object
                break;
            case INTERFACE_TYPE.Chest:
                break;
            default:
                break;
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        GroundItem groundItem = other.GetComponentInParent<GroundItem>();
        if(groundItem && groundItem.PickupTime <= 0f)
        {
            Item newItem = new Item(groundItem.Item);
            if (inventory.AddItem(newItem, newItem.Amount))
            {
                Destroy(other.transform.parent.gameObject);
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
        inventory.Clear();
        equipment.Clear();
    }

    public ItemDatabaseObject ItemDatabase
    {
        get
        {
            return this.inventory.ItemDatabase;
        }
    }
}