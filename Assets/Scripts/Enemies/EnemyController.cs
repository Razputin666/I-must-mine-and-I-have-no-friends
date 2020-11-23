using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public enum EnemyStates
    {
        FrogMining, FrogAggressive
    }

    public EnemyStates enemyStates { get; private set; }

    [SerializeField] private float whenToStop;
    [SerializeField] private float whenToBack;
    [SerializeField] private float whenToMove;
    [SerializeField] private float speed;
    [SerializeField] public int enemyHP;
    [SerializeField] private float enterDigModeTimer;


    
    #region EnemyValues

    private float distanceToPlayer;
    private float distanceGained;
    public PlayerController player;
    private JumpController jumpController;
    public Transform item;
    
    public Rigidbody2D rb2d;
    private CapsuleCollider2D enemyCollider;
    private bool facingRight;
    public float maxFallSpeed;
    private float maxSpeed;
    float heightTimer;
    float widthTimer;

    public int enemyStrength;
    public Vector2 enemyKnockBack;



    #endregion
    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerController>();
        enemyCollider = GetComponent<CapsuleCollider2D>();
        jumpController = GetComponent<JumpController>();
        item = gameObject.GetComponentInChildren<FacePlayer>().gameObject.transform.Find("ItemHeldInHand");
        item.GetComponent<MiningController>().enabled = false;
        enemyStates = EnemyStates.FrogAggressive;
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        enterDigModeTimer += Time.deltaTime;
        // Debug.Log(player.transform.position.x - transform.position.x);

        //if (distanceToPlayer == whenToStop)
        //{
        //    speed = 0;
        //}
        //if (distanceToPlayer < whenToBack)
        //{
        //    speed = -0.4f;
        //}
        //if (distanceToPlayer > whenToMove)
        //{
        //    speed = 0.4f;

        //}

        if (enterDigModeTimer >= 5f)
        {
           
            // Debug.Log(transform.position.magnitude + " Position " + DistanceGained + " Distance gained");
           
            if(DistanceGained >= transform.position.magnitude)
            {
                SetMiningMode();
            }

            else if((DistanceGained * 1.5f) < transform.position.magnitude && enemyStates != EnemyStates.FrogAggressive)
            {
                SetAggressiveMode();
            }
            DistanceGained = transform.position.magnitude;
            enterDigModeTimer = 0f;
        }




        Vector3 temp = player.transform.position - transform.position;

        // Debug.Log(item.position +" Pickaxe position " + (player.worldPosition - player.transform.position) + " Mouse position");
        if (rb2d.velocity.x == 0f && enemyStates != EnemyStates.FrogMining || temp.y > 0)
        {
            jumpController.Jump();
        }

        if (temp.x >= 0)
        {
            // rb2d.velocity += Vector2.right * (speed * Time.deltaTime);
              transform.position += Vector3.right * (speed * Time.deltaTime);
            //  rb2d.MovePosition(transform.position + (Vector3.right * speed * Time.deltaTime));
           // rb2d.AddForce(Vector3.right * speed * Time.deltaTime);
            Flip(Vector2.left.x);
          
            
        }

        else
        {
           //  rb2d.velocity += Vector2.left * (speed * Time.deltaTime);
             transform.position += Vector3.left * (speed * Time.deltaTime);
            // rb2d.MovePosition(player.transform.position * (speed * Time.deltaTime));
          //  rb2d.MovePosition(transform.position + (Vector3.left * speed * Time.deltaTime));
          // rb2d.AddForce(Vector3.left * speed * Time.deltaTime);
            Flip(Vector2.right.x);
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

        // rb2d.velocity = temp * speed;
    }

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
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //   if(collision.collider.CompareTag("Bullet"))
    //   {
    //        enemyHP -= collision.collider.GetComponent<Bullet>().projectileStrength;
    //   }
    //    if (collision.collider.CompareTag("Enemy"))
    //    {
    //        EnemyController ally = collision.collider.gameObject.GetComponentInParent<EnemyController>();

    //        if (collision.otherCollider.transform.position.x - collision.collider.transform.position.x > 0f)
    //        {
    //            rb2d.AddForceAtPosition(ally.enemyKnockBack, transform.position);
    //        }

    //        else if (collision.otherCollider.transform.position.x - collision.collider.transform.position.x < 0f)
    //        {
    //            rb2d.AddForceAtPosition(-ally.enemyKnockBack, transform.position);
    //        }

    //    }

    //    else if (collision.collider.CompareTag("Player") && enemyStates != EnemyStates.FrogAggressive)
    //    {
    //        SetAggressiveMode();
    //    }
    //}
    public float DistanceGained
    {
        get { return distanceGained; }

        set { distanceGained = value; }
    }

    public void SetMiningMode()
    {
        item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Diamond Pick 1");
        item.GetComponent<MiningController>().enabled = true;
        item.GetComponent<PolygonCollider2D>().enabled = false;
        enemyStates = EnemyStates.FrogMining;
    }

    public void SetAggressiveMode()
    {
        item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Diamond Sword");
        item.GetComponent<MiningController>().enabled = false;
        item.GetComponent<PolygonCollider2D>().enabled = true;
        enemyStates = EnemyStates.FrogAggressive;
    }

}
