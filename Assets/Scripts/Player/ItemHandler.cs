using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemHandler : NetworkBehaviour
{
    [SerializeField]
    private InventoryObject inventory;
    [SerializeField]
    private InventoryObject equipment;
    [SerializeField]
    private InventoryObject quickSlots;

    //[SerializeField]
    //private GameObject inventoryUI;
    //[SerializeField]
    //private GameObject equipmentUI;
    //[SerializeField]
    //private GameObject quickSlotsUI;

    [SerializeField]
    private GameObject playerUIsPrefab;

    private void Start()
    {
        inventory = Instantiate(inventory);
        inventory.Clear();

        if (!isLocalPlayer)
            return;


        equipment = Instantiate(equipment);
        equipment.Clear();

        quickSlots = Instantiate(quickSlots);
        quickSlots.Clear();

        for (int i = 0; i < equipment.GetSlots.Length; i++)
        {
            equipment.GetSlots[i].OnBeforeUpdate += OnBeforeSlotUpdate;
            equipment.GetSlots[i].OnAfterUpdate += OnAfterSlotUpdate;
        }

        for (int i = 0; i < quickSlots.GetSlots.Length; i++)
        {
            quickSlots.GetSlots[i].OnBeforeUpdate += OnBeforeSlotUpdate;
            quickSlots.GetSlots[i].OnAfterUpdate += OnAfterSlotUpdate;
        }
        ShowGUI();
    }

    public void ShowGUI()
    {
        GameObject canvas = GetComponentInChildren<Canvas>().gameObject;
        GameObject playerUI = Instantiate(playerUIsPrefab, canvas.transform);

        GameObject inventoryUI = playerUI.transform.Find("InventoryScreen").gameObject;

        DynamicInterface inventoryUserInterface = inventoryUI.GetComponent<DynamicInterface>();
        inventoryUserInterface.Inventory = inventory;
        inventoryUserInterface.enabled = true;

        GameObject equipmentUI = playerUI.gameObject.transform.Find("EquipmentScreen").gameObject;

        StaticInterface equipmentUserInterface = equipmentUI.GetComponent<StaticInterface>();
        equipmentUserInterface.Inventory = equipment;
        equipmentUserInterface.enabled = true;

        GameObject quickSlotsUI = playerUI.gameObject.transform.Find("QuickSlotScreen").gameObject;

        StaticInterface quickslotsUserInterface = quickSlotsUI.GetComponent<StaticInterface>();
        quickslotsUserInterface.Inventory = quickSlots;
        quickslotsUserInterface.enabled = true;
        //Hides Gui's
        GameObject craftingUI = playerUI.gameObject.transform.Find("CraftingScreen").gameObject;

        CraftingInterface CraftingUserInterface = craftingUI.GetComponent<CraftingInterface>();
        //CraftingUserInterface.Inventory.ToggleVisibility();
        //inventory.ToggleVisibility();
        //equipment.ToggleVisibility();
        //quickSlots.ToggleVisibility();
    }

    public void OnBeforeSlotUpdate(InventorySlot slot)
    {
        if (slot.ItemObject == null && slot.parent.Inventory.InterfaceType != INTERFACE_TYPE.Quickslots)
            return;

        switch (slot.parent.Inventory.InterfaceType)
        {
            case INTERFACE_TYPE.Inventory:
                break;
            case INTERFACE_TYPE.Equipment:
                print(string.Concat("Removed ", slot.ItemObject, " on ", slot.parent.Inventory.InterfaceType, ", Allowed Items: ", string.Join(", ", slot.itemTypeAllowed)));
                //Ta bort variablar från spelaren när man unequippar object

                break;
            case INTERFACE_TYPE.Chest:
                break;
            case INTERFACE_TYPE.Quickslots:
                PlayerController player = GetComponentInParent<PlayerController>();

                if (player.ActiveQuickslot == -1)
                    return;

                InventorySlot[] invSlots = quickSlots.Container.InventorySlot;

                for (int i = 0; i < invSlots.Length; i++)
                {
                    if (invSlots[i].ID == slot.ID)
                    {
                        if (player.ActiveQuickslot == i)
                        {
                            player.UpdateActiveItem(slot.ID);
                            return;
                        }
                    }
                }
                break;
            default:
                break;
        }
    }

    public void OnAfterSlotUpdate(InventorySlot slot)
    {
        if (slot.ItemObject == null && slot.parent.Inventory.InterfaceType != INTERFACE_TYPE.Quickslots)
            return;

        switch (slot.parent.Inventory.InterfaceType)
        {
            case INTERFACE_TYPE.Inventory:
                break;
            case INTERFACE_TYPE.Equipment:
                print(string.Concat("Placed ", slot.ItemObject, " on ", slot.parent.Inventory.InterfaceType, ", Allowed Items: ", string.Join(", ", slot.itemTypeAllowed)));
                //Lägg till variablar till spelaren när man equippar object
                break;
            case INTERFACE_TYPE.Chest:
                break;
            case INTERFACE_TYPE.Quickslots:
                PlayerController player = GetComponentInParent<PlayerController>();

                if (player.ActiveQuickslot == -1)
                    return;

                InventorySlot[] invSlots = quickSlots.Container.InventorySlot;

                for (int i = 0; i < invSlots.Length; i++)
                {
                    if (invSlots[i].ID == slot.ID)
                    {
                        if (player.ActiveQuickslot == i)
                        {
                            player.UpdateActiveItem(slot.ID);
                            return;
                        }
                    }
                }
                break;
            default:
                break;
        }
    }
    [Command]
    private void CmdRemoveGroundItem(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }

    // [Client]
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     GroundItem groundItem = other.GetComponentInParent<GroundItem>();
    //     if(groundItem != null && groundItem.PickupTime <= 1f)
    //     {
    //         Item newItem = new Item(groundItem.Item);
    //         if (inventory.AddItem(newItem, newItem.Amount))
    //         {
    //             CmdRemoveGroundItem(other.transform.parent.gameObject);
    //         }           
    //     }
    // }
    //[Client]
    //public void OnTriggerEnter2D(Collider2D other)
    //{
    //    GroundItem groundItem = other.GetComponentInParent<GroundItem>();
    //    if (groundItem != null && groundItem.PickupTime <= 0f)
    //    {
    //        Item newItem = new Item(groundItem.Item);
    //        if (inventory.AddItem(newItem, newItem.Amount))
    //        {
    //            Destroy(other.transform.parent.gameObject);
    //        }
    //    }
    //}

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        if(Input.GetKeyDown(KeyCode.P))
        {
            inventory.Save();
            quickSlots.Save();
            equipment.Save();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            inventory.Load();
            quickSlots.Load();
            equipment.Load();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            inventory.ToggleVisibility();
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            equipment.ToggleVisibility();
        }
    }

    private void OnApplicationQuit()
    {
        inventory.Clear();
        equipment.Clear();
        quickSlots.Clear();
    }

    public ItemDatabaseObject ItemDatabase
    {
        get
        {
            return this.inventory.ItemDatabase;
        }
    }

    public InventoryObject Inventory
    {
        get
        {
            return this.inventory;
        }
    }

    public InventoryObject Equipment
    {
        get
        {
            return this.equipment;
        }
    }

    public InventoryObject QuickSlots
    {
        get
        {
            return this.quickSlots;
        }
    }
}