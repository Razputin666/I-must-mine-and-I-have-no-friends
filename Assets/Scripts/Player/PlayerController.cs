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
    public Vector3 mousePosInWorld;
    //public Vector3 mousePos;
    //private float distanceFromPlayerx;
    //private float distanceFromPlayery;
    [SerializeField]
    private int miningStrength;
    #endregion

    public Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    public Transform item;
    [SerializeField]
    private DeathScreen deathScreen;
    private Camera camera;

    private JumpController jumpController;
    [SerializeField] 
    private LevelGeneratorLayered mapSize;

    //Network
    [SerializeField]
    private TextMesh playerNameText;
    [SerializeField]
    private GameObject floatingInfo;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;
    [SyncVar(hook = nameof(OnActiveItemChanged))]
    private int activeItemID = -1;
    [SerializeField]
    Camera _secondCamera;

    private SceneScript sceneScript;
    [SerializeField]
    private float itemPickupRange;
    private float itemPickupCooldown;
    private const float itemPickupCooldownDefault = 0.5f;
    [SyncVar]
    private bool isReady;
    public bool IsReady { get; private set; }
    public ItemHandler ItemHandler { get; private set; }
    public int ActiveQuickslot { get; private set; } = -1;
    private void Awake()
    {
        //allow all players to run this
        sceneScript = GameObject.FindObjectOfType<SceneScript>();
        item = gameObject.transform.Find("Gubb_arm").gameObject.transform.Find("ItemHeldInHand");

        ItemHandler = GetComponent<ItemHandler>();
        IsReady = false;
        
        itemPickupCooldown = itemPickupCooldownDefault;

        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (IsReady)
        {
            //if (!isLocalPlayer)
            //{
            //    //if(camera)
            //    //    floatingInfo.transform.LookAt(camera.transform);
            //    return;
            //}
            if(isLocalPlayer)
                CheckQuickslotInput();

            if(isServer)
            {
                itemPickupCooldown -= Time.deltaTime;

                if (itemPickupCooldown <= 0f)
                {
                    itemPickupCooldown = itemPickupCooldownDefault;

                    CheckItemCollision();
                }
            }
        }
    }

    #region Network

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

    [Command(ignoreAuthority = true)]
    private void CmdRemoveGroundItem(GameObject obj)
    {
        NetworkServer.Destroy(obj);
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

    [Command]
    private void CmdSendMousePos(Vector3 mousePos)
    {
        mousePosInWorld = mousePos;
    }

    #endregion

    #region Server

    public override void OnStartServer()
    {
        IsReady = true;

        rb2d = GetComponent<Rigidbody2D>();
    }
    [Server]
    private void CheckItemCollision()
    {
        Debug.Log("Checking collision");
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, itemPickupRange);
        
        foreach (Collider2D collider2D in colliderArray)
        {
            if (collider2D.TryGetComponent<GroundItem>(out GroundItem groundItem))
            {
                Debug.Log("Collision");
                Item newItem = new Item(groundItem.Item);

                RpcAddItemToPlayer(GetComponent<NetworkIdentity>().connectionToClient, newItem.ID, newItem.Amount);
                NetworkServer.Destroy(collider2D.gameObject);
            }
        }
    }

    [Server]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsReady)
        {
            if (collision.collider.CompareTag("EnemyWeapon"))
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
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawSphere(transform.position, itemPickupRange);
    //}
    #endregion

    #region Callbacks
    private void OnNameChanged(string _Old, string _New)
    {
        playerNameText.text = playerName;
    }
    private void OnActiveItemChanged(int _Old, int _New)
    {
        ItemObject itemObj = ItemHandler.ItemDatabase.GetItemAt(_New);
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
    #endregion

    #region Client
    public override void OnStartLocalPlayer()
    {
        if (camera == null)
            camera = Instantiate(Camera.main, transform);

        Camera.main.GetComponentInParent<AudioListener>().enabled = false;
        camera.GetComponent<AudioListener>().enabled = true;
        //camera.transform.SetParent(transform);
        camera.transform.localPosition = new Vector3(0, 0, -20);

        //_camera.transform.position = new Vector3(Mathf.Clamp(rb2d.transform.position.x, (0) + 26.7f, (mapSize.startPosition.x) - 26.7f), rb2d.transform.position.y, -1);

        //Camera.main.CopyFrom(camera);
        //floatingInfo.transform.localPosition = new Vector3(0.1f, 1f, 0f);
        //floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        string name = "Player" + Random.Range(100, 999);

        //CmdSetupPlayer(name);

        item.GetComponent<SpriteRenderer>().sprite = null;
        item.GetComponent<DefaultGun>().enabled = false;
        //GetComponent<MiningController>().enabled = false;

        playerStates = PlayerStates.Idle;

        jumpController = GetComponent<JumpController>();
        ItemHandler = GetComponent<ItemHandler>();

        //mapSize = GameObject.Find("LevelGeneration").GetComponent<LevelGeneratorLayered>();

        StartCoroutine(CoroutineCoordinator());
    }

    [TargetRpc]
    public void RpcAddItemToPlayer(NetworkConnection conn, int itemID, int itemAmount)
    {
        Debug.Log("Adding item. ID: " + itemID + " amount: " + itemAmount);
        ItemObject item = Instantiate(GetInventoryObject.ItemDatabase.GetItemAt(itemID));
        item.Data.Amount = itemAmount;

        Item newItem = new Item(item);
        if(!ItemHandler.QuickSlots.AddItem(newItem, newItem.Amount))
        {
            if (!ItemHandler.Inventory.AddItem(newItem, newItem.Amount))
                return; //spawn back item to the world
        }
    }

    [TargetRpc]
    public void RpcRemoveItemFromQuickslot(NetworkConnection conn, int amount)
    {
        ItemHandler.QuickSlots.GetSlots[ActiveQuickslot].RemoveAmount(amount);
    }

    [Client]
    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    void FixedUpdate()
    {
        if (IsReady)
        {
            if (!isLocalPlayer)
                return;

            Vector3 mousePosInWorld = camera.ScreenToWorldPoint(Input.mousePosition);
            mousePosInWorld.z = 0;
            CmdSendMousePos(mousePosInWorld);

            if (playerHP <= 0)
            {
                Die();
            }
        }
    }
    [Client]
    public void UpdateActiveItem(int itemID)
    {
        CmdActiveItemChanged(itemID);
    }

    [Client]
    private void QuickslotActiveChanged(int index)
    {
        if (ActiveQuickslot != index)
        {
            ItemObject itemObj = ItemHandler.QuickSlots.Container.InventorySlot[index].ItemObject;

            int id = itemObj != null ? itemObj.Data.ID : -1;
            CmdActiveItemChanged(id);
            ActiveQuickslot = index;
        }
        else
        {
            CmdActiveItemChanged(-1);
            ActiveQuickslot = -1;
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
                //GetComponent<MiningController>().enabled = false;
                //item.GetComponent<DefaultGun>().enabled = false;
                break;
            case ITEM_TYPE.Weapon:
                playerStates = PlayerStates.Normal;
                //GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = true;
                break;
            case ITEM_TYPE.MiningLaser:
                playerStates = PlayerStates.Mining;
                //GetComponent<MiningController>().enabled = true;
                //item.GetComponent<DefaultGun>().enabled = false;
                break;
            default:
                playerStates = PlayerStates.Idle;
                //GetComponent<MiningController>().enabled = false;
                //item.GetComponent<DefaultGun>().enabled = false;
                break;
        }
    }

    //[Client]
    //public void SetReady(bool ready)
    //{
    //    IsReady = ready;

    //    if (!isLocalPlayer)
    //        return;

    //    if (IsReady)
    //    {
    //        itemHandler.ShowGUI();
    //        //GetComponent<MiningController>().UpdateTiles();
    //    }
    //}
    //[Client]
    //public void RemoveItemFromGround(GameObject obj)
    //{
    //    if (isLocalPlayer)
    //        CmdRemoveGroundItem(obj);
    //}

    void Die()
    {
        //deathScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    #endregion

    #endregion

    private IEnumerator FadeServerMessage()
    {
        if(sceneScript)
        {
            yield return new WaitForSeconds(5f);

            sceneScript.canvasStatusText.enabled = false;
        }
    }

    private IEnumerator CoroutineCoordinator()
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

    #region Getter

    public ItemObject GetActiveItem()
    {
        if (ActiveQuickslot == -1)
            return null;

        return ItemHandler.QuickSlots.GetSlots[ActiveQuickslot].ItemObject;
    }

    public InventoryObject GetInventoryObject
    {
        get { return ItemHandler.Inventory; }
    }

    //public float DistanceFromPlayerX
    //{
    //    get { return distanceFromPlayerx; }

    //    set { distanceFromPlayerx = value; }
    //}

    //public float DistanceFromPlayerY
    //{
    //    get { return distanceFromPlayery; }

    //    set { distanceFromPlayery = value; }
    //}

    
    public int ActiveItemID
    { 
        get { return activeItemID; } 
    }

    public int MiningStrength
    {
        get { return miningStrength; }
    }
    #endregion
}