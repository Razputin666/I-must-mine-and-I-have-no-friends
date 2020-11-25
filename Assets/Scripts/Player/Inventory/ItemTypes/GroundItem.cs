using UnityEngine;
using UnityEditor;

public class GroundItem : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    private ItemObject item;

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
                GetComponentInChildren<SpriteRenderer>().sprite = item.UIDisplaySprite;
                Debug.Log(GetComponentInChildren<SpriteRenderer>().sprite);
            }
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
