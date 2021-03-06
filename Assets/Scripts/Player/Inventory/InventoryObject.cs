﻿using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;

public enum INTERFACE_TYPE
{
    Inventory,
    Equipment,
    Chest,
    Crafting,
    Quickslots
}

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    [SerializeField]
    private string savePath;
    [SerializeField]
    private ItemDatabaseObject itemDatabase;
    [SerializeField]
    private Inventory container;
    [SerializeField]
    private INTERFACE_TYPE interfaceType;

    public InventorySlot GetEmptySlot()
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].Item.ID <= -1)
            {
                return GetSlots[i];
            }
        }
        return null;
    }

    public InventorySlot FindItemInInventory(Item item)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].Item.ID == item.ID)
            {
                return GetSlots[i];
            }
        }
        return null;
    }

    public InventorySlot FindItemInInventory(string itemName)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].Item.Name == itemName)
            {
                return GetSlots[i];
            }
        }
        return null;
    }

    public List<InventorySlot> FindItemsInInventory(string itemType)
    {
        List<InventorySlot> itemList = new List<InventorySlot>(); 
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].Item.Type == itemType)
            {
                itemList.Add(GetSlots[i]);
            }
        }
        return itemList;
    }
    public bool IsItemInInventory(Item item)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].Item.ID == item.ID)
            {
                return true;
            }
        }
        return false;
    }

    public bool AddItem(Item item, int amount)
    {
        //Check if there is an empty slot in the inventory
        if (EmptySlotCount <= 0 && FindItemInInventory(item) == null)
            return false;

        InventorySlot slot = FindItemInInventory(item);
        //If the item is not stackable or we couldnt find the same item type in the inventory add a new slot
        if (!ItemDatabase.GetItemAt(item.ID).Stackable || slot == null)
        {
            GetEmptySlot().UpdateSlot(item, amount);
            return true;
        }
        //add the amount to an exisiting item of the same type.
        slot.AddAmount(amount);
        return true;
    }

    public void SwapItems(InventorySlot slot1, InventorySlot slot2)
    {
        if (slot1 == slot2)
            return;
        
        //Check if the items can swap used for the equipment slots.
        if (slot2.CanPlaceInSlot(slot1.ItemObject) && slot1.CanPlaceInSlot(slot2.ItemObject))
        {
            //check so that both slots arent empty
            if(slot1.ID >= 0 || slot2.ID >= 0)
            {
                //check if the items are of the same type then stack them and is stackable
                if (slot1.ID == slot2.ID && ItemDatabase.GetItemAt(slot1.ID).Stackable)
                {
                    slot2.UpdateSlot(slot2.Item, slot2.Amount + slot1.Amount);
                    slot1.RemoveItem();
                }
                else
                {
                    InventorySlot temp = new InventorySlot(slot2.Item, slot2.Amount);
                    slot2.UpdateSlot(slot1.Item, slot1.Amount);
                    slot1.UpdateSlot(temp.Item, temp.Amount);
                }
            }
        }
    }

    public void SplitItem(InventorySlot item)
    {
        //Check if the slot is empty
        if (item == null || item.ID < 0)
            return;
        //check if the item is stackable
        if (!ItemDatabase.GetItemAt(item.ID).Stackable || item.Amount <= 1)
            return;

        int newAmount = item.Amount / 2;

        InventorySlot newSlot = GetEmptySlot();

        //check if there was an empty slot
        if (newSlot == null)
            return;

        //Create a new item from the same itemObject type.
        Item newItem = new Item(Instantiate(ItemDatabase.GetItemAt(item.ID)));
        //Update the amount of the original item.
        item.UpdateSlot(item.Item, item.Amount - newAmount);
        //put the new item in the inventory with an updated amount.
        newSlot.UpdateSlot(newItem, newAmount);
    }

    public void RemoveItem(Item item)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if(GetSlots[i].Item == item)
            {
                GetSlots[i].RemoveItem();
            }
        }
    }
    public void RemoveItem(Item item, int amount)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].Item == item)
            {
                if (item.Amount > amount)
                    GetSlots[i].UpdateSlot(item, item.Amount - amount);
                else
                    GetSlots[i].RemoveItem();
            }
        }
    }
    public void RemoveAllItems()
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].Item != null)
            {
                GetSlots[i].RemoveItem();
            }
        }
    }

    public void ToggleVisibility()
    {
        GetSlots[0].parent.gameObject.SetActive(false);//!GetSlots[0].parent.gameObject.activeSelf);
    }
    #region Getters
    public Inventory Container
    {
        get 
        {
            return this.container;
        }
    }

    public ItemDatabaseObject ItemDatabase
    {
        get
        {
            return this.itemDatabase;
        }
    }
    public INTERFACE_TYPE InterfaceType
    {
        get
        {
            return this.interfaceType;
        }
    }
    public InventorySlot[] GetSlots
    {
        get
        {
            return container.InventorySlot;
        }
    }

    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < GetSlots.Length; i++)
            {
                if (GetSlots[i].Item.ID <= -1)
                    counter++;
            }
            return counter;
        }
    }
#endregion
    #region Save/Load
    [ContextMenu("Save")]
    public void Save()
    {
        //string saveData = JsonUtility.ToJson(this, true);
        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        //bf.Serialize(file, saveData);
        //file.Close();

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, container);
        stream.Close();
    }
    [ContextMenu("Load")]
    public void Load()
    {
        if(File.Exists(string.Concat(Application.persistentDataPath, savePath)))
        {
            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
            //JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this);
            //file.Close();

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
            Inventory newContainer = (Inventory)formatter.Deserialize(stream);
            
            for (int i = 0; i < GetSlots.Length; i++)
            {
                GetSlots[i].UpdateSlot(newContainer.InventorySlot[i].Item, newContainer.InventorySlot[i].Amount);
            }
            stream.Close();
        }
    }
    [ContextMenu("Clear")]
    public void Clear()
    {
        container.Clear();
    }
    #endregion
}
