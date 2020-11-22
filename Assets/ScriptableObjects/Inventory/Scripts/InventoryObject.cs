using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public List<InventorySlot> container = new List<InventorySlot>();

    public void AddItem(ItemObject item, int amount)
    {
        bool hasItem = false;

        for(int i = 0; i < container.Count; i++)
        {
            if(container[i].Item == item)
            {
                container[i].AddAmount(amount);
                hasItem = true;
                break;
            }
        }

        if(!hasItem)
        {
            container.Add(new InventorySlot(item, amount));
        }
    }
}

[System.Serializable]
public class InventorySlot
{
    private ItemObject item;
    private int amount;

    public InventorySlot(ItemObject item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public void AddAmount(int value)
    {
        amount += value;
    }

    public ItemObject Item
    {
        get
        {
            return this.item;
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