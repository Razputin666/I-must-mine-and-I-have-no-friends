﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    [SerializeField]
    private string savePath;
    [SerializeField]
    private ItemDatabaseObject itemDatabase;
    public Inventory container;
    public bool AddItem(Item item, int amount)
    {
        //Check if there is an empty slot in the inventory
        if (EmptySlotCount <= 0)
            return false;

        InventorySlot slot = FindItemInInventory(item);

        //If the item is not stackable or we couldnt find the same item type in the inventory add a new slot
        if(!ItemDatabase.GetItemAt(item.id).stackable || slot == null)
        {
            SetEmptySlot(item, amount);
            return true;
        }
        //add the amount to an exisiting item of the same type.
        slot.AddAmount(amount);
        return true;
    }

    public InventorySlot FindItemInInventory(Item item)
    {
        for (int i = 0; i < container.Items.Length; i++)
        {
            if (container.Items[i].Item.id <= -1)
            {
                return container.Items[i];
            }
        }
        return null;
    }

    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < container.Items.Length; i++)
            {
                if (container.Items[i].Item.id <= -1)
                    counter++;
            }
            return counter;
        }
    }

    public InventorySlot SetEmptySlot(Item item, int amount)
    {
        for (int i = 0; i < container.Items.Length; i++)
        {
            //Check if the slot is empty == -1
            if (container.Items[i].Item.id <= -1)
            {
                container.Items[i].UpdateSlot(item, amount);
                return container.Items[i];
            }
        }
        //Setup functionallity when inventory full
        return null;
    }

    public void SwapItems(InventorySlot item1, InventorySlot item2)
    {
        //Check if the items can swap used for the equipment slots.
        if (item2.CanPlaceInSlot(item1.ItemObject) && item1.CanPlaceInSlot(item2.ItemObject))
        {
            InventorySlot temp = new InventorySlot(item2.Item, item2.Amount);
            item2.UpdateSlot(item1.Item, item1.Amount);
            item1.UpdateSlot(temp.Item, temp.Amount);
        }

    }

    public void RemoveItem(Item item)
    {
        for (int i = 0; i < container.Items.Length; i++)
        {
            if(container.Items[i].Item == item)
            {
                container.Items[i].UpdateSlot(null, 0);
            }
        }
    }

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
            
            for (int i = 0; i < container.Items.Length; i++)
            {
                container.Items[i].UpdateSlot(newContainer.Items[i].Item, newContainer.Items[i].Amount);
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

[System.Serializable]
public class Inventory
{
    [SerializeField]
    private InventorySlot[] items = new InventorySlot[36];
    
    public void Clear()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].RemoveItem();
        }
    }
    
    public InventorySlot[] Items
    {
        get
        {
            return this.items;
        }
        set
        {
            this.items = value;
        }
    }
}

[System.Serializable]
public class InventorySlot
{
    public ITEM_TYPE[] itemTypeAllowed = new ITEM_TYPE[0];
    [System.NonSerialized]
    public UserInterface parent;

    public Item Item;
    public int Amount;
    public ItemObject ItemObject
    {
        get
        {
            if(Item.id >= 0)
                return parent.Inventory.ItemDatabase.GetItemAt(Item.id);

            return null;
        }
    }

    public InventorySlot()
    {
        this.Item = new Item();
        this.Amount = 0;
    }

    public InventorySlot(Item item, int amount)
    {
        this.Item = item;
        this.Amount = amount;
    }

    public void UpdateSlot(Item item, int amount)
    {
        this.Item = item;
        this.Amount = amount;
    }

    public void RemoveItem()
    {
        Item = new Item();
        Amount = 0;
    }

    public void AddAmount(int value)
    {
        Amount += value;
    }

    public bool CanPlaceInSlot(ItemObject itemObject)
    {
        //Check if every item is allowed or the item we are trying to place is couldnt be found or if the slot is empty
        if (itemTypeAllowed.Length <= 0 || itemObject == null || itemObject.data.id < 0)
            return true;
            

        for (int i = 0; i < itemTypeAllowed.Length; i++)
        {
            //Check if the item type is allowed in the slot
            if (itemObject.itemType == itemTypeAllowed[i])
            {
                return true;
            }
                
        }
        return false;
    }

    //public Item Item
    //{
    //    get
    //    {
    //        return this.item;
    //    }
    //    set
    //    {
    //        this.item = value;
    //    }
    //}

    //public int Amount
    //{
    //    get
    //    {
    //        return this.amount;
    //    }
    //}

    //public int ID
    //{
    //    get
    //    {
    //        return this.id;
    //    }
    //}
}