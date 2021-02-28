using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CraftingInterface : UserInterface
{
    [SerializeField] private GameObject inventoryPrefab;
    [SerializeField] private GameObject recipePrefab;
    [SerializeField] private GameObject[] materialSlots;
    [SerializeField] private GameObject resultSlot;
    [SerializeField] private GameObject craftName;
    [SerializeField] private InventoryObject playerInventory;
    private CraftingChecker craftChecker;

    private CraftingRecipeObject selectedRecipe;

    private float craftButtonCooldown = 1f;

    Dictionary<GameObject, CraftingRecipeObject> crafting = new Dictionary<GameObject, CraftingRecipeObject>();

    public override void CreateSlots()
    {
        GameObject contentObject = GameObject.Find("Content");
        
        craftChecker = GetComponent<CraftingChecker>();
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

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

        for (int i = 0; i < materialSlots.Length; i++)
        {
            GameObject itemObject = materialSlots[i];

            inventory.GetSlots[i].slotObject = itemObject;

            //Link the database to the itemObject
            slotsOnInterface.Add(itemObject, inventory.GetSlots[i]);
            inventory.GetSlots[i].OnAfterUpdate += OnSlotUpdate;

            //Add events to our object inside our gameobjects array that is in the same order as our equipment database
            AddEvent(itemObject, EventTriggerType.PointerEnter, delegate { OnEnter(itemObject); });
            AddEvent(itemObject, EventTriggerType.PointerExit, delegate { OnExit(itemObject); });

            materialSlots[i].SetActive(false);
        }
        inventory.GetSlots[6].slotObject = resultSlot;
        inventory.GetSlots[6].OnAfterUpdate += OnSlotUpdate;

        //Link the database to the itemObject
        slotsOnInterface.Add(resultSlot, inventory.GetSlots[6]);
        
        AddEvent(resultSlot, EventTriggerType.PointerEnter, delegate { OnEnter(resultSlot); });
        AddEvent(resultSlot, EventTriggerType.PointerExit, delegate { OnExit(resultSlot); });

        UnityEngine.UI.Button button = GetComponentInChildren<UnityEngine.UI.Button>();

        AddEvent(button.gameObject, EventTriggerType.PointerDown, delegate { OnCraftClick(); });

        CraftingRecipeObject startCraft = craftChecker.CraftingDatabase.GetRecipeAt(0);

        for (int i = 0; i < startCraft.Materials.Count; i++)
        {
            slotsOnInterface[materialSlots[i]].UpdateSlot(new Item(Instantiate(startCraft.Materials[i].ItemObject)), startCraft.Materials[i].Amount);

            materialSlots[i].SetActive(true);
        }

        slotsOnInterface[resultSlot].UpdateSlot(new Item(Instantiate(startCraft.ResultObject.ItemObject)), startCraft.ResultObject.Amount);
        craftName.GetComponentInChildren<TextMeshProUGUI>().text = startCraft.name;
    }

    private void OnRecipeSelectionChanged(GameObject obj)
    {
        craftChecker.CheckAvailableRecipes(playerInventory);
        CraftingRecipeObject craft = crafting[obj];
        selectedRecipe = craft;
        for (int i = 0; i < materialSlots.Length; i++)
        {
            if(i < craft.Materials.Count)
            {
                slotsOnInterface[materialSlots[i]].UpdateSlot(new Item(Instantiate(craft.Materials[i].ItemObject)), craft.Materials[i].Amount);
                materialSlots[i].SetActive(true);
            }
            else
            {
                slotsOnInterface[materialSlots[i]].UpdateSlot(new Item(), 0);

                materialSlots[i].SetActive(false);
            }
        }
        slotsOnInterface[resultSlot].UpdateSlot(new Item(Instantiate(craft.ResultObject.ItemObject)), craft.ResultObject.Amount);

        //resultSlot.GetComponentInChildren<UnityEngine.UI.Image>().sprite = craft.ResultObject.ItemObject.UIDisplaySprite;
        //resultSlot.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
        //resultSlot.GetComponentInChildren<TextMeshProUGUI>().text = craft.ResultObject.Amount == 1 ? "" : craft.ResultObject.Amount.ToString("n0");
        craftName.GetComponentInChildren<TextMeshProUGUI>().text = craft.name;
    }

    private void Update()
    {
        if(this.craftButtonCooldown > 0f)
            this.craftButtonCooldown -= Time.deltaTime;
    }

    private void OnCraftClick()
    {
        if (Input.GetMouseButtonDown(0) && this.craftButtonCooldown <= 0f)
        {
            this.craftButtonCooldown = 0.5f;
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
}