using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Items/Database")]
public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] 
    private ItemObject[] items;
    //Either use this dictionary or a nested for loop in OnAfterDesirialize depending on memory vs performance
    private Dictionary<int, ItemObject> getItem = new Dictionary<int, ItemObject>();

    public ItemObject GetItemAt(int id)
    {
        return this.getItem[id];
    }

    public void OnAfterDeserialize()
    {
        for(int i = 0; i < items.Length; i++)
        {
            items[i].itemID = i;
            getItem.Add(i, items[i]);
        }
    }

    public void OnBeforeSerialize()
    {
        getItem = new Dictionary<int, ItemObject>();

    }

    public Dictionary<int, ItemObject> GetItem
    {
        get
        {
            return this.getItem;
        }
    }
}