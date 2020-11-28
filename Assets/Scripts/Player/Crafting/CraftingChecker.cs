using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingChecker : MonoBehaviour
{
    [SerializeField]
    private CraftingRecipeDatabase craftingRecipeDatabase;
    private List<CraftingRecipeObject> availableRecipes = new List<CraftingRecipeObject>();

    public void CheckAvailableRecipes(InventoryObject currentInventory)
    {
        availableRecipes = new List<CraftingRecipeObject>();
        CraftingRecipeObject[] craftingRecipes = craftingRecipeDatabase.GetRecipes();
        for (int i = 0; i < craftingRecipes.Length; i++)
        {
            int itemsFound = 0;
            for (int n = 0; n < craftingRecipes[i].Materials.Count; n++)
            {
                int amount = 0;
                for (int j = 0; j < currentInventory.GetSlots.Length; j++)
                {
                    //Check if the material exists in the inventory
                    if(craftingRecipes[i].Materials[n].ItemObject.Data.ID == currentInventory.GetSlots[j].ID)
                    {
                        //add the amount from the inventory and check if we have enought for the material requirements
                        amount += currentInventory.GetSlots[j].Amount;
                        if(amount >= craftingRecipes[i].Materials[n].Amount)
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
                availableRecipes.Add(craftingRecipes[i]);
            }
                
        }
    }

    public List<CraftingRecipeObject> AvailableRecipes
    {
        get
        {
            return this.availableRecipes;
        }
    }

    public void CraftItem(InventoryObject currentInventory, CraftingRecipeObject recipe)
    {
        if (currentInventory.EmptySlotCount <= 0)
            return;

        List<Item> itemsToBeRemoved = new List<Item>();
        int foundItems = 0;

        for (int i = 0; i < recipe.Materials.Count; i++)
        {
            
            ItemAmount currentMaterial = recipe.Materials[i];
            int amountRemaining = currentMaterial.Amount;
            for (int j = 0; j < currentInventory.GetSlots.Length; j++)
            {
                
                Item currentItem = currentInventory.GetSlots[j].Item;
                if (currentMaterial.ItemObject.Data.ID == currentItem.ID)
                {
                    
                    itemsToBeRemoved.Add(currentItem);
                    if(currentItem.Amount >= amountRemaining)
                    {
                        amountRemaining = 0;
                        foundItems++;
                        break;
                    }
                    else
                    {
                        amountRemaining -= currentItem.Amount;
                    }
                }
            }
        }
        if(foundItems >= recipe.Materials.Count)
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
                        if(itemsToBeRemoved[j].Amount > amount)
                        {
                            currentInventory.RemoveItem(itemsToBeRemoved[j], amount);
                            itemsRemoved++;
                            break;
                        }
                        else
                        {
                            amount -= itemsToBeRemoved[j].Amount;
                            currentInventory.RemoveItem(itemsToBeRemoved[j]);
                            if (amount == 0)
                            {
                                itemsRemoved++;
                                break;
                            }
                                
                        }
                    }
                }
            }
            if (itemsRemoved == recipe.Materials.Count)
            {
                ItemObject newItemObj = Instantiate(recipe.ResultObject.ItemObject);
                Item newItem = new Item(newItemObj);
                currentInventory.AddItem(newItem, recipe.ResultObject.Amount);
            }
                
        }
    }
}