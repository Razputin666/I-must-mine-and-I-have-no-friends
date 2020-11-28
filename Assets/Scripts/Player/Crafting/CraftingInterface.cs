using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CraftingInterface : UserInterface
{
    [SerializeField]
    private GameObject inventoryPrefab;
    [SerializeField]
    private GameObject recipePrefab;
    [SerializeField]
    private InventoryObject playerInventory;
    private CraftingChecker craftChecker;

    private CraftingRecipeObject selectedRecipe;
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

    Dictionary<GameObject, CraftingRecipeObject> crafting = new Dictionary<GameObject, CraftingRecipeObject>();

    public override void CreateSlots()
    {
        GameObject contentObject = GameObject.Find("Content");
        
        craftChecker = GetComponent<CraftingChecker>();
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

        //for (int i = 0; i < inventory.GetSlots.Length; i++)
        //{
            //GameObject itemObject = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
            //itemObject.GetComponent<RectTransform>().localPosition = GetPosition(i);

            //inventory.GetSlots[i].slotObject = itemObject;
            //inventory.GetSlots[i].OnBeforeUpdate += OnCraftSlotUpdate;
            //inventory.GetSlots[i].OnAfterUpdate += OnCraftSlotUpdate;

            //slotsOnInterface.Add(itemObject, inventory.GetSlots[i]);

            //Add events
            //AddEvent(itemObject, EventTriggerType.PointerEnter, delegate { OnEnter(itemObject); });
            //AddEvent(itemObject, EventTriggerType.PointerExit, delegate { OnExit(itemObject); });
            //AddEvent(itemObject, EventTriggerType.PointerDown, delegate { OnPointerDown(itemObject); });
            //AddEvent(itemObject, EventTriggerType.BeginDrag, delegate { OnDragStart(itemObject); });
            //AddEvent(itemObject, EventTriggerType.EndDrag, delegate { OnDragEnd(itemObject); });
            //AddEvent(itemObject, EventTriggerType.Drag, delegate { OnDrag(itemObject); });

            //inventory.GetSlots[i].slotObject.transform.GetChild(0).GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(0f, 70f / 255f, 168f / 255f, 190f / 255f);
        //}

        int yPos = -16;
        CraftingRecipeObject[] craftingRecipes = craftChecker.CraftingDatabase.GetRecipes();
        for (int i = 0; i < craftingRecipes.Length; i++)
        {
            GameObject newRecipe = Instantiate(recipePrefab, Vector3.zero, Quaternion.identity, contentObject.transform);
            newRecipe.GetComponent<Text>().text = craftingRecipes[i].name;
            newRecipe.GetComponent<RectTransform>().localPosition = new Vector3(40, yPos -= 50, 0);
                
            AddEvent(newRecipe, EventTriggerType.PointerDown, delegate { OnRecipeSelectionChanged(newRecipe); });
            crafting.Add(newRecipe, craftingRecipes[i]);
        }
    }

    private void OnRecipeSelectionChanged(GameObject obj)
    {
        craftChecker.CheckAvailableRecipes(playerInventory);
        CraftingRecipeObject craft = crafting[obj];
        selectedRecipe = craft;
        for (int i = 0; i < craft.Materials.Count || i < 6; i++)
        {
            if(i < craft.Materials.Count)
            {
                gameObject.transform.GetChild(i).GetComponentInChildren<UnityEngine.UI.Image>().sprite = craft.Materials[i].ItemObject.UIDisplaySprite;
                gameObject.transform.GetChild(i).GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
                gameObject.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = craft.Materials[i].Amount == 1 ? "" : craft.Materials[i].Amount.ToString("n0");
            }
            else
            {
                gameObject.transform.GetChild(i).GetComponentInChildren<UnityEngine.UI.Image>().sprite = null;
                gameObject.transform.GetChild(i).GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(0, 70f / 255f, 168f / 255f, 1);
                gameObject.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }

        gameObject.transform.GetChild(6).GetComponentInChildren<UnityEngine.UI.Image>().sprite = craft.ResultObject.ItemObject.UIDisplaySprite;
        gameObject.transform.GetChild(6).GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
        gameObject.transform.GetChild(6).GetComponentInChildren<TextMeshProUGUI>().text = craft.ResultObject.Amount == 1 ? "" : craft.ResultObject.Amount.ToString("n0");
        gameObject.transform.GetChild(7).GetComponentInChildren<TextMeshProUGUI>().text = craft.name;

        UnityEngine.UI.Button button = GetComponentInChildren<UnityEngine.UI.Button>();
        //button.on.AddListener(OnCraftClick);
        button.gameObject.AddComponent<EventTrigger>();
        AddEvent(button.gameObject, EventTriggerType.PointerDown, delegate { OnCraftClick(); });
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            craftChecker.CheckAvailableRecipes(playerInventory);
            
        }
    }

    private void OnCraftClick()
    {
        //if(Input.GetMouseButtonDown(0))
        {
            List<CraftingRecipeObject> recipes = craftChecker.CraftableItems;
            for (int i = 0; i < recipes.Count; i++)
            {
                if (recipes[i].ID == selectedRecipe.ID)
                {
                    Debug.Log("crafting " + selectedRecipe.ResultObject.ItemObject + " A:" + selectedRecipe.ResultObject.Amount);
                    craftChecker.CraftItem(playerInventory, recipes[i]);
                    break;
                }
            }
        }
        
    }
    //private void OnCraftSlotUpdate(InventorySlot slot)
    //{
    //    //when displaying interface check what recipes are craftable
    //    //update craftables when updating inventory
    //    Debug.Log("update");
    //    craftChecker.CheckAvailableRecipes(playerInventory);

    //    CraftingRecipeObject[] recipes = craftChecker.CraftingDatabase.GetRecipes();

    //    for (int i = 0; i < recipes.Length && i < inventory.GetSlots.Length; i++)
    //    {
    //        InventorySlot slotObj = inventory.GetSlots[i];
    //        ItemObject resultingObject = recipes[i].ResultObject.ItemObject;

    //        //for (int j = 0; j < craftChecker.CraftableItems.Count; j++)
    //        //{
    //        //    if(craftChecker.CraftableItems[j].ID == recipes[i].ID)
    //        //    {
    //        //        slotObj.slotObject.transform.GetChild(0).GetComponentInChildren<UnityEngine.UI.Image>().sprite = resultingObject.UIDisplaySprite;
    //        //        slotObj.slotObject.transform.GetChild(0).GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
    //        //        slotObj.slotObject.GetComponentInChildren<TextMeshProUGUI>().text = slot.Amount == 1 ? "" : recipes[i].ResultObject.Amount.ToString("n0");
    //        //    }
    //        //    else
    //        //    {
    //        //        slotObj.slotObject.transform.GetChild(0).GetComponentInChildren<UnityEngine.UI.Image>().sprite = null;
    //        //        slotObj.slotObject.transform.GetChild(0).GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(0, 70f/255f, 168f/255f, 1);
    //        //        slotObj.slotObject.GetComponentInChildren<TextMeshProUGUI>().text = "";
    //        //    }
    //        //}
            
    //    }
    //}

    private Vector3 GetPosition(int index)
    {
        return new Vector3(X_START + (X_SPACE_BETWEEN_ITEM * (index % NUMBER_OF_COLUMN)), Y_START + (-Y_SPACE_BETWEEN_ITEM * (index / NUMBER_OF_COLUMN)), 0f);
    }    
}