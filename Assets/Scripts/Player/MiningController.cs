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
    [SerializeField] private TileMapManager tileMapManager;

    [SerializeField] private Tilemap tilemap;

    public Transform endOfGun;
    private Dictionary<Vector3Int, float> something = new Dictionary<Vector3Int, float>();

    // Start is called before the first frame update
    void Start()
    {
        tileMapManager = GameObject.FindWithTag("GameManager").GetComponent<TileMapManager>();
        endOfGun = transform.Find("EndOfGun");
      //  player = GetComponentInParent<FaceMouse>().GetComponentInParent<PlayerController>();
      //  tileMapChecker = gameObject.GetComponentInParent<FaceMouse>().gameObject.GetComponentInParent<PlayerController>().gameObject.GetComponentInChildren<TileMapChecker>();
    }


    public void Mine(Vector3Int blockToMine, float miningStr)
    {
        if(!coolDownSystem.IsOnCoolDown(id))
        {
            RaycastHit2D tileMapCheck = Physics2D.Linecast(transform.position, new Vector2(blockToMine.x, blockToMine.y));
            if(tileMapCheck.collider != null && tileMapCheck.collider.gameObject.CompareTag("TileMap"))
            {
                tilemap = tileMapCheck.collider.attachedRigidbody.GetComponent<Tilemap>();
            }
            Vector3Int blockInLocal = tilemap.WorldToCell(blockToMine);
            float blockStr = 0;
            //string checkForPlant = tileMapManager.BlockTypeGet(new Vector3Int(blockInLocal.x, blockInLocal.y + 1, 0), tilemap);



            if (!something.TryGetValue(blockInLocal, out blockStr))
            {
                
                blockStr = tileMapManager.BlockStrengthGet(blockInLocal, tilemap);
                if(blockStr >= 0)
                {
                    blockStr -= miningStr * Time.deltaTime;
                    something.Add(blockInLocal, blockStr);
                }
            }
            else
            {
                blockStr = something[blockInLocal];
                blockStr -= miningStr * Time.deltaTime;
                something[blockInLocal] = blockStr;
            }

            if(blockStr <= 0)
            {
                tilemap.SetTile(blockInLocal, null);
                //if (checkForPlant == "Plant")
                //{
                //    Debug.Log(checkForPlant);
                //    tilemap.SetTile(new Vector3Int(blockInLocal.x, blockInLocal.y + 1, 0), null);
                //}
                something.Remove(blockInLocal);
                coolDownSystem.PutOnCoolDown(this);
            }

        }
    }



    public int Id => id;

    public float CoolDownDuration => coolDownDuration;



}
