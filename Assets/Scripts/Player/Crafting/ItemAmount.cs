using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemAmount
{
    [SerializeField]
    private ItemObject itemObject;

    [SerializeField]
    private int amount;

    public ItemObject ItemObject
    {
        get
        {
            return this.itemObject;
        }
        set
        {
            this.itemObject = value;
        }
    }

    public int Amount
    {
        get
        {
            return this.amount;
        }
        set
        {
            this.amount = value;
        }
    }
}
