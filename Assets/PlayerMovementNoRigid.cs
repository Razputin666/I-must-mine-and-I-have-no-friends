using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementNoRigid : MonoBehaviour
{
    [SerializeField]
    LayerMask blockLayerMask;

    public enum PlayerStates
    {
        Mining, Normal
    }

    public PlayerStates playerStates { get; private set; }

    #region PlayerValues
   // private Tilemap targetedBlock;

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
    private Inventory inventory;
    public Transform item;
    private JumpController jumpController;
    [SerializeField] private DeathScreen deathScreen;


    [SerializeField]
    Camera _camera;

    void Start()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.

        facingRight = true;
        rb2d = GetComponent<Rigidbody2D>();
        boxCollider2d = transform.GetComponent<BoxCollider2D>();
        capsuleCollider2d = transform.GetComponent<CapsuleCollider2D>();
        jumpController = GetComponent<JumpController>();
        // itemController.SetMiningMode(); // Vi har inte combat än så den e på mining default
    }

    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    void FixedUpdate()
    {

        mousePos = Input.mousePosition;
        worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        _camera.transform.position = new Vector3(rb2d.transform.position.x, rb2d.transform.position.y, -1);


        if (Input.GetKey(KeyCode.D))
        {
            rb2d.MovePosition(Vector2.right * Time.deltaTime);
            Flip(Vector2.right.x);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb2d.MovePosition(Vector2.left  * Time.deltaTime);
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

        if (Input.GetKey("space") && jumpController.IsGrounded())
        {
            jumpController.Jump();
        }

        if (playerHP <= 0)
        {
            Die();
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
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





    #region GameLogic

    void Die()
    {
        Debug.Log("playerded");
        gameObject.SetActive(false);
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
