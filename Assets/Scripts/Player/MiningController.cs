using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour, HasCoolDownInterFace
{
    [SerializeField] private ItemDatabaseObject itemDatabase;
    [SerializeField] private int id = 2;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private CoolDownSystem coolDownSystem;
    [SerializeField] private Transform[] points;

    public Transform endOfGun;

    // Start is called before the first frame update
    void Start()
    {
        
        endOfGun = transform.Find("EndOfGun");
      //  player = GetComponentInParent<FaceMouse>().GetComponentInParent<PlayerController>();
      //  tileMapChecker = gameObject.GetComponentInParent<FaceMouse>().gameObject.GetComponentInParent<PlayerController>().gameObject.GetComponentInChildren<TileMapChecker>();
    }

    private void OnEnable()
    {
        if(itemDatabase == null)
            itemDatabase = GetComponent<ItemDatabaseObject>();
    }


    public bool Mine(Vector3Int blockToMine, Tilemap currentTileMap)
    {
        if(!coolDownSystem.IsOnCoolDown(id))
        {
            if(currentTileMap.GetTile(blockToMine) != null)
            {
                ItemObject itemObj = itemDatabase.GetItemAt(8);
                if (itemObj != null)
                {
                    ItemObject newItemObj = Instantiate(itemObj);
                    SpawnManager.SpawnItemAt(blockToMine, newItemObj);
                }
                else
                {
                    Debug.Log("null itemObj");
                }
                currentTileMap.SetTile(blockToMine, null);
                coolDownSystem.PutOnCoolDown(this);
                return true;
            }
            coolDownSystem.PutOnCoolDown(this);
            return false;
        }
        return false;
    }


    public int Id => id;

    public float CoolDownDuration => coolDownDuration;
}
