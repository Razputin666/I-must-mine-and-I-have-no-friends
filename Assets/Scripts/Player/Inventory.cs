﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //Items in the players inventory
    public List<Item> characterItems = new List<Item>();
    //Reference to our items
    public ItemDatabase itemDatabase;

    [SerializeField]
    private UIInventory inventoryUI;

    private int numberOfSlots;

    private void Start()
    {
        numberOfSlots = inventoryUI.GetMaxSlots();
        AddItem(0);
        AddItem(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            AddItem(0);
        }
            
    }

    public void AddItem(int id)
    {
        
            Item itemToAdd = itemDatabase.GetItem(id);
            characterItems.Add(itemToAdd);
            inventoryUI.AddNewItem(itemToAdd);
            Debug.Log("Added item: " + itemToAdd.Title);
       
        
    }
    public void AddItem(string itemName)
    {
        Item itemToAdd = itemDatabase.GetItem(itemName);
        characterItems.Add(itemToAdd);
        Debug.Log("Added item: " + itemToAdd.Title);
    }
    public Item CheckforItem(int id)
    {
        return characterItems.Find(item => item.ID == id);
    }

    public void RemoveItem(int id)
    {
        Item item = CheckforItem(id);
        if(item != null)
        {
            characterItems.Remove(item);
            inventoryUI.RemoveItem(item);
            Debug.Log("Item removed: " + item.Title);
        }
    }

    public Item GetItem(int slot)
    {
        Debug.Log(characterItems.Count);
        return characterItems[slot];
    }
}
