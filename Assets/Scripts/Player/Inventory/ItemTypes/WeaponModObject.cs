﻿using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Mod Object", menuName = "Inventory System/Items/Weapon Mod")]
public class WeaponModObject : ItemObject
{
    [SerializeField]
    private float fireRate;
    //public void Awake()
    //{
    //    //itemType = ITEM_TYPE.WeaponMod;
    //}
}
