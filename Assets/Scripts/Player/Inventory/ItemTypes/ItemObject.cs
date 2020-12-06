using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_TYPE
{
    None,
    HelmetMod,
    ChestMod,
    LegMod,
    BootMod,
    LaserMod,
    FlightMod,
    LightMod,
    WeaponMod,
    TileBlock,
    Weapon,
    MiningLaser,
    Component 
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