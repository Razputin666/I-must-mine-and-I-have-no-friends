using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class JumpController : NetworkBehaviour, HasCoolDownInterFace
{
    [SerializeField] private int id = 1;
    [SerializeField] private float coolDownDuration;
    //[SerializeField] private PlayerController player;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private CoolDownSystem coolDownSystem = null;
    private Rigidbody2D rb2d;
    private Transform unit;
    private CapsuleCollider2D capsuleCollider2d;

    public int Id => id;
    public float CoolDownDuration => coolDownDuration;

    public void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        capsuleCollider2d = transform.GetComponent<CapsuleCollider2D>();
    }
  
    public bool IsGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.CapsuleCast(capsuleCollider2d.bounds.center, capsuleCollider2d.bounds.size, CapsuleDirection2D.Vertical, 0f, Vector2.down, 0.5f);
      //  Physics2D.IgnoreCollision(, GetComponent<CapsuleCollider2D>());
        return raycastHit2D.collider != null;
    }
    [Client]
    public void Jump()
    {
        if (!coolDownSystem.IsOnCoolDown(id))
        {
            GetComponent<Rigidbody2D>().AddForce(transform.up * jumpVelocity);
            coolDownSystem.PutOnCoolDown(this);
        }
    }
}