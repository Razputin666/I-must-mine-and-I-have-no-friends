using UnityEngine;

[System.Serializable]
public class Inventory
{
    [SerializeField]
    private InventorySlot[] inventorySlots = new InventorySlot[32];

    public void Clear()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].RemoveItem();
        }
    }

    public InventorySlot[] InventorySlot
    {
        get
        {
            return this.inventorySlots;
        }
        set
        {
            this.inventorySlots = value;
        }
    }
}
