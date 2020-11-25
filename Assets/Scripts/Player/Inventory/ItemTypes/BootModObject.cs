using UnityEngine;

[CreateAssetMenu(fileName = "New Boot Mod Object", menuName = "Inventory System/Items/Boots")]
public class BootModObject : ItemObject
{
    [SerializeField]
    private float jumpMultiplier;
    //public void Awake()
    //{
    //    //itemType = ITEM_TYPE.WeaponMod;
    //}
}