using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyController : NetworkBehaviour, HasCoolDownInterFace
{

    public enum EnemyStates
    {
        FrogMining, 
        FrogAggressive, 
        FrogReturn, 
        FrogIdle
    }

    public EnemyStates enemyStates { get; private set; }

    //private float whenToStop;
    //private float whenToBack;
    //private float whenToMove;
    [SerializeField] private float speed;
    [SerializeField] public int enemyHP;
    [SerializeField] private InventoryObject inventory;
    [SerializeField] private int id = 4;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private CoolDownSystem coolDownSystem;
    
    #region EnemyValues
    private float enterDigModeTimer = 0;
    private float distanceToPlayer;
    private float distanceGained;
    public PlayerController player;
    private JumpController jumpController;
    private EvilBeavisBaseController baseController;
    private List<Vector3Int> targetedOre;
    [SerializeField] private Transform item;
    
    public Rigidbody2D rb2d;
    private CapsuleCollider2D enemyCollider;
    private bool facingRight;
    public float maxFallSpeed;
    private float maxSpeed;
    float heightTimer;
    float widthTimer;

    public int enemyStrength;
    public Vector2 enemyKnockBack;
    public float miningStrength;
    public Vector3 target;
    //private Vector3 pathfindingTarget;
    private float distanceToTarget;
    private int blockAmount;
    private int oreAmount;

    //Path Variables
    private Vector3 previousPosition;
    private float timeStoodStill;
    private Vector2 moveDir;
    private float playerSearchTimer;
    private float movedCheckTimer;
    private Pathfinder pathfinder;
    //Item pickup variables
    [SerializeField] private float itemPickupRange;
    private float itemPickupCooldown;
    private const float itemPickupCooldownDefault = 0.5f;

    #endregion
    // Start is called before the first frame update
    public override void OnStartServer()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.simulated = true;
        //player = FindObjectOfType<PlayerController>();
        enemyCollider = GetComponent<CapsuleCollider2D>();
        jumpController = GetComponent<JumpController>();
        pathfinder = GetComponent<Pathfinder>();
        //item = gameObject.GetComponentInChildren<FacePlayer>().gameObject.transform.Find("ItemHeldInHand");
        //item.GetComponent<MiningController>().enabled = false;
        //baseController = GameObject.FindWithTag("EnemyBase").GetComponent<EvilBeavisBaseController>();
        //targetedOre = baseController.targetedOre;
        enemyStates = EnemyStates.FrogIdle;
        inventory = Instantiate(inventory);
        moveDir = Vector2.right;
        timeStoodStill = 0f;
        playerSearchTimer = 0f;
        movedCheckTimer = 0f;
        previousPosition = transform.position;
    }

    void Update()
    {
        if (!isServer)
            return;

        playerSearchTimer += Time.deltaTime;
        itemPickupCooldown += Time.deltaTime;
        movedCheckTimer += Time.deltaTime;
        CheckState();

        if(itemPickupCooldown >= itemPickupCooldownDefault)
        {
            CheckItemCollision();
            itemPickupCooldown = 0f;
        }
        if(movedCheckTimer > 0.2f)
        {
            previousPosition = transform.position;
            movedCheckTimer = 0f;
        }

        if (playerSearchTimer >= 3f)
            LookForPlayer();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isServer)
            return;

        moveDir.Normalize();
        Flip(moveDir.x);
        rb2d.velocity += moveDir * speed * Time.fixedDeltaTime;
        if (rb2d.velocity.y < -25f)
        {
            heightTimer += Time.deltaTime;
            maxFallSpeed = Mathf.Clamp(heightTimer, 1f, 5f);
            rb2d.velocity -= Vector2.down * maxFallSpeed;
            if (rb2d.velocity.y > -35f)
                heightTimer = 1f;
        }

        maxSpeed = Mathf.Abs(rb2d.velocity.x);

        if (maxSpeed > 10f && rb2d.velocity.x > 0)
        {
            widthTimer += Time.deltaTime;
            float asedf = Mathf.Clamp(widthTimer, 1f, 3f);
            rb2d.velocity -= Vector2.right * asedf;
            if (maxSpeed > 15f)
                widthTimer = 1f;
        }
        else if (maxSpeed > 10f && rb2d.velocity.x < 0)
        {
            widthTimer += Time.deltaTime;
            float asedf = Mathf.Clamp(widthTimer, 1f, 3f);
            rb2d.velocity -= Vector2.left * asedf;
            if (maxSpeed > 15f)
                widthTimer = 1f;
        }

        if(enemyHP <= 0)
        {
            Die();
        }

    }

    private void CheckState()
    {
        switch(enemyStates)
        {
            case EnemyStates.FrogIdle:
                if (Mathf.Abs(transform.position.x - previousPosition.x) < 0.1f)
                {
                    timeStoodStill += Time.deltaTime;

                    //Change direction if we stand still for too long
                    if (timeStoodStill >= 3f)
                    {
                        moveDir *= -1;
                    }
                    else if (timeStoodStill >= 0.5f)//Try jumping if we are stuck
                    {
                        jumpController.Jump();
                    }
                }
                else
                {
                    timeStoodStill = 0f;
                }
                break;

            case EnemyStates.FrogAggressive:
                pathfinder.Move();

                if (!pathfinder.pathCompleted)
                {
                    enemyStates = EnemyStates.FrogIdle;
                    LookForPlayer();
                    break;
                }

                moveDir = pathfinder.Direction;

                break;

            default:
                break;
        }
    }

    private void LookForPlayer()
    {
        playerSearchTimer = 0;
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, 30);

        foreach (Collider2D collider2D in colliderArray)
        {
            if (collider2D.TryGetComponent<PlayerController>(out PlayerController player))
            {
                Debug.Log("found player");
                if(pathfinder.CalculatePath(transform.position + Vector3.down, player.transform.position))
                {
                    target = player.transform.position;
                    enemyStates = EnemyStates.FrogAggressive;
                    Debug.Log("Hunting Player at: " + player.transform.position);
                    return;
                } 
            }
        }
    }

    [Server]
    private void CheckItemCollision()
    {
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, itemPickupRange);

        foreach (Collider2D collider2D in colliderArray)
        {
            if (collider2D.TryGetComponent<GroundItem>(out GroundItem groundItem))
            {
                Item newItem = new Item(groundItem.Item);
                //tell the client to add the item we collided with
                GetInventoryObject.AddItem(newItem, newItem.Amount);
                NetworkServer.Destroy(collider2D.gameObject);
            }
        }
    }

    //void MoveEnemy()
    //{


    //}

    //void GetTarget()
    //{

    //    if (player != null)
    //    {
    //        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

    //        if (distanceToPlayer > 30f || Vector3.Distance(player.transform.position, baseController.gameObject.transform.position) > 30f && player != null)
    //        {
    //            target = player.transform.position - transform.position;
    //        }
    //    }
    //    else if (baseController.targetedOre.Count > 0)
    //    {
    //        blockAmount = GetBlockAmount();
    //        oreAmount = GetOreAmount();
    //        if (blockAmount >= 50 || oreAmount >= 10)
    //        {
    //             target = baseController.transform.position - transform.position;
    //            // target = new Vector3(Mathf.Clamp(target.x, target.x - 20f, target.x + 20f), Mathf.Clamp(target.y, target.y - 20f, target.y + 20f));
    //            //  target = new Vector3(Mathf.Clamp(baseController.transform.position.x - transform.position.x, -20f, 20f), Mathf.Clamp(baseController.transform.position.y - transform.position.y, -20f, 20f));
    //          //  target = baseController.transform.position;
    //        }
    //        else
    //        {
    //              target = baseController.targetedOre[baseController.targetedOre.Count - 1] - transform.position;
    //            // target = new Vector3(Mathf.Clamp(target.x, target.x - 20f, target.x + 20f), Mathf.Clamp(target.y, target.y - 20f, target.y + 20f));
    //          //  target = baseController.targetedOre[baseController.targetedOre.Count - 1];
    //            distanceToTarget = Vector3.Distance(baseController.targetedOre[baseController.targetedOre.Count - 1], transform.position);
    //        }
    //    }
    //}

    //void StateChange()
    //{


    //    if (DistanceGained >= transform.position.magnitude)
    //    {
    //        SetMiningMode();
    //    }

    //    if (player != null && Vector3.Distance(transform.position, player.transform.position) <= 10f)
    //    {
    //        RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, 9f);
    //        if (hit.transform && hit.transform.CompareTag("Player"))
    //        {
    //            SetAggressiveMode();
    //        }
    //    }

    //    DistanceGained = transform.position.magnitude;
    //    enterDigModeTimer = 0f;
    //}

    void Die()
    {
        Destroy(gameObject);
    }

    private void Flip(float horizontal)
    {
        if (horizontal > 0 && !facingRight || horizontal < 0 && facingRight)
        {
            facingRight = !facingRight;

            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
            //coolDownSystem.PutOnCoolDown(this);

        }
    }
    public float DistanceGained
    {
        get { return distanceGained; }

        set { distanceGained = value; }
    }

    public void SetMiningMode()
    {
        item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Diamond Pick 1");
        //item.GetComponent<MiningController>().enabled = true;
        item.GetComponent<PolygonCollider2D>().enabled = false;
        enemyStates = EnemyStates.FrogMining;
    }

    public void SetAggressiveMode()
    {
        item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Diamond Sword");
        //item.GetComponent<MiningController>().enabled = false;
        item.GetComponent<PolygonCollider2D>().enabled = true;
        enemyStates = EnemyStates.FrogAggressive;
    }


    public InventoryObject GetInventoryObject
    {
        get { return inventory; }
    }

    private int GetBlockAmount()
    {
        int items = 0;
        List<int> listOfItems = new List<int>();

        //if(inventory.FindItemInInventory("Dirt") != null)
        //junk = inventory.FindItemInInventory("Dirt").Amount;
        //if (inventory.FindItemInInventory("Stone") != null)
        //    junk += inventory.FindItemInInventory("Stone").Amount;
        //if (inventory.FindItemInInventory("Grass") != null)
        //    junk += inventory.FindItemInInventory("Grass").Amount;
        
        if (inventory.FindItemsInInventory("Common") != null)
        {
            for (int i = 0; i < inventory.FindItemsInInventory("Common").Count; i++)
            {
                listOfItems.Add(inventory.FindItemsInInventory("Common")[i].Amount);
            }
            for (int j = 0; j < listOfItems.Count; j++)
            {
                items += listOfItems[j];
            }
        }
        
        return items;
    }
    private int GetOreAmount()
    {
        int items = 0;
        List<int> listOfItems = new List<int>();
        if (inventory.FindItemsInInventory("Ore") != null)
        {
            for (int i = 0; i < inventory.FindItemsInInventory("Ore").Count; i++)
            {
                listOfItems.Add(inventory.FindItemsInInventory("Ore")[i].Amount);
            }
            for (int j = 0; j < listOfItems.Count; j++)
            {
                items += listOfItems[j];
            }
        }

        return items;
    }

    public int OreAmount
    {
        get { return oreAmount; }

        set { oreAmount = value; }
    }

    public int BlockAmount
    {
        get { return blockAmount; }

        set { blockAmount = value; }
    }

    public InventoryObject Inventory
    {
        get
        { 
            return inventory;
        }

        set
        {
            inventory = value;
        }
    }

    public int Id => id;

    public float CoolDownDuration => coolDownDuration;
}
