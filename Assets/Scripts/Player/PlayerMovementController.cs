using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovementController : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] Rigidbody2D rb2d;
    private JumpController jumpController;
    private FaceMouse faceMouse;

    private Vector2 previousInput;

    public float jumpVelocity;
    public float maxFallSpeed;
    private float maxSpeed;
    float heightTimer;
    float widthTimer;
    float jumpTimer;
    private bool facingRight = true;
    private bool isJumping = false;

    public void Start()
    {
        if(!isLocalPlayer)
        {
            GetComponent<Rigidbody2D>().simulated = false;
        }
    }

    public override void OnStartAuthority()
    {
        enabled = true;

        rb2d = GetComponent<Rigidbody2D>();
        jumpController = GetComponent<JumpController>();
        faceMouse = GetComponentInChildren<FaceMouse>();

        InputManager.Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        InputManager.Controls.Player.Move.canceled += ctx => ResetMovement();

        InputManager.Controls.Player.Jump.performed += ctx => isJumping = true;
        InputManager.Controls.Player.Jump.canceled += ctx => isJumping = false;
    }
    
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        faceMouse.RotateArm();
    }

    [ClientCallback]
    private void FixedUpdate() => Move();

    [Client]
    private void SetMovement(Vector2 movement) => previousInput = movement;
    [Client]
    private void ResetMovement() => previousInput = Vector2.zero;

    private void Move()
    {
        if (!isLocalPlayer)
            return;
        
        rb2d.velocity += previousInput * movementSpeed;
        Flip(previousInput.x);

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

        if (isJumping && jumpController.IsGrounded())
        {
            jumpController.Jump();
        }
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
}