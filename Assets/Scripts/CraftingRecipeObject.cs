using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Inventory System/Crafting/Recipe")]
public class CraftingRecipeObject : ScriptableObject
{
    [SerializeField]
    private ItemAmount resultObject;

    [SerializeField]
    private List<ItemAmount> materials;

    [SerializeField]
    private int iD;

    public ItemAmount ResultObject
    {
        get
        {
            return this.resultObject;
        }
    }

    public List<ItemAmount> Materials
    { 
        get
        {
            return this.materials;
        } 
    }

    public int ID
    {
        get
        {
            return this.iD;
        }
        set
        {
            this.iD = value;
        }
    }
}
