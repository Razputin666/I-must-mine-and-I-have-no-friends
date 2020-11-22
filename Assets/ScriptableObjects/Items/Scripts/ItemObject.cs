using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_TYPE
{
    Default,
    Module,
    Weapon,
    Mining
}

public abstract class ItemObject : ScriptableObject
{
    public GameObject prefab;
    public ITEM_TYPE itemType;
    [TextArea(15, 20)]
    public string description;

}