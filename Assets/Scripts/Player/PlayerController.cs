﻿using System.Collections;
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
    private Tilemap targetedBlock;
    
    public int playerHP;
    public float speed;                //Floating point variable to store the player's movement speed.
    public float jumpVelocity;
    public float maxFallSpeed;
    private float maxSpeed;
    Vector2 horizontalSpeed;
    Vector2 verticalSpeed;
    float heightTimer;
    float widthTimer;
    float jumpTimer;
    public Rigidbody2D rb2d;        //Store a reference to the Rigidbody2D component required to use 2D Physics.
    private BoxCollider2D boxCollider2d;
    private CapsuleCollider2D capsuleCollider2d;
    private bool facingRight;
    public Vector3 worldPosition;
    public Vector3 mousePos;
    private float distanceFromPlayerx;
    private float distanceFromPlayery;
    #endregion


    public Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    private ItemController itemController;
    private ItemHandler itemHandler;
    public Transform item;
    private JumpController jumpController;
   [SerializeField]
    private DeathScreen deathScreen;


    //[SerializeField]
    Camera camera;

    //Network
    [SerializeField]
    private TextMesh playerNameText;
    [SerializeField]
    private GameObject floatingInfo;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;
  
    private SceneScript sceneScript;

    private void Awake()
    {
        //allow all players to run this
        sceneScript = GameObject.FindObjectOfType<SceneScript>();
    }

    #region Network
    private void OnNameChanged(string _Old, string _New)
    {
        playerNameText.text = playerName;
    }
    private void OnActiveItemChanged(ItemObject _Old, ItemObject _new)
    {
        if (_new == null)
            return;

        item.GetComponent<SpriteRenderer>().sprite = _new.UIDisplaySprite;
    }

    public override void OnStartLocalPlayer()
    {
        if (camera == null)
            camera = Instantiate(Camera.main);
        
        camera.GetComponent<AudioListener>().enabled = true;
        camera.transform.SetParent(transform);
        camera.transform.localPosition = new Vector3(0, 0, -20);

        //Camera.main.CopyFrom(camera);
        floatingInfo.transform.localPosition = new Vector3(-0.1f, 1f, 0f);
        floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        string name = "Player" + Random.Range(100, 999);

        CmdSetupPlayer(name);

        //old
        //inventory = GetComponentInChildren<Inventory>();
        item = gameObject.GetComponentInChildren<FaceMouse>().gameObject.transform.Find("ItemHeldInHand");
        item.GetComponent<SpriteRenderer>().sprite = null;// Resources.Load<Sprite>("Sprites/Items/Drill");
        item.GetComponent<DefaultGun>().enabled = false;
        item.GetComponent<MiningController>().enabled = false;

        //Get and store a reference to the Rigidbody2D component so that we can access it.

        facingRight = true;
        rb2d = GetComponent<Rigidbody2D>();
        boxCollider2d = transform.GetComponent<BoxCollider2D>();
        capsuleCollider2d = transform.GetComponent<CapsuleCollider2D>();
        jumpController = GetComponent<JumpController>();
        itemHandler = GetComponent<ItemHandler>();
        playerStates = PlayerStates.Idle;
        // itemController.SetMiningMode(); // Vi har inte combat än så den e på mining default
        StartCoroutine(CoroutineCoordinator());
    }

    #region Commands
    [Command]
    public void CmdActiveItemChanged(string spriteName)
    {
        UpdateSprite(spriteName);
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

    [ClientRpc]
    private void UpdateSprite(string spriteName)
    {
        Debug.Log(spriteName);
        if (spriteName != "")
            item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/" + spriteName);
        else
            item.GetComponent<SpriteRenderer>().sprite = null;
    }

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
            if(camera)
                floatingInfo.transform.LookAt(camera.transform);
            return;
        }

        CheckQuickslotInput();
    }

    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        mousePos = Input.mousePosition;
        worldPosition = camera.ScreenToWorldPoint(mousePos);
        //camera.transform.position = new Vector3(rb2d.transform.position.x, rb2d.transform.position.y, -1);

        //CalculateMovement();

        if (Input.GetKey("space") && jumpController.IsGrounded())
        {
            jumpController.Jump();
        }

        if (playerHP <= 0)
        {
            Die();
        }

    }
    #endregion

    //private void CalculateMovement()
    //{
    //    if (Input.GetKey(KeyCode.D))
    //    {
    //        rb2d.velocity += Vector2.right * speed;
    //        Flip(Vector2.right.x);
    //    }

    //    if (Input.GetKey(KeyCode.A))
    //    {
    //        rb2d.velocity += Vector2.left * speed;
    //        Flip(Vector2.left.x);
    //    }
    //    if (rb2d.velocity.y < -25f)
    //    {
    //        heightTimer += Time.deltaTime;
    //        maxFallSpeed = Mathf.Clamp(heightTimer, 1f, 5f);
    //        rb2d.velocity -= Vector2.down * maxFallSpeed;
    //        if (rb2d.velocity.y > -35f)
    //            heightTimer = 1f;
    //    }

    //    maxSpeed = Mathf.Abs(rb2d.velocity.x);

    //    if (maxSpeed > 25f && rb2d.velocity.x > 0)
    //    {
    //        widthTimer += Time.deltaTime;
    //        float asedf = Mathf.Clamp(widthTimer, 1f, 3f);
    //        rb2d.velocity -= Vector2.right * asedf;
    //        if (maxSpeed > 30f)
    //            widthTimer = 1f;
    //    }

    //    if (maxSpeed > 25f && rb2d.velocity.x < 0)
    //    {
    //        widthTimer += Time.deltaTime;
    //        float asedf = Mathf.Clamp(widthTimer, 1f, 3f);
    //        rb2d.velocity -= Vector2.left * asedf;
    //        if (maxSpeed > 30f)
    //            widthTimer = 1f;
    //    }
    //}


    private void UpdateState(ItemObject itemObj)
    {
        if (itemObj != null)
        {
            CmdActiveItemChanged(itemObj.UIDisplaySprite.name);

            SetPlayerState(itemObj.ItemType);
        }
        else
        {
            CmdActiveItemChanged("");

            playerStates = PlayerStates.Idle;
            item.GetComponent<MiningController>().enabled = false;
            item.GetComponent<DefaultGun>().enabled = false;
        }
    }

    private void CheckQuickslotInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[0].ItemObject;
            UpdateState(itemObj);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[1].ItemObject;
            UpdateState(itemObj);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[2].ItemObject;
            UpdateState(itemObj);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[3].ItemObject;
            UpdateState(itemObj);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[4].ItemObject;
            UpdateState(itemObj);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[5].ItemObject;
            UpdateState(itemObj);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[6].ItemObject;
            UpdateState(itemObj);
        }
    }

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

    //IEnumerator MineBlock()
    //{
    //    int blockHP = 1;

    //    while (Input.GetMouseButton(0) && TargetedBlock != null)
    //    {
    //        if(blockHP == 0)
    //        {
    //            blockTile = TargetedBlock.GetComponent<Tilemap>();



    //            Vector3Int targetBlockIntPos = Vector3Int.FloorToInt(worldPosition);
    //            targetBlockIntPos.z = 0;
    //            Debug.Log(targetBlockIntPos);

    //            blockTile.SetTile(targetBlockIntPos, null);


    //        }
    //        blockHP -= 1;
    //        yield return new WaitForSeconds(0.1f);          
    //    }

    //}

    //public Tilemap TargetedBlock
    //{
    //    get 
    //    {
    //        return targetedBlock; 

    //    }

    //    set 
    //    {
    //        targetedBlock = value;
    //    }
    //}

    public float DistanceFromPlayerX
    {
        get { return distanceFromPlayerx; }

        set { distanceFromPlayerx = value; }
    }

    public float DistanceFromPlayerY
    {
        get { return distanceFromPlayery; }

        set { distanceFromPlayery = value; }
    }

    //IEnumerator MineBlockOld()
    //{
    //    int blockHP = 1;
    //    Collider2D thisTargetedBlock = TargetedBlock;
    //    while (Input.GetMouseButton(0) && TargetedBlock == thisTargetedBlock && TargetedBlock != null)
    //    {
    //        if (blockHP == 0)
    //        {
    //            TilemapVisual tilemapVisual = targetedBlock.transform.parent.GetComponent<TilemapVisual>();
    //            if (tilemapVisual != null)
    //            {
    //                TilemapOLD.TilemapObject tilemapObject = tilemapVisual.GetGridObjectAtXY(targetedBlock.transform.position);
    //                if (tilemapObject != null)
    //                {
    //                    tilemapObject.SetTilemapSprite(TilemapOLD.TilemapObject.TilemapSprite.None);

    //                    //Destroy(TargetedBlock.gameObject);
    //                }
    //            }

    //        }
    //        blockHP -= 1;
    //        yield return new WaitForSeconds(0.05f);
    //    }

    //}
    #endregion
    //private void Flip(float horizontal)
    //{
    //    if (horizontal > 0 && !facingRight || horizontal < 0 && facingRight)
    //    {
    //        facingRight = !facingRight;

    //        Vector3 theScale = transform.localScale;
    //        theScale.x *= -1;
    //        transform.localScale = theScale;
    //    }
    //}
}