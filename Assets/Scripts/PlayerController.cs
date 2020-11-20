﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    LayerMask blockLayerMask;

    private enum UnitMode
    {
        Mining, Combat
    }
    private Collider2D targetedBlock;
    private UnitMode unitMode;            //Floating point variable to store the player's movement speed.
    private Rigidbody2D rb2d;        //Store a reference to the Rigidbody2D component required to use 2D Physics.
    private BoxCollider2D boxCollider2d;
    private bool facingRight;
    Vector3 worldPosition;
    private float distanceFromPlayerx;
    private float distanceFromPlayery;
    private bool inPrecisionMode;
    private bool inFreeMode;
    private bool movementIniated;

    [SerializeField]
    int movespeed;
    [SerializeField]
    int maxAccell;
    [SerializeField]
    float jumpVelocity;
    int accell;

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
       
        _camera.transform.position = new Vector3(rb2d.transform.position.x, rb2d.transform.position.y, -1);


        if (Input.GetKey(KeyCode.D))
        {
            Debug.Log("D PRESSED");
            Flip(1);
            for (int i = 0; i < maxAccell; i++)
            {
                rb2d.velocity = new Vector2((+movespeed + i * 0.001f), rb2d.velocity.y);

            }
            rb2d.velocity = new Vector2((+movespeed + maxAccell), rb2d.velocity.y);

        }


        else if (Input.GetKey(KeyCode.A))
        {
            Flip(-1);
            for (int i = 0; i < maxAccell; i++)
            {
                rb2d.velocity = new Vector2((-movespeed - i * 0.001f), rb2d.velocity.y);

            }
            rb2d.velocity = new Vector2((-movespeed - maxAccell), rb2d.velocity.y);

        }

        else
        {

           rb2d.velocity = new Vector2(0, rb2d.velocity.y);
        }


           if (Input.GetKey(KeyCode.Space) && IsGrounded())
           {
               rb2d.velocity += Vector2.up * jumpVelocity;
           }

        
        switch (unitMode)
        {
            case UnitMode.Mining:
             
                    
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane;
                worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
                Ray whereMouseIs = _camera.ScreenPointToRay((worldPosition - transform.position) - worldPosition);
                RaycastHit2D precisionModeRay = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                RaycastHit2D FreeModeRay = Physics2D.Raycast(transform.position, worldPosition - transform.position, 6f);

                TargetedBlock = precisionModeRay.collider;
                if (TargetedBlock != null)
                { 
                    float playerPositionX = Mathf.Abs(transform.position.x);
                    float playerPositionY = Mathf.Abs(transform.position.y);

                    float blockPositionX = Mathf.Abs(TargetedBlock.transform.position.x);
                    float blockPositionY = Mathf.Abs(TargetedBlock.transform.position.y);

                    DistanceFromPlayerX = Mathf.Abs(playerPositionX - blockPositionX);
                    DistanceFromPlayerY = Mathf.Abs(playerPositionY - blockPositionY);

                }
          
                if (FreeModeRay && Input.GetMouseButton(0) && Input.GetKey("left shift"))
                {
                    RaycastHit2D freeMode = Physics2D.BoxCast(boxCollider2d.bounds.center, boxCollider2d.bounds.size *1.5f, 0f, boxCollider2d.bounds.center, .1f);

                     TargetedBlock = FreeModeRay.collider;
                    // coroutineQueue.Enqueue(MineBlock());
                    StartCoroutine(MineBlock());

                }

                else if (DistanceFromPlayerX < 7f && DistanceFromPlayerY < 7f && Input.GetMouseButton(0) && !Input.GetKey("left shift"))
                {
                   // coroutineQueue.Enqueue(MineBlock());
                    StartCoroutine(MineBlock());

                }
                
                break;
            case UnitMode.Combat:
                break;
        }
    }


    bool IsGrounded()
    {
       RaycastHit2D raycastHit2D = Physics2D.BoxCast(boxCollider2d.bounds.center, boxCollider2d.bounds.size, 0f, Vector2.down, 1f);
       RaycastHit2D raycastHitWallLeft2D = Physics2D.BoxCast(boxCollider2d.bounds.center, boxCollider2d.bounds.size, 0f, Vector2.left, 0.5f);
       RaycastHit2D raycastHitWallRight2D = Physics2D.BoxCast(boxCollider2d.bounds.center, boxCollider2d.bounds.size, 0f, Vector2.right, 0.5f);

        // Debug.Log(raycastHit2D.collider);
        return raycastHit2D.collider != null;
        return raycastHitWallLeft2D.collider != null;
        return raycastHitWallRight2D.collider != null;
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

    private void AcceleratorHandle(int typeOfAccel)
    {

    }

    IEnumerator MineBlock()
    {
        int blockHP = 1;
        Collider2D thisTargetedBlock = TargetedBlock;
        while (Input.GetMouseButton(0) && TargetedBlock == thisTargetedBlock && TargetedBlock != null)
        {
            if(blockHP == 0)
            {
                TilemapVisual tilemapVisual = targetedBlock.transform.parent.GetComponent<TilemapVisual>();
                if (tilemapVisual != null)
                {
                    Tilemap.TilemapObject tilemapObject = tilemapVisual.GetGridObjectAtXY(targetedBlock.transform.position);
                    if (tilemapObject != null)
                    {
                        tilemapObject.SetTilemapSprite(Tilemap.TilemapObject.TilemapSprite.None);


                       // Destroy(TargetedBlock.gameObject);
                    }  
                }
                
            }
            blockHP -= 1;
            yield return new WaitForSeconds(0.05f);          
        }
             
    }

    public Collider2D TargetedBlock
    {
        get 
        {
            return targetedBlock; 

        }

        set 
        {
            targetedBlock = value;
        }
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

    public int Accell
    {
        get { return accell; }

        set { accell = value; }
    }
    #endregion
    private void Flip(float horizontal)
    {
        if(horizontal > 0 && !facingRight || horizontal < 0 && facingRight)
        {
            facingRight = !facingRight;

            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
}