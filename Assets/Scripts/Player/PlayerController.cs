using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    LayerMask blockLayerMask;

    public enum PlayerStates
    {
        Idle, Mining, Normal, Building
    }

    public PlayerStates playerStates { get; private set; }

    #region PlayerValues    
    public int playerHP;

    public Rigidbody2D rb2d;        //Store a reference to the Rigidbody2D component required to use 2D Physics.
    public Vector3 worldPosition;
    public Vector3 mousePos;
    private float distanceFromPlayerx;
    private float distanceFromPlayery;
    #endregion

    public Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    private ItemController itemController;
    private ItemHandler itemHandler;
    public Transform item;
    [SerializeField]
    private DeathScreen deathScreen;
    private Camera camera;

    private JumpController jumpController;
   [SerializeField]private DeathScreen deathScreen;
    [SerializeField] private LevelGeneratorLayered mapSize;

    //Network
    [SerializeField]
    private TextMesh playerNameText;
    [SerializeField]
    private GameObject floatingInfo;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;
    [SyncVar(hook = nameof(OnActiveItemChanged))]
    private int activeItemID = -1;
    private int activeQuickslot = -1;

    Camera _camera;
    [SerializeField]
    Camera _secondCamera;

    private SceneScript sceneScript;

    private void Awake()
    {
        //allow all players to run this
        sceneScript = GameObject.FindObjectOfType<SceneScript>();
        item = gameObject.GetComponentInChildren<FaceMouse>().gameObject.transform.Find("ItemHeldInHand");

        itemHandler = GetComponent<ItemHandler>();
        if (!isLocalPlayer)
        {
            
        }
    }

    #region Network
    private void OnNameChanged(string _Old, string _New)
    {
        playerNameText.text = playerName;
    }
    private void OnActiveItemChanged(int _Old, int _New)
    {
        ItemObject itemObj = itemHandler.ItemDatabase.GetItemAt(_New);
        if (itemObj != null)
        {
            SetPlayerState(itemObj.ItemType);
            item.GetComponent<SpriteRenderer>().sprite = itemObj.UIDisplaySprite;
        }
        else
        {
            SetPlayerState(ITEM_TYPE.None);
            item.GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    public override void OnStartLocalPlayer()
    {
        if (camera == null)
            camera = Instantiate(Camera.main);

        Camera.main.GetComponentInParent<AudioListener>().enabled = false;
        camera.GetComponent<AudioListener>().enabled = true;
        camera.transform.SetParent(transform);
        camera.transform.localPosition = new Vector3(0, 0, -20);

        //_camera.transform.position = new Vector3(Mathf.Clamp(rb2d.transform.position.x, (0) + 26.7f, (mapSize.startPosition.x) - 26.7f), rb2d.transform.position.y, -1);

        //Camera.main.CopyFrom(camera);
        //floatingInfo.transform.localPosition = new Vector3(0.1f, 1f, 0f);
        //floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        string name = "Player" + Random.Range(100, 999);

        CmdSetupPlayer(name);

        //old
        //inventory = GetComponentInChildren<Inventory>();
        
        item.GetComponent<SpriteRenderer>().sprite = null;
        item.GetComponent<DefaultGun>().enabled = false;
        item.GetComponent<MiningController>().enabled = false;

        //Get and store a reference to the Rigidbody2D component so that we can access it.

        //facingRight = true;
        rb2d = GetComponent<Rigidbody2D>();
        //boxCollider2d = transform.GetComponent<BoxCollider2D>();
        //capsuleCollider2d = transform.GetComponent<CapsuleCollider2D>();
        
        playerStates = PlayerStates.Idle;

        boxCollider2d = transform.GetComponent<BoxCollider2D>();
        capsuleCollider2d = transform.GetComponent<CapsuleCollider2D>();
        jumpController = GetComponent<JumpController>();
        itemHandler = GetComponent<ItemHandler>();
        playerStates = PlayerStates.Idle;
        mapSize = GameObject.Find("LevelGeneration").GetComponent<LevelGeneratorLayered>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();

        StartCoroutine(CoroutineCoordinator());
    }

    #region Commands
    [Command]
    public void CmdActiveItemChanged(int itemID)
    {
        activeItemID = itemID;
    }

    [Command]
    public void CmdSetupPlayer(string name)
    {

        //player info sent to server, then server updates sync vars which handles it on all clients
        playerName = name;
        //sceneScript.statusText = $"{playerName} joined.";
        //sceneScript.canvasStatusText.enabled = true;
        StartCoroutine(FadeServerMessage());
    }
        

    [Command]
    public void CmdSendPlayerMessage()
    {
        if(sceneScript)
        {
            sceneScript.canvasStatusText.enabled = true;
            sceneScript.statusText = $"{playerName} says hello {Random.Range(10, 99)}";

            StartCoroutine(FadeServerMessage());
        }
    }

    #endregion

    //[ClientRpc]
    //private void UpdatePlayerState(int itemID)
    //{
    //    ItemDatabaseObject idat = itemHandler.ItemDatabase;
    //    ItemObject itemObj = idat.GetItemAt(itemID);
    //    if (itemObj != null)
    //    {
    //        SetPlayerState(itemObj.ItemType);
    //        item.GetComponent<SpriteRenderer>().sprite = itemObj.UIDisplaySprite;
    //    }
    //    else
    //    {
    //        SetPlayerState(ITEM_TYPE.None);
    //        item.GetComponent<SpriteRenderer>().sprite = null;
    //    }
    //}

    #endregion

    private IEnumerator FadeServerMessage()
    {
        if(sceneScript)
        {
            yield return new WaitForSeconds(5f);

            sceneScript.canvasStatusText.enabled = false;
        }
    }

    #region Updates
    [Client]
    private void Update()
    {
        if (!isLocalPlayer)
        {
            //if(camera)
            //    floatingInfo.transform.LookAt(camera.transform);
            return;
        }

        CheckQuickslotInput();
    }
    [Client]
    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        mousePos = Input.mousePosition;
        worldPosition = camera.ScreenToWorldPoint(mousePos);

        if (playerHP <= 0)
        {
            Die();
        }

    }
    #endregion
    public void UpdateActiveItem(int itemID)
    {
        CmdActiveItemChanged(itemID);
    }

    private void QuickslotActiveChanged(int index)
    {
        if (activeQuickslot != index)
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[index].ItemObject;
            int id = itemObj != null ? itemObj.Data.ID : -1;
            CmdActiveItemChanged(id);
            activeQuickslot = index;
        }
        else
        {
            CmdActiveItemChanged(-1);
            activeQuickslot = -1;
        }
    }

    [Client]
    private void CheckQuickslotInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            QuickslotActiveChanged(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QuickslotActiveChanged(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            QuickslotActiveChanged(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            QuickslotActiveChanged(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            QuickslotActiveChanged(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            QuickslotActiveChanged(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            QuickslotActiveChanged(6);
        }
    }

    [Client]
    private void SetPlayerState(ITEM_TYPE itemType)
    {
        switch (itemType)
        {
            case ITEM_TYPE.TileBlock:
                playerStates = PlayerStates.Building;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = false;
                break;
            case ITEM_TYPE.Weapon:
                playerStates = PlayerStates.Normal;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = true;
                break;
            case ITEM_TYPE.MiningLaser:
                playerStates = PlayerStates.Mining;
                item.GetComponent<MiningController>().enabled = true;
                item.GetComponent<DefaultGun>().enabled = false;
                break;
            default:
                playerStates = PlayerStates.Idle;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = false;
                break;
        }
    }

    [Client]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("EnemyWeapon"))
        {
            EnemyController enemy = collision.collider.gameObject.GetComponentInParent<EnemyController>();
            playerHP -= enemy.enemyStrength;

            if (collision.otherCollider.transform.position.x - collision.collider.transform.position.x > 0f)
            {
                rb2d.AddForceAtPosition(enemy.enemyKnockBack, transform.position);
            }

            else if (collision.otherCollider.transform.position.x - collision.collider.transform.position.x < 0f)
            {
                rb2d.AddForceAtPosition(-enemy.enemyKnockBack, transform.position);
            }
        }


    }

    #region GameLogic

    void Die()
    {
        //deathScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }


    IEnumerator CoroutineCoordinator()
    {
        while (true)
        {
            while (coroutineQueue.Count > 0)
            {
                yield return StartCoroutine(coroutineQueue.Dequeue());
            }
            yield return null;
        }
    }

    public int ActiveQuickslot
    {
        get
        {
            return activeQuickslot;
        }
    }

    
    #endregion
}