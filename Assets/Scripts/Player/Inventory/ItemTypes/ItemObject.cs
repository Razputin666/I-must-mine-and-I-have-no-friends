using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_TYPE
{
    HelmetMod,
    ChestMod,
    LegMod,
    BootMod,
    LaserMod,
    FlightMod,
    LightMod,
    WeaponMod
}

public abstract class ItemObject : ScriptableObject
{
    [SerializeField]
    protected Sprite uiDisplaySprite;
    [SerializeField]
    protected bool stackable;
    [SerializeField]
    protected ITEM_TYPE itemType;
    [SerializeField]
    [TextArea(15, 20)]
    protected string description;
    [SerializeField]
    protected Item data = new Item();

    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }
    #region Getters
    public Sprite UIDisplaySprite
    {
        get 
        {
            return this.uiDisplaySprite;
        }
    }
    public bool Stackable
    {
        get
        {
            return this.stackable;
        }
    }
    public ITEM_TYPE ItemType
    {
        get
        {
            return this.itemType;
        }
    }
    public string Description
    {
        get
        {
            return this.description;
        }
    }
    public Item Data
    {
        get
        {
            return this.data;
        }
    }
    #endregion
}

[System.Serializable]
public class Item
{
    [SerializeField]
    private string name;
    [SerializeField]
    private int id = -1;
    //public ItemBuff[] buffs;

    public Item()
    {
        name = "";
        id = -1;
    }
    public Item(ItemObject item)
    {
        name = item.name;
        id = item.Data.id;

        //buffs = new ItemBuff[item.buffs.Length];
        //for(int i = 0; i < buffs.Length; i++)
        //{
        //    buffs[i] = new ItemBuff(item.buffs[i].min, item.buffs[i].max);
        //}
    }

    public string Name
    {
        get
        {
            return this.name;
        }
    }

    public int ID
    {
        get
        {
            return this.id;
        }
        set
        {
            this.id = value;
        }
    }
}
//[System.Serializable]
//public class ItemBuff
//{
//    public ATTRIBUTES attributes;
//    public int value;
//    public int min;
//    public int max;

//    public ItemBuff(int min, int max)
//    {
//        this.min = min;
//        this.max = max;
//        GenerateValue();
//    }

//    public void GenerateValue()
//    {
//        value = UnityEngine.Random.Range(min, max);
//    }
//}