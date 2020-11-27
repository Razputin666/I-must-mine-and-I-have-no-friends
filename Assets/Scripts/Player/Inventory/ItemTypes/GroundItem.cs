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
    public ItemObject Item
    {
        get 
        {
            return this.item;
        }
        set
        {
            this.item = value;
            if(item != null)
            {
                pickupCooldown = 2f;
                GetComponentInChildren<SpriteRenderer>().sprite = item.UIDisplaySprite;
            }
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
