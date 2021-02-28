using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class EnemyBehaviour : NetworkBehaviour
{
    public enum EnemyTypes
    {
        Default, Frog
    }

    protected enum EnemyStates
    {
        FrogMining,
        FrogAggressive,
        FrogReturn,
        FrogIdle
    }

    [SerializeField] private InventoryObject inventory;
    [SerializeField] protected Stats stats;
    protected Rigidbody2D rb2d;
    [SerializeField] protected LimbMovement limbMovement;
    [HideInInspector] protected EnemyTypes enemyTypes;
    protected EnemyStates state;
    protected EnemyStates prevState;
    [HideInInspector] public Vector3 target;
    protected int currentHitpoints;
    protected bool foundPlayer;
    private float currentSpeed;

    private float fallSpeedReduction;
    private float heightReductionTimer;
    private float widthReductionTimer;
    private Vector3 previousLocation = Vector3.zero;
    protected BaseBehaviour frogBase;
    protected Pathfinder pathfinder;

    [Server]
    protected void Init()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.simulated = true;
        stats = Instantiate(stats);
        currentHitpoints = stats.hitPoints;
        pathfinder = GetComponent<Pathfinder>();

        frogBase = GameObject.FindWithTag("EnemyBase").GetComponent<BaseBehaviour>();
    }

    protected void DeathCheck()
    {
        if (currentHitpoints <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    public void UpdateHealth(int hpChange)
    {
        currentHitpoints += hpChange;
        Mathf.Clamp(currentHitpoints, 0, stats.hitPoints);
    }

    protected void MaxSpeedCheck()
    {
        if (rb2d.velocity.y < stats.maxFallSpeed)
        {
            heightReductionTimer += Time.deltaTime;
            fallSpeedReduction = Mathf.Clamp(heightReductionTimer, 1f, 5f);
            rb2d.velocity -= Vector2.down * fallSpeedReduction;
            if (rb2d.velocity.y > stats.maxFallSpeed + (stats.maxFallSpeed / 2))
                heightReductionTimer = 1f;
        }

        currentSpeed = Mathf.Abs(rb2d.velocity.x);

        if (currentSpeed > stats.maxSpeed && rb2d.velocity.x > 0)
        {
            widthReductionTimer += Time.deltaTime;
            float asedf = Mathf.Clamp(widthReductionTimer, 1f, 3f);
            rb2d.velocity -= Vector2.right * asedf;
            if (currentSpeed > stats.maxSpeed + (stats.maxSpeed / 2))
                widthReductionTimer = 1f;
        }

        else if (currentSpeed > stats.maxSpeed && rb2d.velocity.x < 0)
        {
            widthReductionTimer += Time.deltaTime;
            float asedf = Mathf.Clamp(widthReductionTimer, 1f, 3f);
            rb2d.velocity -= Vector2.left * asedf;
            if (currentSpeed > stats.maxSpeed + (stats.maxSpeed / 2))
                widthReductionTimer = 1f;
        }
    }
    public virtual void SetPath(List<Vector3> path)
    {
    }

    //protected virtual Vector3 FindTarget()
    //{
    //    Collider2D[] playerTargets = Physics2D.OverlapCircleAll(transform.position, stats.aggroRange, LayerMask.GetMask("Player"));

    //    foreach (Collider2D playerTarget in playerTargets)
    //    {
    //        if (playerTargets != null && playerTarget.TryGetComponent<PlayerController>(out PlayerController player) && Vector2.Distance(transform.position, player.transform.position) < stats.aggroRange)
    //        {
    //            Debug.Log("Found player");
    //            foundPlayer = true;
    //            Vector3 tempTarget = player.transform.position;
    //            return tempTarget;
    //        }
    //    }

    //    foundPlayer = false;
    //    return AlternativeTarget();
    //}

    protected virtual bool FindPlayerTarget()
    {
        //playerSearchTimer = 0;
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, stats.aggroRange, LayerMask.GetMask("Player"));

        foreach (Collider2D collider2D in colliderArray)
        {
            if (collider2D.TryGetComponent<PlayerController>(out PlayerController player))
            {
                Debug.Log("found player");
                //PathfindingDots.Instance.CalculatePaths(
                //this,
                //Vector3Int.FloorToInt(transform.position),
                //Vector3Int.FloorToInt(player.transform.position));

                //return true;

                return pathfinder.CalculatePath(
                    Vector3Int.FloorToInt(transform.position),
                    Vector3Int.FloorToInt(player.transform.position));

                //PathfindingDots.Instance.CalculatePaths(
                //    this, 
                //    Vector3Int.FloorToInt(transform.position) + Vector3Int.down, 
                //    Vector3Int.FloorToInt(player.transform.position) + Vector3Int.down);

                //if (pathfinder.CalculatePath(transform.position + Vector3.down, player.transform.position))
                //{
                //    Debug.Log("Hunting Player at: " + player.transform.position);
                //    return true;
                //}
            }
        }
        return false;
    }

    protected abstract void AlternativeTarget();

    protected void Flip()
    {
       
        if (target.x - transform.position.x > 0)
        {
            transform.rotation = new Quaternion(transform.rotation.x, 0, transform.rotation.z, transform.rotation.w);
        }
        else if (target.x - transform.position.x < 0)
        {
            transform.rotation = new Quaternion(transform.rotation.x, 180, transform.rotation.z, transform.rotation.w);
        }
        previousLocation = transform.position;
    }

    protected void Flip(float moveDirX)
    {
        Vector3 tar = transform.position + new Vector3(moveDirX, 0, 0);

        if (tar.x - transform.position.x > 0)
        {
            transform.rotation = new Quaternion(transform.rotation.x, 0, transform.rotation.z, transform.rotation.w);
        }
        else if (tar.x - transform.position.x < 0)
        {
            transform.rotation = new Quaternion(transform.rotation.x, 180, transform.rotation.z, transform.rotation.w);
        }
    }

    public Stats GetStats
    {
        get { return stats; }
    }

    public InventoryObject GetInventory
    {
        get { return inventory; }

        set { inventory = value; }
    }

    public float MiningStrength
    {
        get { return stats.mineStrength; }
    }
}
