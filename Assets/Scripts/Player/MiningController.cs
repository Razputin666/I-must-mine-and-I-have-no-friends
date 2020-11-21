using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour, HasCoolDownInterFace
{

    [SerializeField] private int id = 2;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private CoolDownSystem coolDownSystem;
    [SerializeField] private Transform[] points;
    [SerializeField] private LineController line;

    PlayerController player;
    TileMapChecker tileMapChecker;


    Vector3 worldPosition;
    private Tilemap targetedBlock;

    Vector3Int targetBlockIntPos;

    float timer;
    public Transform endOfGun;

    // Start is called before the first frame update
    void Start()
    {
        endOfGun = transform.Find("EndOfGun");
        line = GetComponent<LineController>();
        player = GetComponentInParent<FaceMouse>().GetComponentInParent<PlayerController>();
        tileMapChecker = gameObject.GetComponentInParent<FaceMouse>().gameObject.GetComponentInParent<PlayerController>().gameObject.GetComponentInChildren<TileMapChecker>();
    }

    void OnEnable()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        targetBlockIntPos = Vector3Int.FloorToInt(player.worldPosition);
        targetBlockIntPos.z = 0;
        Vector3Int playerIntPos = Vector3Int.FloorToInt(transform.position);
        


         TargetedBlock = tileMapChecker.currentTilemap;
         Vector3Int distanceFromPlayer = targetBlockIntPos - playerIntPos;

         if (coolDownSystem.IsOnCoolDown(id))
         {
             return;
         }

         if (Input.GetMouseButton(0) && distanceFromPlayer.x > -5 && distanceFromPlayer.x < 5 && distanceFromPlayer.y > -5 && distanceFromPlayer.y < 5)
         {
             TargetedBlock.SetTile(targetBlockIntPos, null);
             line.enabled = true;
             coolDownSystem.PutOnCoolDown(this);
         }
                
          
    }


public Tilemap TargetedBlock
    {
        get
        {
            return targetedBlock;

        }

        set
        {
            targetedBlock = value;
        }
    }

    public int Id => id;

    public float CoolDownDuration => coolDownDuration;
}
