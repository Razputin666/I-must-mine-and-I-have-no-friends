using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicInterface : UserInterface
{
    public GameObject inventoryPrefab;

    [SerializeField]
    private int X_START;
    [SerializeField]
    private int Y_START;
    [SerializeField]
    private int X_SPACE_BETWEEN_ITEM;
    [SerializeField]
    private int NUMBER_OF_COLUMN;
    [SerializeField]
    private int Y_SPACE_BETWEEN_ITEM;

    public override void CreateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            GameObject itemObject = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
            itemObject.GetComponent<RectTransform>().localPosition = GetPosition(i);

            inventory.GetSlots[i].slotObject = itemObject;

            slotsOnInterface.Add(itemObject, inventory.GetSlots[i]);
            //Add events
            AddEvent(itemObject, EventTriggerType.PointerDown, delegate { OnPointerDown(itemObject); });
            AddEvent(itemObject, EventTriggerType.PointerEnter, delegate { OnEnter(itemObject); });
            AddEvent(itemObject, EventTriggerType.PointerExit, delegate { OnExit(itemObject); });
            AddEvent(itemObject, EventTriggerType.BeginDrag, delegate { OnDragStart(itemObject); });
            AddEvent(itemObject, EventTriggerType.EndDrag, delegate { OnDragEnd(itemObject); });
            AddEvent(itemObject, EventTriggerType.Drag, delegate { OnDrag(itemObject); });
        }
    }

    private void OnPointerDown(GameObject itemObject)
    {
        //If we hover over a slot and press the right mousebutton
        if(MouseData.slotHoveredOver != null && Input.GetMouseButton(1))
        {
            inventory.SplitItem(slotsOnInterface[MouseData.slotHoveredOver]);
        }
    }


    private Vector3 GetPosition(int index)
    {
        return new Vector3(X_START + (X_SPACE_BETWEEN_ITEM * (index % NUMBER_OF_COLUMN)), Y_START + (-Y_SPACE_BETWEEN_ITEM * (index / NUMBER_OF_COLUMN)), 0f);
    }
}
