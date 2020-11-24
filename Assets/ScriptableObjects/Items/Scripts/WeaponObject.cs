using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Object", menuName = "Inventory System/Items/Weapon")]
public class WeaponObject : ItemObject
{
    [SerializeField]
    private float fireRate;
    public void Awake()
    {
        itemType = ITEM_TYPE.WeaponMod;
    }
}
