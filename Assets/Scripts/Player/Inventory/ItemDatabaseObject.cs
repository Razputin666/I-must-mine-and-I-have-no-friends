using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Items/Database")]
public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] 
    private ItemObject[] items;

    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].Data.ID != i)
                items[i].Data.ID = i;
        }
    }
    public void OnAfterDeserialize()
    {
        UpdateID();
    }

    public void OnBeforeSerialize()
    {
        
    }

    public ItemObject GetItemAt(int id)
    {
       return this.items[id];
    }
}