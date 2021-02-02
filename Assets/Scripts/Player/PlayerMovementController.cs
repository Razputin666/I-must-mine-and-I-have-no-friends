using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Tilemaps;

public class PlayerMovementController : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] Rigidbody2D rb2d;
    private JumpController jumpController;
    //private FaceMouse faceMouse;
    private Transform armTransform;
    private Vector2 previousInput;

    [SerializeField] private LimbMovement legMovement;
    public float jumpVelocity;
    public float maxFallSpeed;
    private float maxSpeed;
    float heightTimer;
    float widthTimer;
    //float jumpTimer;
    private bool facingRight = true;
    private bool isJumping = false;
    
    public override void OnStartServer()
    {
        rb2d = GetComponent<Rigidbody2D>();
        jumpController = GetComponent<JumpController>();
        //faceMouse = GetComponentInChildren<FaceMouse>();
        armTransform = transform.Find("Gubb_arm");
        rb2d.simulated = true;
    }

    public override void OnStartLocalPlayer()
    {
        //Callback events for input
        InputManager.Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        InputManager.Controls.Player.Move.canceled += ctx => ResetMovement();

        InputManager.Controls.Player.Jump.performed += ctx => IsJumping = true;
        InputManager.Controls.Player.Jump.canceled += ctx => IsJumping = false;
    }

    private void Update()
    {  
        //Only rotate the arm if it is done on the server and the player is ready
        if(isServer && GetComponent<PlayerController>().IsReady)
        {
            RotateArm();
            //pathfinder.Move();
        }

        if (isClient)
        {
            //if (Input.GetMouseButtonDown(1))
            //{
            //    PlayerController player = GetComponent<PlayerController>();

            //    CmdSetTargetPath(player.mousePosInWorld);
            //}
        }
    }
    private void FixedUpdate() => Move();

    [Client]
    private void SetMovement(Vector2 movement) // sets the movement direction and tells the server
    {
        previousInput = movement;
        CmdInputChanged(previousInput);
        
    }
    [Client]
    private void ResetMovement() // sets the movement direction to 0 and tells the server
    {
        previousInput = Vector2.zero;
        CmdInputChanged(previousInput);
    }

    [Command]
    private void CmdInputChanged(Vector2 input) // Updates movement direction on the server
    {
        previousInput = input;
    }

    [Command]
    private void CmdJumpChanged(bool isJumping) // updates isJumping variable on the server
    {
        this.isJumping = isJumping;
    }
    
    private void Move()
    {
        if (!isServer)
            return;

        if (!GetComponent<PlayerController>().IsReady)
            return;

        //previousInput = pathfinder.Direction;

        Flip(previousInput.x);
        if(previousInput != Vector2.zero)
        {
            if (legMovement != null)
                legMovement.MoveFootTarget(previousInput);
            else
                rb2d.velocity += previousInput * movementSpeed;
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

        if (IsJumping && jumpController.IsGrounded())
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

    public void RotateArm()
    {
        PlayerController player = GetComponent<PlayerController>();
        //Transform armTransform = transform.Find("Gubb_arm");
        Vector2 direction = new Vector2(
        player.mousePosInWorld.x - armTransform.position.x,
        player.mousePosInWorld.y - armTransform.position.y);

        armTransform.up = direction;
    }

    public bool IsJumping
    {
        get { return isJumping; }
        set
        {
            isJumping = value;
            if(isClient)
                CmdJumpChanged(isJumping);
        }
    }
}