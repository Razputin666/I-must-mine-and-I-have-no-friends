using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField] private float whenToStop;
    [SerializeField] private float whenToBack;
    [SerializeField] private float whenToMove;
    [SerializeField] private float speed;
    [SerializeField] private int enemyHP;
    
    #region EnemyValues

    private float distanceToPlayer;
    public PlayerController player;
    private JumpController jumpController;
    
    private Rigidbody2D rb2d;
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
    }

    // Update is called once per frame
    void Update()
    {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
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




        Vector3 temp = player.transform.position - transform.position;
        Debug.Log(distanceToPlayer);
        if (rb2d.velocity.x == 0f)
        {
            jumpController.Jump();
        }

        if (temp.x >= 0)
        {
            rb2d.velocity += Vector2.right * (speed * 0.1f);
            Flip(Vector2.left.x);
        }

        else
        {
            rb2d.velocity += Vector2.left * (speed * 0.1f);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
       if(collision.collider.CompareTag("Bullet"))
        {
            enemyHP -= collision.collider.GetComponent<Bullet>().projectileStrength;
        }



    }
}
