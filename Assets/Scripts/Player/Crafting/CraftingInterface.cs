using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftingInterface : UserInterface
{
    [SerializeField]
    private GameObject inventoryPrefab;
    [SerializeField]
    private CraftingChecker craftChecker;
    [SerializeField]
    private InventoryObject playerInventory;

    [SerializeField]
    private int X_START;
    [SerializeField]
    private int Y_START;
    [SerializeField]
    private int X_SPACE_BETWEEN_ITEM;
    [SerializeField]
    private int NUMBER_OF_COLUMN;
    [SerializeField]
    private int Y_SPACE_BETWEEN_ITEM;

    public override void CreateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            GameObject itemObject = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
            itemObject.GetComponent<RectTransform>().localPosition = GetPosition(i);

            inventory.GetSlots[i].slotObject = itemObject;
            inventory.GetSlots[i].OnBeforeUpdate += OnCraftSlotUpdate;
            inventory.GetSlots[i].OnAfterUpdate += OnCraftSlotUpdate;

            slotsOnInterface.Add(itemObject, inventory.GetSlots[i]);
            //Add events
            AddEvent(itemObject, EventTriggerType.PointerEnter, delegate { OnEnter(itemObject); });
            AddEvent(itemObject, EventTriggerType.PointerExit, delegate { OnExit(itemObject); });
            AddEvent(itemObject, EventTriggerType.PointerDown, delegate { OnPointerDown(itemObject); });
            //AddEvent(itemObject, EventTriggerType.BeginDrag, delegate { OnDragStart(itemObject); });
            //AddEvent(itemObject, EventTriggerType.EndDrag, delegate { OnDragEnd(itemObject); });
            //AddEvent(itemObject, EventTriggerType.Drag, delegate { OnDrag(itemObject); });
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            craftChecker.CheckAvailableRecipes(playerInventory);
            List<CraftingRecipeObject> recipes = craftChecker.AvailableRecipes;
            if(recipes.Count > 0)
                craftChecker.CraftItem(playerInventory, recipes[0]);
        }
    }
    private void OnCraftSlotUpdate(InventorySlot slot)
    {
        //when displaying interface check what recipes are craftable
        //update craftables when updating inventory

        craftChecker.CheckAvailableRecipes(playerInventory);

        
    }

    public void OnPointerDown(GameObject itemObject)
    {
        //pointer click get materials and give result to player

        //If we hover over a slot and press the right mousebutton
        if (MouseData.slotHoveredOver != null && Input.GetMouseButton(0))
        {

        }
    }


    private Vector3 GetPosition(int index)
    {
        return new Vector3(X_START + (X_SPACE_BETWEEN_ITEM * (index % NUMBER_OF_COLUMN)), Y_START + (-Y_SPACE_BETWEEN_ITEM * (index / NUMBER_OF_COLUMN)), 0f);
    }

    

    //pointer enter show tooltip

    //pointer exit hide tooltip

    
}