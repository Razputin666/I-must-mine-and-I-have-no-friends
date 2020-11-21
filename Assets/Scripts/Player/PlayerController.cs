using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    LayerMask blockLayerMask;

    #region PlayerValues
    private Tilemap targetedBlock;
    
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
    private Transform item;


    [SerializeField]
    Camera _camera;

    void Start()
    {
        inventory = GetComponentInChildren<Inventory>();
        item = gameObject.GetComponentInChildren<FaceMouse>().gameObject.transform.Find("ItemHeldInHand");
        item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Drill");
        item.GetComponent<DefaultGun>().enabled = false;
        item.GetComponent<MiningController>().enabled = true;
        Debug.Log(item);
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        
        facingRight = true;
        rb2d = GetComponent<Rigidbody2D>();
        boxCollider2d = transform.GetComponent<BoxCollider2D>();
        capsuleCollider2d = transform.GetComponent<CapsuleCollider2D>();
       // itemController.SetMiningMode(); // Vi har inte combat än så den e på mining default
        StartCoroutine(CoroutineCoordinator());
    }

    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    void FixedUpdate()
    {

        mousePos = Input.mousePosition;
        worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        _camera.transform.position = new Vector3(rb2d.transform.position.x, rb2d.transform.position.y, -1);

        if(Input.GetKey(KeyCode.Alpha1))
        {
            // gameObject.GetComponentInChildren<ItemController>().gameObject.GetComponent<SpriteRenderer>().sprite = inventory.GetItem(0).Icon;
            item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Drill");
            item.GetComponent<DefaultGun>().enabled = false;
            item.GetComponent<MiningController>().enabled = true;

        }

        if(Input.GetKey(KeyCode.Alpha2))
        {
            // gameObject.GetComponentInChildren<ItemController>().gameObject.GetComponent<SpriteRenderer>().sprite = inventory.GetItem(0).Icon;
            item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Gun");
            item.GetComponent<MiningController>().enabled = false;
            item.GetComponent<DefaultGun>().enabled = true;
        }

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

    public bool IsGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.CapsuleCast(capsuleCollider2d.bounds.center, capsuleCollider2d.bounds.size, CapsuleDirection2D.Vertical, 0f, Vector2.down, 0.5f);
        return raycastHit2D.collider != null;
    }





    #region GameLogic

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