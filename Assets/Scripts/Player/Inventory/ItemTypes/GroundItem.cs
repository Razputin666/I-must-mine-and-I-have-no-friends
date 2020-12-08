using UnityEngine;
using UnityEditor;

public class GroundItem : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    private ItemObject item;
    private float pickupCooldown = 2f;

    void Start()
    {
    }

    private void Update()
    {
        if (pickupCooldown >= 0f)
            pickupCooldown -= Time.deltaTime;
    }

    public void SetItemObject(ItemObject item, float cooldown = 2f)
    {
        this.item = item;
        if (item != null)
        {
            pickupCooldown = cooldown;
            GetComponentInChildren<SpriteRenderer>().sprite = item.UIDisplaySprite;
        }
    }

    public ItemObject Item
    {
        get 
        {
            return this.item;
        }
    }

    public float PickupTime
    {
        get
        {
            return this.pickupCooldown;
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
