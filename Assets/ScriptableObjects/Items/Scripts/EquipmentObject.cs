using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Object", menuName = "Inventory System/Items/Equipment")]
public class EquipmentObject : ItemObject
{
    public float fireRate;
    public void Awake()
    {
        itemType = ITEM_TYPE.Module;
    }
}
