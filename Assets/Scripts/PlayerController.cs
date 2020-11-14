using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    LayerMask blockLayerMask;

    private enum UnitMode
    {
        Mining, Combat
    }

    private UnitMode unitMode;
    public float speed;                //Floating point variable to store the player's movement speed.
    public float jumpVelocity;
    private Rigidbody2D rb2d;        //Store a reference to the Rigidbody2D component required to use 2D Physics.
    private BoxCollider2D boxCollider2d;
    Vector3 worldPosition;

    public Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();


    [SerializeField]
    Camera _camera;

    void Start()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.
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

        //Store the current vertical input in the float moveVertical.
        float moveVertical = Input.GetAxis("Vertical");

        //Use the two store floats to create a new Vector2 variable movement.
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        //Call the AddForce function of our Rigidbody2D rb2d supplying movement multiplied by speed to move our player.
        rb2d.AddForce(movement * speed);
        _camera.transform.position = new Vector3(rb2d.transform.position.x, rb2d.transform.position.y, -1);
        if (Input.GetKey(KeyCode.Space) && IsGrounded())
        {
            rb2d.velocity = Vector2.up * jumpVelocity;
        }

        if (Input.GetKey(KeyCode.Space))
            unitMode = UnitMode.Mining;

                switch(unitMode)
        {
            case UnitMode.Mining:
             
                    RaycastHit2D hit;
                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = Camera.main.nearClipPlane;
                    worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

                    RaycastHit2D ray = Physics2D.Raycast(transform.position, worldPosition - transform.position, 10f);
                    Debug.DrawRay(transform.position, worldPosition - transform.position, Color.red);

                    if (ray && Input.GetMouseButton(0) && ray.collider.tag == "BlockTier1")
                    {
                     Destroy(ray.collider.gameObject);
                     Debug.Log(ray.collider);
                   // coroutineQueue.Enqueue(MineBlock(ray.collider));

                    }
                

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

    IEnumerator MineBlock(Collider2D block)
    {
       // yield return new WaitForSeconds(0.5f);

        if (block.gameObject != null)
        { 
            Destroy(block.gameObject);
            
        }

        yield return null;
    }

    #endregion

}

