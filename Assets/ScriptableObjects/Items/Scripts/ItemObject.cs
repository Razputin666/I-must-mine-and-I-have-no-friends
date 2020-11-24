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

//public enum ATTRIBUTES
//{
//    Agi,
//    Int,
//    Sta,
//    Str
//}
public abstract class ItemObject : ScriptableObject
{
    public int itemID;
    public Sprite uiDisplaySprite;
    public ITEM_TYPE itemType;
    [TextArea(15, 20)]
    public string description;
    //public ItemBuff[] buffs;

    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }
}

[System.Serializable]
public class Item
{
    public string name;
    public int id;
    //public ItemBuff[] buffs;

    public Item()
    {
        name = "";
        id = -1;
    }
    public Item(ItemObject item)
    {
        name = item.name;
        id = item.itemID;

        //buffs = new ItemBuff[item.buffs.Length];
        //for(int i = 0; i < buffs.Length; i++)
        //{
        //    buffs[i] = new ItemBuff(item.buffs[i].min, item.buffs[i].max);
        //}
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