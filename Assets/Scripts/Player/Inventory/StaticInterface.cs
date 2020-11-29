using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StaticInterface : UserInterface
{
    [SerializeField]
    private GameObject[] slots;

    public override void CreateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
        //Loop through all the items in our Equipment database
        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            inventory.GetSlots[i].OnAfterUpdate += OnSlotUpdate;

            GameObject itemObject = slots[i];
            
            //Add events to our object inside our gameobjects array that is in the same order as our equipment database
            AddEvent(itemObject, EventTriggerType.PointerEnter, delegate { OnEnter(itemObject); });
            AddEvent(itemObject, EventTriggerType.PointerExit, delegate { OnExit(itemObject); });
            AddEvent(itemObject, EventTriggerType.BeginDrag, delegate { OnDragStart(itemObject); });
            AddEvent(itemObject, EventTriggerType.EndDrag, delegate { OnDragEnd(itemObject); });
            AddEvent(itemObject, EventTriggerType.Drag, delegate { OnDrag(itemObject); });

            inventory.GetSlots[i].slotObject = itemObject;

            //Link the database to the itemObject
            slotsOnInterface.Add(itemObject, inventory.GetSlots[i]);

            inventory.GetSlots[i].slotObject.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(0f, 70f / 255f, 168f / 255f, 190f / 255f);
        }
    }
}
