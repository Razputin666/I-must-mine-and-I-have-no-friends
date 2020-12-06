using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    LayerMask blockLayerMask;

    public enum PlayerStates
    {
        Mining, Normal, Building, Idle
    }

    public PlayerStates playerStates { get; private set; }

    #region PlayerValues
    private Tilemap targetedBlock;
    
    public int playerHP;
    public float speed;                //Floating point variable to store the player's movement speed.
    public float jumpVelocity;
    public float miningStrength;

    public float maxFallSpeed;
    private float maxSpeed;
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
   [SerializeField]private DeathScreen deathScreen;
    [SerializeField] private LevelGeneratorLayered mapSize;


    [SerializeField]
    Camera _camera;
    [SerializeField]
    Camera _secondCamera;

    void Start()
    {
        //inventory = GetComponentInChildren<Inventory>();
        item = gameObject.GetComponentInChildren<FaceMouse>().gameObject.transform.Find("ItemHeldInHand");
        item.GetComponent<SpriteRenderer>().sprite = null;
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
        mapSize = GameObject.Find("LevelGeneration").GetComponent<LevelGeneratorLayered>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
       // itemController.SetMiningMode(); // Vi har inte combat än så den e på mining default
        StartCoroutine(CoroutineCoordinator());
    }

    private void Update()
    {
        CheckQuickslotInput();
    }

    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    void FixedUpdate()
    {
        mousePos = Input.mousePosition;
        worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        _camera.transform.position = new Vector3(Mathf.Clamp(rb2d.transform.position.x, (0) + 26.7f, (mapSize.startPosition.x) - 26.7f), rb2d.transform.position.y, -1);



        CalculateMovement();

        if (Input.GetKey("space") && jumpController.IsGrounded())
        {
            jumpController.Jump();
        }

        if (playerHP <= 0)
        {
            Die();
        }

    }

    private void CalculateMovement()
    {
        if (Input.GetKey(KeyCode.D))
        {
            rb2d.velocity += Vector2.right * speed;
            Flip(Vector2.right.x);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb2d.velocity += Vector2.left * speed;
            Flip(Vector2.left.x);
        }
        if (rb2d.velocity.y < -25f)
        {
            heightTimer += Time.deltaTime;
            maxFallSpeed = Mathf.Clamp(heightTimer, 1f, 5f);
            rb2d.velocity -= Vector2.down * maxFallSpeed;
            if (rb2d.velocity.y > -35f)
                heightTimer = 1f;
        }

        maxSpeed = Mathf.Abs(rb2d.velocity.x);

        if (maxSpeed > 25f && rb2d.velocity.x > 0)
        {
            widthTimer += Time.deltaTime;
            float asedf = Mathf.Clamp(widthTimer, 1f, 3f);
            rb2d.velocity -= Vector2.right * asedf;
            if (maxSpeed > 30f)
                widthTimer = 1f;
        }

        if (maxSpeed > 25f && rb2d.velocity.x < 0)
        {
            widthTimer += Time.deltaTime;
            float asedf = Mathf.Clamp(widthTimer, 1f, 3f);
            rb2d.velocity -= Vector2.left * asedf;
            if (maxSpeed > 30f)
                widthTimer = 1f;
        }
    }

    private void CheckQuickslotInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[0].ItemObject;
            if (itemObj != null)
            {
                item.GetComponent<SpriteRenderer>().sprite = itemObj.UIDisplaySprite;

                SetPlayerState(itemObj.ItemType);
            }
            else
            {
                playerStates = PlayerStates.Normal;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[1].ItemObject;
            if (itemObj != null)
            {
                item.GetComponent<SpriteRenderer>().sprite = itemObj.UIDisplaySprite;

                SetPlayerState(itemObj.ItemType);
            }
            else
            {
                playerStates = PlayerStates.Normal;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[2].ItemObject;
            if (itemObj != null)
            {
                item.GetComponent<SpriteRenderer>().sprite = itemObj.UIDisplaySprite;

                SetPlayerState(itemObj.ItemType);
            }
            else
            {
                playerStates = PlayerStates.Normal;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = false;
            }

        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[3].ItemObject;
            if (itemObj != null)
            {
                item.GetComponent<SpriteRenderer>().sprite = itemObj.UIDisplaySprite;

                SetPlayerState(itemObj.ItemType);
            }
            else
            {
                playerStates = PlayerStates.Normal;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[4].ItemObject;
            if (itemObj != null)
            {
                item.GetComponent<SpriteRenderer>().sprite = itemObj.UIDisplaySprite;

                SetPlayerState(itemObj.ItemType);
            }
            else
            {
                playerStates = PlayerStates.Normal;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[5].ItemObject;
            if (itemObj != null)
            {
                item.GetComponent<SpriteRenderer>().sprite = itemObj.UIDisplaySprite;

                SetPlayerState(itemObj.ItemType);
            }
            else
            {
                playerStates = PlayerStates.Normal;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ItemObject itemObj = itemHandler.QuickSlots.Container.InventorySlot[6].ItemObject;
            if (itemObj != null)
            {
                item.GetComponent<SpriteRenderer>().sprite = itemObj.UIDisplaySprite;

                SetPlayerState(itemObj.ItemType);
            }
            else
            {
                playerStates = PlayerStates.Normal;
                item.GetComponent<MiningController>().enabled = false;
                item.GetComponent<DefaultGun>().enabled = false;
            }
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
        deathScreen.gameObject.SetActive(true);
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
    public InventoryObject GetInventoryObject
    {
        get { return itemHandler.Inventory; }
    }

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

  
    #endregion
    private void Flip(float horizontal)
    {
        if (horizontal > 0 && !facingRight || horizontal < 0 && facingRight)
        {
            facingRight = !facingRight;

            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
}