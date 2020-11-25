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
    public Sprite uiDisplaySprite;
    public bool stackable;
    public ITEM_TYPE itemType;
    [TextArea(15, 20)]
    public string description;
    public Item data = new Item();

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
    public int id = -1;
    //public ItemBuff[] buffs;

    public Item()
    {
        name = "";
        id = -1;
    }
    public Item(ItemObject item)
    {
        name = item.name;
        id = item.data.id;

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