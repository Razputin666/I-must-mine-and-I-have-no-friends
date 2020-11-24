using System.Collections;
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
    public void AddItem(Item item, int amount)
    {
        
        //if (!item.Stackable)
        //{
        //SetEmptySlot(item, amount);
        //}
        for (int i = 0; i < container.Items.Length; i++)
        {
            if (container.Items[i].ID == item.id)
            {
                container.Items[i].AddAmount(amount);
                return;
            }
        }
        SetEmptySlot(item, amount);
    }

    public InventorySlot SetEmptySlot(Item item, int amount)
    {
        for (int i = 0; i < container.Items.Length; i++)
        {
            if (container.Items[i].ID <= -1)
            {
                container.Items[i].UpdateSlot(item.id, item, amount);
                return container.Items[i];
            }
        }
        //Setup functionallity when inventory full
        return null;
    }

    public void MoveItem(InventorySlot item1, InventorySlot item2)
    {
        InventorySlot temp = new InventorySlot(item2.ID, item2.Item, item2.Amount);
        item2.UpdateSlot(item1.ID, item1.Item, item1.Amount);
        item1.UpdateSlot(temp.ID, temp.Item, temp.Amount);
    }

    public void RemoveItem(Item item)
    {
        for (int i = 0; i < container.Items.Length; i++)
        {
            if(container.Items[i].Item == item)
            {
                container.Items[i].UpdateSlot(-1, null, 0);
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
                container.Items[i].UpdateSlot(newContainer.Items[i].ID, newContainer.Items[i].Item, newContainer.Items[i].Amount);
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
            items[i].UpdateSlot(-1, new Item(), 0);
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
    public UserInterface parent;
    public int ID;

    public Item Item;

    public int Amount;

    public InventorySlot()
    {
        this.ID = -1;
        this.Item = null;
        this.Amount = 0;
    }

    public InventorySlot(int id, Item item, int amount)
    {
        this.ID = id;
        this.Item = item;
        this.Amount = amount;
    }

    public void UpdateSlot(int id, Item item, int amount)
    {
        this.ID = id;
        this.Item = item;
        this.Amount = amount;
    }

    public void AddAmount(int value)
    {
        Amount += value;
    }

    public bool CanPlaceInSlot(ItemObject item)
    {
        if (itemTypeAllowed.Length <= 0)
        {
            return true;
        }
            

        for (int i = 0; i < itemTypeAllowed.Length; i++)
        {
            if (item.itemType == itemTypeAllowed[i])
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