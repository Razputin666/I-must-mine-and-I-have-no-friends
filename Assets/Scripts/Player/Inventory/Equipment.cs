using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    enum EQUIPMENT_SLOT
    {
        HEAD, BODY, LEG, BOOTS,
        GEAR1, GEAR2, GEAR3, GEAR4
        //TRINKET1, TRINKET2, TRINKET3, TRINKET4
    }

    [SerializeField]
    private UIEquipment equipmentUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            equipmentUI.gameObject.SetActive(!equipmentUI.gameObject.activeSelf);
        }
    }

    public void EquipItem(int slot, Item item)
    {
        
    }
}
