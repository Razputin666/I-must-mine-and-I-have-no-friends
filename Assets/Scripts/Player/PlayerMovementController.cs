using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Tilemaps;

public class PlayerMovementController : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private Transform armTransform;
    [SerializeField] private PlayerController player;
    private JumpController jumpController;
    //private FaceMouse faceMouse;

    private Vector2 previousInput;

    [SerializeField] private LimbMovement legMovement;
    public float jumpVelocity;
    public float maxFallSpeed;
    float heightTimer;
    float widthTimer;
    //float jumpTimer;
    private bool isJumping = false;

    //Swinging stuff
    [SerializeField] private Transform backSwingTarget;
    [SerializeField] private Transform frontSwingTarget;
    float swingTimer;
    private bool isSwinging;
    private bool swingInMotion;

    public override void OnStartServer()
    {
        rb2d = GetComponent<Rigidbody2D>();
        jumpController = GetComponent<JumpController>();
        //faceMouse = GetComponentInChildren<FaceMouse>();
        //armTransform = transform.Find("Gubb_arm");
        rb2d.simulated = true;
    }

    public override void OnStartLocalPlayer()
    {
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
            //if(isServer && GetComponent<PlayerController>().IsReady)
            //{
            //    if (!isSwinging && !swingInMotion)
            //    RotateArm();
            //    else if (!swingInMotion)
            //    {
            //       StartCoroutine(SwingAt(armTransform));
            //    }
            //    //PathfindingTest();

            //}

            //if (isLocalPlayer)
            //{
            //    PlayerController player = GetComponent<PlayerController>();
            //   Vector2 direction = new Vector2(
            // Mathf.Clamp(Mathf.Abs(player.mousePosInWorld.x), 0, 1f),
            //  Mathf.Clamp(Mathf.Abs(player.mousePosInWorld.y), 0, 1f));
            //     Debug.Log(Vector2.Dot(direction , armTransform.up));
            //   // Debug.Log(direction);
            //    if (Input.GetMouseButton(0))
            //    {
            //        CmdSwingAt(true);
            //        isSwinging = true;
            //    }
            //    else if (isSwinging)
            //    {
            //        CmdSwingAt(false);
            //        isSwinging = false;
            //    }
            //}
            //if (isClient)
            //{
            //    if (Input.GetMouseButtonDown(0))
            //    {
            //        PlayerController player = GetComponent<PlayerController>();

            //        CmdSetTargetPath(player.mousePosInWorld);
            //    }
            //    if (Input.GetMouseButtonDown(1))
            //    {
            //        PlayerController player = GetComponent<PlayerController>();

            //    CmdSetTargetPath(player.mousePosInWorld);
            //}
        }
    }

    private IEnumerator SwingAt()
    {
        int heavyness = 10; // represents how heavy a melee weapon is. Should be brought along with a parameter
        float strength = 1; // represents strength of user
        swingInMotion = true;
        swingTimer = 1f;

        
        Vector3 difference = backSwingTarget.position - armTransform.position;
        difference.Normalize();
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        Quaternion backswing = Quaternion.Euler(armTransform.rotation.x, armTransform.rotation.y, rotationZ);
        while (isSwinging || Mathf.Abs(Quaternion.Dot(armTransform.rotation, backswing)) < 0.9999f)
        {
            difference = backSwingTarget.position - armTransform.position;
            difference.Normalize();
            rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            backswing = Quaternion.Euler(armTransform.rotation.x, armTransform.rotation.y, rotationZ);

            armTransform.rotation = Quaternion.Lerp(armTransform.rotation, backswing, (heavyness - strength) * Time.deltaTime);
           // Debug.Log(Mathf.Abs(Quaternion.Dot(armTransform.rotation, backswing)));
            yield return null;
        }
        difference = player.mousePosInWorld - armTransform.position;
        difference.Normalize();
        rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        Quaternion targetSwing = Quaternion.Euler(armTransform.rotation.x, armTransform.rotation.y, rotationZ);
        while (Mathf.Abs(Quaternion.Dot(armTransform.rotation, targetSwing)) < 0.9999f)
        {
            armTransform.rotation = Quaternion.Lerp(armTransform.rotation, targetSwing, heavyness * Time.deltaTime);
            Debug.Log(Mathf.Abs(Quaternion.Dot(armTransform.rotation, targetSwing)));
            yield return null;
        }
        swingInMotion = false;

    }


    [Command]
    private void CmdSwingAt(bool swing)
    {
        isSwinging = swing;
    }
    
    private void FixedUpdate()
    {
        if (isServer && GetComponent<PlayerController>().IsReady)
        {
            Move();

            if (!isSwinging && !swingInMotion)
            {
                RotateArm();
            }
            else if (!swingInMotion)
            {
                StartCoroutine(SwingAt());
            }
        }

        if (isLocalPlayer)
        {
            if (Input.GetMouseButton(0) && player.playerStates == PlayerController.PlayerStates.Swinging)
            {
                CmdSwingAt(true);
                isSwinging = true;
            }
            else if (isSwinging)
            {
                CmdSwingAt(false);
                isSwinging = false;
            }
        }
    }

    [Client]
    private void SetMovement(Vector2 movement)
    {
        previousInput = movement;
        CmdInputChanged(previousInput);
    }
    [Client]
    private void ResetMovement() 
    {
        previousInput = Vector2.zero;
        CmdInputChanged(previousInput);
    }

    [Command]
    private void CmdInputChanged(Vector2 input)
    {
        previousInput = input;
    }

    [Command]
    private void CmdJumpChanged(bool isJumping)
    {
        this.isJumping = isJumping;
    }
    
    private void Move()
    {
        Flip(previousInput.x);

        CheckMaxSpeed();

        if (IsJumping && jumpController.IsGrounded())
        {
            jumpController.Jump();
        }
    }

    private void CheckMaxSpeed()
    {
        if (previousInput != Vector2.zero)
        {
            if (legMovement != null)
                legMovement.MoveFootTarget(previousInput);
            else
                rb2d.velocity += previousInput * movementSpeed * Time.fixedDeltaTime;
        }

        if (rb2d.velocity.y < -25f)
        {
            heightTimer += Time.deltaTime;
            maxFallSpeed = Mathf.Clamp(heightTimer, 1f, 5f);
            rb2d.velocity -= Vector2.down * maxFallSpeed;
            if (rb2d.velocity.y > -35f)
                heightTimer = 1f;
        }

        float currentSpeed = Mathf.Abs(rb2d.velocity.x);

        if(currentSpeed > 25f)
        {
            widthTimer += Time.deltaTime;
            float asedf = Mathf.Clamp(widthTimer, 1f, 3f);

            Vector2 oppositeDirection = rb2d.velocity.x > 0f ? Vector2.right : Vector2.left;
            rb2d.velocity -= oppositeDirection * asedf;
            if (currentSpeed > 30f)
                widthTimer = 1f;

        }
    }

    private void Flip(float horizontal)
    {
        if (horizontal > 0)
        {
            transform.rotation = new Quaternion(transform.rotation.x, 0, transform.rotation.z, transform.rotation.w);
        }
        else if (horizontal < 0)
        {
            transform.rotation = new Quaternion(transform.rotation.x, 180, transform.rotation.z, transform.rotation.w);
        }
    }

    public void RotateArm()
    {
        
        //Vector2 direction = 
        //player.mousePosInWorld - armTransform.position;

        //armTransform.up = direction;
        Vector3 difference = player.mousePosInWorld - armTransform.position;
        difference.Normalize();

        float rotationz = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        armTransform.rotation = Quaternion.Euler(0, 0, rotationz);

        //if (rotationz < -90 || rotationz > 90)
        //{
        //    if (player.transform.eulerangles.y == 0)
        //    {
        //        armtransform.localrotation = quaternion.Euler(180, 0, -rotationZ);
        //    }
        //    else if (player.transform.eulerAngles.y == 180)
        //    {
        //        armTransform.localRotation = Quaternion.Euler(180, 180, -rotationZ);
        //    }
        //}
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