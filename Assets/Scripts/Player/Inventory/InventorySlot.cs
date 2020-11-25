using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SlotUpdated(InventorySlot slot);

[System.Serializable]
public class InventorySlot
{
    public ITEM_TYPE[] itemTypeAllowed = new ITEM_TYPE[0];
    [System.NonSerialized]
    public UserInterface parent;
    [System.NonSerialized]
    public GameObject slotObject;
    [System.NonSerialized]
    public SlotUpdated OnAfterUpdate;
    [System.NonSerialized]
    public SlotUpdated OnBeforeUpdate;

    [SerializeField]
    private Item item;
    [SerializeField]
    private int amount;
    public ItemObject ItemObject
    {
        get
        {
            if (item.ID >= 0)
                return parent.Inventory.ItemDatabase.GetItemAt(item.ID);

            return null;
        }
    }
    #region Constructors
    public InventorySlot()
    {
        UpdateSlot(new Item(), 0);
    }

    public InventorySlot(Item item, int amount)
    {
        UpdateSlot(item, amount);
    }
    #endregion 

    public void UpdateSlot(Item item, int amount)
    {
        if (OnBeforeUpdate != null)
            OnBeforeUpdate.Invoke(this);

        this.item = item;
        this.amount = amount;

        if (OnAfterUpdate != null)
            OnAfterUpdate.Invoke(this);

    }

    public void RemoveItem()
    {
        UpdateSlot(new Item(), 0);
    }

    public void AddAmount(int value)
    {
        this.amount += value;
    }
    //Check if we can place the itemobject in this slot
    public bool CanPlaceInSlot(ItemObject itemObject)
    {
        //Check if every item is allowed or the item we are trying to place is couldnt be found or if the slot is empty
        if (itemTypeAllowed.Length <= 0 || itemObject == null || itemObject.Data.ID < 0)
            return true;


        for (int i = 0; i < itemTypeAllowed.Length; i++)
        {
            //Check if the item type is allowed in the slot
            if (itemObject.ItemType == itemTypeAllowed[i])
            {
                return true;
            }

        }
        return false;
    }

    public Item Item
    {
        get
        {
            return this.item;
        }
        set
        {
            this.item = value;
        }
    }

    public int Amount
    {
        get
        {
            return this.amount;
        }
    }
}
