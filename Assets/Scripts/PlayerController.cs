using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    LayerMask blockLayerMask;

    public float speed;                //Floating point variable to store the player's movement speed.
    public float jumpVelocity;
    private Rigidbody2D rb2d;        //Store a reference to the Rigidbody2D component required to use 2D Physics.
    private BoxCollider2D boxCollider2d;

    // Use this for initialization
    [SerializeField]
    Camera _camera;

    void Start()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        rb2d = GetComponent<Rigidbody2D>();
        boxCollider2d = transform.GetComponent<BoxCollider2D>();
       
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

        //Debug.Log(IsGrounded());
    }

    bool IsGrounded()
    {
       RaycastHit2D raycastHit2D = Physics2D.BoxCast(boxCollider2d.bounds.center, boxCollider2d.bounds.size, 0f, Vector2.down, .1f);
       // Debug.Log(raycastHit2D.collider);
        return raycastHit2D.collider != null;
        
             
        
    }


}

