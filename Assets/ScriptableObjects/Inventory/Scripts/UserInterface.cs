using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public abstract class UserInterface : MonoBehaviour
{
    public ItemHandler player;
    [SerializeField]
    protected InventoryObject inventory;

    protected Dictionary<GameObject, InventorySlot> itemsDisplayed = new Dictionary<GameObject, InventorySlot>();
    //Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < inventory.Container.Items.Length; i++)
        {
            inventory.Container.Items[i].parent = this;
        }
        CreateSlots();
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSlots();
    }

    public void UpdateSlots()
    {
        foreach (KeyValuePair<GameObject, InventorySlot> slot in itemsDisplayed)
        {
            if (slot.Value.ID >= 0)
            {
                slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = inventory.ItemDatabase.GetItemAt(slot.Value.Item.id).uiDisplaySprite;
                slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1);
                slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = slot.Value.Amount == 1 ? "" : slot.Value.Amount.ToString("n0");
            }
            else //empty slot
            {
                slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = null;
                slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);
                slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }
    }

    public abstract void CreateSlots();

    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    public void OnEnter(GameObject obj)
    {
        // Whenever we enter an inventory slot set the mouseitem hoverObj to that object so we now which slot the mouse is over
        player.mouseItem.hoverObj = obj;
        if (itemsDisplayed.ContainsKey(obj)) //Check if it is an actual item
            player.mouseItem.hoverSlot = itemsDisplayed[obj];
    }
    public void OnExit(GameObject obj)
    {
        player.mouseItem.hoverObj = null;
        player.mouseItem.hoverSlot = null;
    }
    private void OnExitInterface(GameObject obj)
    {
        player.mouseItem.ui = null;
    }

    private void OnEnterInterface(GameObject obj)
    {
        player.mouseItem.ui = obj.GetComponent<UserInterface>();
    }

    public void OnDragStart(GameObject obj)
    {
        GameObject mouseObject = new GameObject();

        RectTransform rt = mouseObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);
        mouseObject.transform.SetParent(transform.parent);

        if (itemsDisplayed[obj].ID >= 0)
        {
            Image img = mouseObject.AddComponent<Image>();
            img.sprite = inventory.ItemDatabase.GetItemAt(itemsDisplayed[obj].ID).uiDisplaySprite;
            img.raycastTarget = false;
        }

        player.mouseItem.obj = mouseObject;
        player.mouseItem.item = itemsDisplayed[obj];
    }
    public void OnDragEnd(GameObject obj)
    {
        MouseItem itemOnMouse = player.mouseItem;
        InventorySlot mouseHoverItem = itemOnMouse.hoverSlot;
        GameObject mouseHoverObj = itemOnMouse.hoverObj;
        Dictionary<int, ItemObject> GetItemObject = inventory.ItemDatabase.GetItem;

        if(itemOnMouse.ui != null)
        {
            if (mouseHoverObj)
            {
                //Check if the item is allowed to be placed in the slot and if swapping an item from the equipment check if the swapped item is allowed to be equipped
                if (mouseHoverItem.CanPlaceInSlot(GetItemObject[itemsDisplayed[obj].ID]) &&
                    (mouseHoverItem.Item.id <= -1 || (mouseHoverItem.Item.id >= 0 && itemsDisplayed[obj].CanPlaceInSlot(GetItemObject[mouseHoverItem.Item.id]))))
                {
                    inventory.MoveItem(itemsDisplayed[obj], mouseHoverItem.parent.itemsDisplayed[mouseHoverObj]);
                }
            }
        }
        else
        {
            inventory.RemoveItem(itemsDisplayed[obj].Item);
        }
        Destroy(itemOnMouse.obj);
        itemOnMouse.item = null;
    }
    public void OnDrag(GameObject obj)
    {
        if (player.mouseItem.obj != null)
        {
            player.mouseItem.obj.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }


}

public class MouseItem
{
    public UserInterface ui;
    public GameObject obj;
    public InventorySlot item;
    public InventorySlot hoverSlot;
    public GameObject hoverObj;
}