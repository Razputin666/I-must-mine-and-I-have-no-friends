﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    private List<UIItem> uIItem = new List<UIItem>();
    [SerializeField]
    private GameObject inventorySlotPrefab;
    [SerializeField]
    private Transform slotPanel;
    [SerializeField]
    private int numberOfSlots = 16; 

    private void Awake()
    {
        for(int i = 0; i < numberOfSlots; i++)
        {
            GameObject instance = Instantiate(inventorySlotPrefab);
            instance.transform.SetParent(slotPanel);
            uIItem.Add(instance.GetComponentInChildren<UIItem>());
        }
    }

    public void UpdateSlot(int slot, Item item)
    {
        uIItem[slot].UpdateItem(item);
    }

    public void AddNewItem(Item item)
    {
        UpdateSlot(uIItem.FindIndex(i => i.Item == null), item);
    }
    public void RemoveItem(Item item)
    {
        UpdateSlot(uIItem.FindIndex(i => i.Item == null), null);
    }
    public int GetMaxSlots()
    {
        return numberOfSlots;
    }

    public Item GetItem(int slot)
    {
        return uIItem[slot].Item;
    }
}
