using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class EnemyBehaviour : MonoBehaviour
{
    public enum EnemyTypes
    {
        Default, Frog
    }
    [SerializeField] private InventoryObject inventory;
    [SerializeField] protected Stats stats;
    public Rigidbody2D rb2d;
    [SerializeField] protected LimbMovement limbMovement;
    [HideInInspector] protected EnemyTypes enemyTypes;


    [HideInInspector] public Vector3 target;
    protected List<PlayerController> players;
    protected int currentHitpoints;
    protected bool foundPlayer;
    private float currentSpeed;



    private float fallSpeedReduction;
    private float heightReductionTimer;
    private float widthReductionTimer;
    private Vector3 previousLocation = Vector3.zero;


    protected virtual void Start()
    {
        stats = Instantiate(stats);
        currentHitpoints = stats.hitPoints;
        // players = GameObject.Find("PlayerList").GetComponent<PlayerList>.players; Fixa det efter multiplayer
    }

    protected virtual void FixedUpdate()
    {
        DeathCheck();
        //  MaxSpeedCheck();
    }

    private void DeathCheck()
    {
        if (currentHitpoints <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void UpdateHealth(int hpChange)
    {
        currentHitpoints += hpChange;
        Mathf.Clamp(currentHitpoints, 0, stats.hitPoints);
    }

    private void MaxSpeedCheck()
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

    protected virtual Vector3 FindTarget()
    {
    Collider2D[] playerTargets = Physics2D.OverlapCircleAll(transform.position, stats.aggroRange, LayerMask.GetMask("Player"));

        foreach (Collider2D playerTarget in playerTargets)
        {
            if (playerTargets != null && playerTarget.TryGetComponent<PlayerController>(out PlayerController player) && Vector2.Distance(transform.position, player.transform.position) < stats.aggroRange)
            {
                Debug.Log("Found player");
                foundPlayer = true;
                Vector3 tempTarget = player.transform.position;
                return tempTarget;
            }
        }

            foundPlayer = false;
            return AlternativeTarget();
    }

    protected abstract Vector3 AlternativeTarget();

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

    public Stats GetStats
    {
        get { return stats; }
    }

    public InventoryObject GetInventory
    {
        get { return inventory; }

        set { inventory = value; }
    }

}
