using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingChecker : MonoBehaviour
{
    [SerializeField]
    private CraftingRecipeDatabase craftingRecipeDatabase;
    private List<CraftingRecipeObject> craftableItems = new List<CraftingRecipeObject>();

    public void CheckAvailableRecipes(InventoryObject currentInventory)
    {
        craftableItems = new List<CraftingRecipeObject>();
        CraftingRecipeObject[] craftingRecipes = craftingRecipeDatabase.GetRecipes();
        for (int i = 0; i < craftingRecipes.Length; i++) // Loop through all recipes 
        {
            int itemsFound = 0;
            for (int n = 0; n < craftingRecipes[i].Materials.Count; n++) // Loop through the required materials.
            {
                int amount = 0;
                ItemAmount currentMaterial = craftingRecipes[i].Materials[n];
                for (int j = 0; j < currentInventory.GetSlots.Length; j++) //loop through our inventory.
                {
                    //Check if the material exists in the inventory
                    if(currentMaterial.ItemObject.Data.ID == currentInventory.GetSlots[j].ID)
                    {
                        //add the amount from the inventory and check if we have enought for the material requirements
                        amount += currentInventory.GetSlots[j].Amount;
                        if(amount >= currentMaterial.Amount)
                        {
                            itemsFound++;
                            break;
                        } 
                    }
                }
            }
            //check if we found enough items in our inventory to satisfy the material requirements.
            if (itemsFound == craftingRecipes[i].Materials.Count)
            {
                //Add the recipe to our list of craftable items
                craftableItems.Add(craftingRecipes[i]);
            }
        }
    }

    public List<CraftingRecipeObject> CraftableItems
    {
        get
        {
            return this.craftableItems;
        }
    }

    public CraftingRecipeDatabase CraftingDatabase
    {
        get
        {
            return this.craftingRecipeDatabase;
        }
    }

    public void CraftItem(InventoryObject currentInventory, CraftingRecipeObject recipe)
    {
        if (currentInventory.EmptySlotCount <= 0)
            return;

        List<Item> itemsToBeRemoved = new List<Item>();
        int foundItems = 0;

        for (int i = 0; i < recipe.Materials.Count; i++)// Loop through the required materials.
        {
            ItemAmount currentMaterial = recipe.Materials[i];
            int amountRemaining = currentMaterial.Amount;
            for (int j = 0; j < currentInventory.GetSlots.Length; j++) // Loop through our inventory.
            {
                
                Item currentItem = currentInventory.GetSlots[j].Item;
                //Check if the material exists in the inventory
                if (currentMaterial.ItemObject.Data.ID == currentItem.ID)
                {
                    //Add it to our list of items that we can remove for the recipe
                    itemsToBeRemoved.Add(currentItem);
                    if(currentItem.Amount >= amountRemaining) //Check if the current item has enought stacks to satisfy the requirements
                    {
                        amountRemaining = 0;
                        foundItems++; //increas the found item integer and check the next material
                        break;
                    }
                    else //If we still need more of that material update the amount we have gotten so far and check for more stacks
                    {
                        amountRemaining -= currentItem.Amount;
                    }
                }
            }
        }
        if(foundItems >= recipe.Materials.Count)//If we found enough items to craft
        {
            int itemsRemoved = 0;
            //remove items and if amount of an item is less than mat req  remove item and then remove the remaining amount from the next of the same type
            for (int i = 0; i < recipe.Materials.Count; i++)
            {
                int amount = recipe.Materials[i].Amount;
                for (int j = 0; j < itemsToBeRemoved.Count; j++)
                {
                    if(recipe.Materials[i].ItemObject.Data.ID == itemsToBeRemoved[j].ID)
                    {
                        //If this items has enought stacks(Amount) only remove that amount from our inventory and check the next material
                        if (itemsToBeRemoved[j].Amount > amount) 
                        {
                            currentInventory.RemoveItem(itemsToBeRemoved[j], amount);
                            itemsRemoved++;
                            break;
                        }
                        else // if that item amount is less then what we need then remove that item and check for more items of the same type
                        {
                            amount -= itemsToBeRemoved[j].Amount;
                            currentInventory.RemoveItem(itemsToBeRemoved[j]);
                            if (amount == 0)//we we found enough items of that type then check the next material
                            {
                                itemsRemoved++;
                                break;
                            }
                                
                        }
                    }
                }
            }
            if (itemsRemoved == recipe.Materials.Count)//if we removed all the materials then add the crafted item to our inventory
            {
                ItemObject newItemObj = Instantiate(recipe.ResultObject.ItemObject);
                Item newItem = new Item(newItemObj);
                currentInventory.AddItem(newItem, recipe.ResultObject.Amount);
            }
            else
                Debug.LogError("Somthing went wrong with our crafting, MISSING Mats");
        }
    }
}