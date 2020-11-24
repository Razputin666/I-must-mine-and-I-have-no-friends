using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandler : MonoBehaviour
{
    public MouseItem mouseItem = new MouseItem();
    [SerializeField]
    private InventoryObject inventory;

    public void OnTriggerEnter2D(Collider2D other)
    {
        GroundItem item = other.GetComponent<GroundItem>();

        if(item)
        {
            inventory.AddItem(new Item(item.Item), 1);
            Destroy(other.gameObject);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            inventory.Save();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            inventory.Load();
        }
    }

    private void OnApplicationQuit()
    {
        inventory.Container.Items = new InventorySlot[36] ;
    }
}