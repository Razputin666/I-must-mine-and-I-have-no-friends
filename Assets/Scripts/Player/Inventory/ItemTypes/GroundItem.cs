using UnityEngine;
using UnityEditor;
using Mirror;

public class GroundItem : NetworkBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private ItemObject item;

    public float PickupTime { get; private set; } = 2f;

    struct ItemData
    {
        public int itemID;
        public int itemAmount;
    }

    [SyncVar(hook = nameof(OnItemUpdate))]
    private ItemData itemData;

    private void Update()
    {
        if (PickupTime >= 0f)
            PickupTime -= Time.deltaTime;
    }

    public void SetItemObject(ItemObject item, float cooldown = 2f)
    {
        this.item = item;
        if (item != null)
        {
            PickupTime = cooldown;
            GetComponentInChildren<SpriteRenderer>().sprite = item.UIDisplaySprite;

            itemData = new ItemData
            {
                itemID = item.Data.ID,
                itemAmount = item.Data.Amount
            };
        }
    }
    private void OnItemUpdate(ItemData _old, ItemData _new)
    {
        GameObject parentObject = GameObject.Find("ItemSpawner");

        transform.SetParent(parentObject.transform, true);

        ItemDatabaseObject db = Resources.Load("ScriptableObjects/ItemDatabase") as ItemDatabaseObject;

        ItemObject item = db.GetItemAt(_new.itemID);
        item.Data.Amount = _new.itemAmount;

        this.item = item;

        GetComponentInChildren<SpriteRenderer>().sprite = item.UIDisplaySprite;
    }

    public ItemObject Item
    {
        get 
        {
            return this.item;
        }
    }

    public void OnAfterDeserialize()
    {
        
    }

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if(item != null)
            GetComponentInChildren<SpriteRenderer>().sprite = item.UIDisplaySprite;

        EditorUtility.SetDirty(GetComponentInChildren<SpriteRenderer>());
#endif
    }
}
