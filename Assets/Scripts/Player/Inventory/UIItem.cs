using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private ItemOld item;
    private Image spriteImage;
    private UIItem selectedItem;
    private Tooltip tooltip;

    private void Awake()
    {
        spriteImage = GetComponent<Image>();
        UpdateItem(null);
        selectedItem = GameObject.Find("SelectedItem").GetComponent<UIItem>();

        tooltip = GameObject.Find("Tooltip").GetComponent<Tooltip>();
    }

    public void UpdateItem(ItemOld item)
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
                ItemOld clone;
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
                //if there was no previous selected item grab the clicked one and clear the slot it was on
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(this.item != null)
        {
            tooltip.GenerateTooltip(this.item);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.gameObject.SetActive(false);
    }

    public ItemOld Item
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
