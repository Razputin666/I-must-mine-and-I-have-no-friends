using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Database", menuName = "Inventory System/Crafting/Database")]
public class CraftingRecipeDatabase : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField]
    private CraftingRecipeObject[] recipeObject;

    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for (int i = 0; i < recipeObject.Length; i++)
        {
            if (recipeObject[i].ID != i)
                recipeObject[i].ID = i;
        }
    }
    public void OnAfterDeserialize()
    {
        UpdateID();
    }

    public void OnBeforeSerialize()
    {

    }

    public CraftingRecipeObject GetRecipeAt(int id)
    {
        return this.recipeObject[id];
    }

    public CraftingRecipeObject[] GetRecipes()
    {
        return this.recipeObject;
    }
}