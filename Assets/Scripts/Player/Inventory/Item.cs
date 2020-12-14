using UnityEngine;

[System.Serializable]
public class Item
{
    [SerializeField]
    private string name;
    [SerializeField]
    private int id = -1;
    [SerializeField]
    private int amount;
    [SerializeField]
    private string type;
    //public ItemBuff[] buffs;
    private ItemObject itemObject;
    public Item()
    {
        name = "";
        id = -1;
        amount = 0;
        type = "";
        itemObject = null;
    }
    public Item(ItemObject item)
    {
        itemObject = item;
        name = item.Data.Name;
        id = item.Data.id;
        type = item.Data.type;
        amount = item.Data.amount;
        //buffs = new ItemBuff[item.buffs.Length];
        //for(int i = 0; i < buffs.Length; i++)
        //{
        //    buffs[i] = new ItemBuff(item.buffs[i].min, item.buffs[i].max);
        //}
    }

    public string Name
    {
        get
        {
            return this.name;
        }
    }
    public string Type
    {
        get
        {
            return this.type;
        }
    }

    public int ID
    {
        get
        {
            return this.id;
        }
        set
        {
            this.id = value;
        }
    }

    public int Amount
    {
        get
        {
            return this.amount;
        }
        set
        {
            this.amount = value;
        }
    }
    public ItemObject ItemObject
    {
        get
        {
            return this.itemObject;
        }
    }
}
//[System.Serializable]
//public class ItemBuff
//{
//    public ATTRIBUTES attributes;
//    public int value;
//    public int min;
//    public int max;

//    public ItemBuff(int min, int max)
//    {
//        this.min = min;
//        this.max = max;
//        GenerateValue();
//    }

//    public void GenerateValue()
//    {
//        value = UnityEngine.Random.Range(min, max);
//    }
//}
