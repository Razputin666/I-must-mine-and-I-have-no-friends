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
   // private LegMovement legMovement;
   [SerializeField]private DeathScreen deathScreen;
    [SerializeField] private LevelGeneratorLayered mapSize;
    [SerializeField] private LimbMovement legMovement;


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
        
        facingRight = true;
        // legMovement = GetComponent<LegMovement>();
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
        //  _camera.transform.position = new Vector3(Mathf.Clamp(rb2d.transform.position.x, (0) + 26.7f, (mapSize.startPosition.x) - 26.7f), rb2d.transform.position.y, -1);
        _camera.transform.position = new Vector3(transform.position.x, transform.position.y, -1f);


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
            //rb2d.velocity += Vector2.right * speed;
            legMovement.MoveFootTarget(Vector2.right);
            Flip(Vector2.right.x);
        }

        if (Input.GetKey(KeyCode.A))
        {
           // rb2d.velocity += Vector2.left * speed;
            //RaycastHit2D slopeCheck = Physics2D.Raycast(new Vector2(transform.position.x - 3f, transform.position.y), Vector2.down, transform.localScale.y * 4, LayerMask.GetMask("Blocks"));
            //RaycastHit2D slopeCheck = Physics2D.BoxCast(new Vector2(transform.position.x, transform.position.y - transform.localScale.y), new Vector2(boxCollider2d.bounds.size.x, boxCollider2d.bounds.size.y), 0f, Vector2.left, 0.2f);
            ////RaycastHit2D slopeCheck = Physics2D.Raycast(new Vector2(transform.position.x + 1f, transform.position.y), Vector2.down, transform.localScale.y * 4, LayerMask.GetMask("Blocks"));
            //Debug.Log(slopeCheck.point.y);
            //if (slopeCheck.collider != null && transform.position.y - slopeCheck.point.y < 3f && jumpController.IsGrounded())
            //{

            //    transform.position = Vector3.MoveTowards(transform.position, new Vector2(transform.position.x, slopeCheck.point.y + transform.localScale.y - 0.5f), speed);
            //}
            //for (int i = 0; i < legMovement.legs.Length; i++)
            //{
            //    legMovement.currentTargets[i].position = Vector2.MoveTowards(legMovement.currentTargets[i].position, legMovement.desiredTargets[i].position, speed * Time.deltaTime);
            //}
            legMovement.MoveFootTarget(Vector2.left);
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
            EnemyBehaviour enemy = collision.collider.gameObject.GetComponentInParent<EnemyBehaviour>();
            playerHP -= enemy.GetStats.strength;

            if (collision.otherCollider.transform.position.x - collision.collider.transform.position.x > 0f)
            {
                rb2d.AddForceAtPosition(Vector2.right * 1000, transform.position);
            }

            else if (collision.otherCollider.transform.position.x - collision.collider.transform.position.x < 0f)
            {
                rb2d.AddForceAtPosition(Vector2.left * 1000, transform.position);
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