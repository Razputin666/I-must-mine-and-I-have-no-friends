using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Items/Database")]
public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] 
    private ItemObject[] itemObjects;

    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for (int i = 0; i < itemObjects.Length; i++)
        {
            if (itemObjects[i].Data.ID != i)
                itemObjects[i].Data.ID = i;
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
        if(id >= 0 && id < itemObjects.Length)
            return this.itemObjects[id];
        return null;
    }
}