using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    private List<ItemOld> items = new List<ItemOld>();

    private void Awake()
    {
        BuildDatabase();
    }

    public ItemOld GetItem(int id)
    {
        return items.Find(item => item.ID == id);
    }

    public ItemOld GetItem(string itemName)
    {
        return items.Find(item => item.Title == itemName);
    }

    private void BuildDatabase()
    {
        items = new List<ItemOld>()
        {
            new Weapon(0, "Diamond Sword", "A sword made of diamond",
            new Dictionary<string, int>
            {
                {"Power", 15 }
            }),
            new Weapon(1, "Diamond Pick", "A pick made of diamond",
            new Dictionary<string, int>
            {
                {"Power", 5 },
                {"Mining", 15 }
            })
        };
    }
}
