using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIItem : MonoBehaviour, IPointerClickHandler
{
    private Item item;
    private Image spriteImage;
    private UIItem selectedItem;

    private void Awake()
    {
        spriteImage = GetComponent<Image>();
        UpdateItem(null);
        selectedItem = GameObject.Find("SelectedItem").GetComponent<UIItem>();
    }

    public void UpdateItem(Item item)
    {
        this.item = item;

        if(this.item != null)
        {
            spriteImage.color = Color.white;
            spriteImage.sprite = this.item.Icon;
        }
        else
        {
            spriteImage.color = Color.clear;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(this.item != null)
        {
            if(selectedItem.item != null)
            {
                Item clone;
                if(selectedItem.item.GetType() == typeof(Weapon))
                {
                    clone = new Weapon((Weapon)selectedItem.item);
                    //Grab the item we clicked and put it in selectedItem
                    selectedItem.UpdateItem(this.item);
                    //Save the item we dragged inside the inventory
                    UpdateItem(clone);
                }
            }
            else
            {
                //if there wa sno previous selected item grab the clicked one and clear the slot it was on
                selectedItem.UpdateItem(this.item);
                UpdateItem(null);
            }
        }
        else if(selectedItem.item != null) //if there was no item in the inventory and we had an item selected
        {
            //Drop the item in the inventory again
            UpdateItem(selectedItem.item);
            selectedItem.UpdateItem(null);
        }
    }

    public Item Item
    {
        get
        {
            return this.item;
        }

        set
        {
            this.item = value;
        }
    }
}
