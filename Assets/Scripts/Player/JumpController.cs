using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpController : MonoBehaviour, HasCoolDownInterFace
{
    [SerializeField] private int id = 1;
    [SerializeField] private float coolDownDuration;
    //[SerializeField] private PlayerController player;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private CoolDownSystem coolDownSystem = null;
    [SerializeField] private Transform body;
    private Rigidbody2D rb2d;
    private BoxCollider2D collider2d;

    public int Id => id;
    public float CoolDownDuration => coolDownDuration;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<BoxCollider2D>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (!IsGrounded())
        //{
        //    return;
        //}
        

        //    if (coolDownSystem.IsOnCoolDown(id))
        //    { 
        //        return; 
        //    }

        // player.rb2d.AddForce(transform.up * player.jumpVelocity);
        //rb2d.AddForce(transform.up * jumpVelocity);
        //coolDownSystem.PutOnCoolDown(this);
    }

    public bool IsGrounded()
    {
         RaycastHit2D raycastHit2D = Physics2D.BoxCast(body.position, collider2d.bounds.size, 0f, Vector2.down, 0.8f, LayerMask.GetMask("Blocks"));
        //  Physics2D.IgnoreCollision(, GetComponent<CapsuleCollider2D>());
        return raycastHit2D.collider != null;
    }

    public void Jump()
    {
        if (!coolDownSystem.IsOnCoolDown(id))
        {
            rb2d.AddForce(transform.up * jumpVelocity);
            coolDownSystem.PutOnCoolDown(this);
        }
    }
}
