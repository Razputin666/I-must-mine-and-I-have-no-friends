using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class UserInterface : MonoBehaviour
{
    [SerializeField]
    protected InventoryObject inventory;

    protected Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
    //Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            inventory.GetSlots[i].parent = this;
            inventory.GetSlots[i].OnAfterUpdate += OnSlotUpdate;
        }
        CreateSlots();
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
    }

    private void OnSlotUpdate(InventorySlot slot)
    {
        if (slot.Item.ID >= 0)
        {
            slot.slotObject.transform.GetChild(0).GetComponentInChildren<Image>().sprite = slot.ItemObject.UIDisplaySprite;
            slot.slotObject.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1);
            slot.slotObject.GetComponentInChildren<TextMeshProUGUI>().text = slot.Amount == 1 ? "" : slot.Amount.ToString("n0");
        }
        else //empty slot
        {
            slot.slotObject.transform.GetChild(0).GetComponentInChildren<Image>().sprite = null;
            slot.slotObject.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);
            slot.slotObject.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }

    public abstract void CreateSlots();

    public GameObject CreateTempObject(GameObject obj)
    {
        GameObject tempObject = null;
        //if the item exists on out inventory we are trying to drag
        if(slotsOnInterface[obj].Item.ID >= 0)
        {
            tempObject = new GameObject();
            RectTransform rt = tempObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 50);
            tempObject.transform.SetParent(transform.parent);

            Image img = tempObject.AddComponent<Image>();
            img.sprite = slotsOnInterface[obj].ItemObject.UIDisplaySprite;
            img.raycastTarget = false;
        }

        return tempObject;
    }

    #region Events
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
        MouseData.slotHoveredOver = obj;
    }
    public void OnExit(GameObject obj)
    {
        MouseData.slotHoveredOver = null;
    }
    
    private void OnEnterInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = obj.GetComponent<UserInterface>();
    }

    private void OnExitInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = null;
    }

    public void OnDragStart(GameObject obj)
    {
        MouseData.tempItemBeingDragged = CreateTempObject(obj);
    }
    public void OnDragEnd(GameObject obj)
    {
        Destroy(MouseData.tempItemBeingDragged);

        if(MouseData.interfaceMouseIsOver == null)
        {
            ItemObject newItem = slotsOnInterface[obj].ItemObject;
            newItem.Data.Amount = slotsOnInterface[obj].Item.Amount;
            Debug.Log(newItem.Data.Amount);
            SpawnManager.SpawnItemAt(Camera.main.ScreenToWorldPoint(Input.mousePosition), newItem);
            slotsOnInterface[obj].RemoveItem();
            return;
        }
        if(MouseData.slotHoveredOver != null)
        {
            InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
            inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
        }
    }
    public void OnDrag(GameObject obj)
    {
        if (MouseData.tempItemBeingDragged != null)
            MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
    }
    #endregion

    public InventoryObject Inventory
    {
        get
        {
            return this.inventory;
        }
    }
}
