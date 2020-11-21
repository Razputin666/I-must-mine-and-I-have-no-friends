using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    LayerMask blockLayerMask;

    public enum UnitMode
    {
        Mining, Combat
    }
    private Tilemap targetedBlock;
    public UnitMode unitMode;
    public float speed;                //Floating point variable to store the player's movement speed.
    public float jumpVelocity;
    public float maxFallSpeed;
    private float maxSpeed;
    Vector2 horizontalSpeed;
    Vector2 verticalSpeed;
    float heightTimer;
    float widthTimer;
    float jumpTimer;


    private Rigidbody2D rb2d;        //Store a reference to the Rigidbody2D component required to use 2D Physics.
    private BoxCollider2D boxCollider2d;
    private bool facingRight;
    Vector3 worldPosition;
    private float distanceFromPlayerx;
    private float distanceFromPlayery;
    private bool inPrecisionMode;
    private bool inFreeMode;
    private Tilemap blockTile;
    Vector3Int targetBlockIntPos;


    public Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();


    [SerializeField]
    Camera _camera;

    void Start()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        facingRight = true;
        rb2d = GetComponent<Rigidbody2D>();
        boxCollider2d = transform.GetComponent<BoxCollider2D>();
        SetMiningMode(); // Vi har inte combat än så den e på mining default
        StartCoroutine(CoroutineCoordinator());
    }

    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    void FixedUpdate()
    {

        //Store the current horizontal input in the float moveHorizontal.
        float moveHorizontal = Input.GetAxis("Horizontal");
        Flip(moveHorizontal);
        //Store the current vertical input in the float moveVertical.
        float moveVertical = Input.GetAxis("Vertical");

        //Use the two store floats to create a new Vector2 variable movement.
        Vector2 movement = new Vector2(moveHorizontal, 0);



        //Call the AddForce function of our Rigidbody2D rb2d supplying movement multiplied by speed to move our player.
        rb2d.AddForce(movement * speed);
        _camera.transform.position = new Vector3(rb2d.transform.position.x, rb2d.transform.position.y, -1);



        if(Input.GetKey(KeyCode.D))
        {
            rb2d.velocity += Vector2.right * speed;
            Flip(Vector2.right.x);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb2d.velocity += Vector2.left * speed;
            Flip(Vector2.left.x);
        }
        
        if (Input.GetKey(KeyCode.Space) && IsGrounded())
        {
            rb2d.velocity += Vector2.up * jumpVelocity;

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
        //Debug.Log(maxSpeed);
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


        switch (unitMode)
        {
            case UnitMode.Mining:


                //Vector3 mousePos = Input.mousePosition;
                //mousePos.z = Camera.main.nearClipPlane;
                //worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
                //targetBlockIntPos = Vector3Int.FloorToInt(worldPosition);
                //targetBlockIntPos.z = 0;
                //Ray whereMouseIs = _camera.ScreenPointToRay((worldPosition - transform.position) - worldPosition);
                //RaycastHit2D precisionModeRay = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                //RaycastHit2D FreeModeRay = Physics2D.Raycast(transform.position, worldPosition - transform.position, 6f);

                //TargetedBlock = precisionModeRay.collider.GetComponent<Tilemap>();
                //if (TargetedBlock != null)
                //{ 
                //    float playerPositionX = Mathf.Abs(transform.position.x);
                //    float playerPositionY = Mathf.Abs(transform.position.y);

                //    float blockPositionX = Mathf.Abs(TargetedBlock.transform.position.x);
                //    float blockPositionY = Mathf.Abs(TargetedBlock.transform.position.y);

                //    DistanceFromPlayerX = Mathf.Abs(playerPositionX - blockPositionX);
                //    DistanceFromPlayerY = Mathf.Abs(playerPositionY - blockPositionY);

                //}

                //if (FreeModeRay && Input.GetMouseButton(0) && Input.GetKey("left shift"))
                //{
                //    RaycastHit2D freeMode = Physics2D.BoxCast(boxCollider2d.bounds.center, boxCollider2d.bounds.size *1.5f, 0f, boxCollider2d.bounds.center, .1f);

                //     TargetedBlock = FreeModeRay.collider.GetComponent<Tilemap>();
                //    // coroutineQueue.Enqueue(MineBlock());
                //    StartCoroutine(MineBlock());

                //}

                //else if (DistanceFromPlayerX < 7f && DistanceFromPlayerY < 7f && Input.GetMouseButton(0) && !Input.GetKey("left shift"))
                //{
                //   // coroutineQueue.Enqueue(MineBlock());
                //    StartCoroutine(MineBlock());

                //}

                break;
            case UnitMode.Combat:
                break;
        }
    }

    bool IsGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(boxCollider2d.bounds.center, boxCollider2d.bounds.size, 0f, Vector2.down, .1f);
        // Debug.Log(raycastHit2D.collider);
        return raycastHit2D.collider != null;
    }



    #region GameLogic
    public void SetMiningMode()
    {
        unitMode = UnitMode.Mining;
    }

    public void SetCombatMode()
    {
        unitMode = UnitMode.Combat;
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