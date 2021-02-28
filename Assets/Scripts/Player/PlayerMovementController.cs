using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerMovementController : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private Transform armTransform;
    [SerializeField] private PlayerController player;
    private JumpController jumpController;
    //private FaceMouse faceMouse;

    private Vector2 previousInput;

    public float jumpVelocity;
    public float maxFallSpeed;
    private float maxSpeed;
    float heightTimer;
    float widthTimer;
    //float jumpTimer;
    private bool facingRight = true;
    private bool isJumping = false;

    private int currentPathIndex;
    private List<Vector3> pathVectorList;

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

        //        Debug.Log(player.mousePosInWorld);
        //    }
        //}
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

    [Command]
    private void CmdSetTargetPath(Vector3 targetPositon)
    {
        currentPathIndex = 0;
        //Debug.Log(targetPositon);
        pathVectorList = Pathfinding.Instance.FindPath(transform.position, targetPositon);
        //Debug.Log(transform.position + Vector3.down);
        //foreach (Vector3 pos in pathVectorList)
        //{
        //    Debug.Log(pos);
        //}

        if (pathVectorList != null && pathVectorList.Count > 1)
            pathVectorList.RemoveAt(0);
    }

    private void PathfindingTest()
    {
        if(pathVectorList != null)
        {
            Vector3 targetPosition = pathVectorList[currentPathIndex];
            Vector3 adjustedTransform = transform.position + Vector3.down;

            if(PathRequiresMining(targetPosition))
            {

            }
            else if(Vector3.Distance(adjustedTransform, targetPosition) > 1.0f && Vector3.Distance(transform.position + Vector3.up, targetPosition) > 1.0f)
            {
                //Debug.Log(targetPosition + " " + adjustedTransform);
                Vector3 moveDir = (targetPosition - transform.position).normalized;
                //Debug.Log(moveDir);
                if (moveDir.y > 0.8)
                    IsJumping = true;
                else
                    IsJumping = false;

                previousInput = moveDir;
                float distanceBefore = Vector3.Distance(transform.position, targetPosition);
                //rb2d.velocity += new Vector2(moveDir.x, moveDir.y) * movementSpeed;
                //transform.position += moveDir * 5f * Time.deltaTime;
            }
            else
            {
                currentPathIndex++;
                if(currentPathIndex >= pathVectorList.Count)
                {
                    pathVectorList = null;
                    previousInput = Vector2.zero;
                }
            }
        }
    }
    
    private bool PathRequiresMining(Vector3 targetPosition)
    {
        PathNode currentNode = Pathfinding.Instance.GetNode(targetPosition);
        if (currentNode.isMineable)
        {
            Debug.Log("Mining 1 " + targetPosition);
            GetComponent<MiningController>().Mine(targetPosition, GetComponent<PlayerController>().MiningStrength);

            return true;
        }
        else
        {
            Vector3 adjustedTransform = transform.position;// + Vector3.down;
            Debug.Log(targetPosition);
            //Debug.Log(adjustedTransform);
            Vector3 moveDir = (targetPosition - adjustedTransform).normalized;
            //Debug.Log(moveDir);
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            {
                PathNode upNeighbour = Pathfinding.Instance.GetNode(targetPosition + Vector3.up);
                if (upNeighbour != null && upNeighbour.isMineable)
                {
                    Debug.Log("Mining 1 " + targetPosition + Vector3.up);
                    GetComponent<MiningController>().Mine(targetPosition + Vector3.up, GetComponent<PlayerController>().MiningStrength);
                    return true;
                }
                PathNode downNeighbour = Pathfinding.Instance.GetNode(targetPosition + Vector3.down);
                if (downNeighbour != null && downNeighbour.isMineable)
                {
                    Debug.Log("Mining 2 " + targetPosition + Vector3.down);
                    GetComponent<MiningController>().Mine(targetPosition + Vector3.down, GetComponent<PlayerController>().MiningStrength);
                    return true;
                }
            }
            else
            {
                PathNode rightNeighbour = Pathfinding.Instance.GetNode(targetPosition + Vector3.right);
                if (rightNeighbour != null && rightNeighbour.isMineable)
                {
                    Debug.Log("Mining 3 " + targetPosition + Vector3.right);
                    GetComponent<MiningController>().Mine(targetPosition + Vector3.right, GetComponent<PlayerController>().MiningStrength);
                    return true;
                }
                PathNode leftNeighbour = Pathfinding.Instance.GetNode(targetPosition + Vector3.left);
                if (leftNeighbour != null && leftNeighbour.isMineable)
                {
                    Debug.Log("Mining 4 " + targetPosition + Vector3.left);
                    GetComponent<MiningController>().Mine(targetPosition + Vector3.left, GetComponent<PlayerController>().MiningStrength);
                    return true;
                }
                return false;
            }
            //Debug.Log("Checking neighbours");
            //foreach (PathNode neigbourNode in currentNode.neighbourNodes)
            //{
            //    //Debug.Log(neigbourNode.ToString() + neigbourNode.isMineable);
            //    if (neigbourNode.isMineable)
            //    {
            //        Vector3 adjustedTransform = transform.position;// + Vector3.down;
            //        Vector3 moveDir = (targetPosition - adjustedTransform).normalized;

            //        if(Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            //        {
            //            //Vector3 neighbourPosition = Pathfinding.Instance.GetWorldPosition(neigbourNode.x, neigbourNode.y);
            //            //Debug.Log(targetPosition);
            //            //Debug.Log(neighbourPosition);
            //            //Debug.Log("Up Dist: " + Vector3.Distance(targetPosition + Vector3.up, neighbourPosition));
            //            //Debug.Log("Down Dist: " + Vector3.Distance(targetPosition + Vector3.down, neighbourPosition));
            //            //if (Pathfinding.Instance.GetNode(neighbourPosition).isMineable && Vector3.Distance(targetPosition + Vector3.up, neighbourPosition) < 1.5f)
            //            //{
            //            //    Debug.Log("Mining 2 " + targetPosition);
            //            //    GetComponent<MiningController>().Mine(neighbourPosition, GetComponent<PlayerController>().MiningStrength);
            //            //    return true;
            //            //}
            //            //else if (Pathfinding.Instance.GetNode(neighbourPosition).isMineable && Vector3.Distance(targetPosition + Vector3.down, neighbourPosition) < 1.5f)
            //            //{
            //            //    Debug.Log("Mining 3 " + targetPosition);
            //            //    GetComponent<MiningController>().Mine(neighbourPosition, GetComponent<PlayerController>().MiningStrength);
            //            //    return true;
            //            //}
            //            PathNode upNeighbour = Pathfinding.Instance.GetNode(targetPosition + Vector3.up);
            //            if (upNeighbour.isMineable)
            //            {
            //                Debug.Log("Mining 4 " + targetPosition);
            //                GetComponent<MiningController>().Mine(Pathfinding.Instance.GetWorldPosition(upNeighbour.x, upNeighbour.y), GetComponent<PlayerController>().MiningStrength);
            //                return true;
            //            }
            //            PathNode downNeighbour = Pathfinding.Instance.GetNode(targetPosition + Vector3.down);
            //            if (downNeighbour.isMineable)
            //            {
            //                Debug.Log("Mining 4 " + targetPosition);
            //                GetComponent<MiningController>().Mine(Pathfinding.Instance.GetWorldPosition(downNeighbour.x, downNeighbour.y), GetComponent<PlayerController>().MiningStrength);
            //                return true;
            //            }
            //            continue;
            //        }
            //        else
            //        {
            //            //Vector3 neighbourPosition = Pathfinding.Instance.GetWorldPosition(neigbourNode.x, neigbourNode.y);

            //            //Debug.Log(targetPosition);
            //            //Debug.Log(neighbourPosition);
            //            //Debug.Log("Right Dist: " + Vector3.Distance(targetPosition + Vector3.right, neighbourPosition));
            //            //Debug.Log("Left Dist: " + Vector3.Distance(targetPosition + Vector3.left, neighbourPosition));
            //            //if (Pathfinding.Instance.GetNode(neighbourPosition).isMineable && Vector3.Distance(targetPosition + Vector3.right, neighbourPosition) < 1.5f)
            //            //{
            //            //    Debug.Log("Mining 4 " + targetPosition);
            //            //    GetComponent<MiningController>().Mine(neighbourPosition, GetComponent<PlayerController>().MiningStrength);
            //            //    return true;
            //            //}
            //            //else if (Pathfinding.Instance.GetNode(neighbourPosition).isMineable && Vector3.Distance(targetPosition + Vector3.left, neighbourPosition) < 1.5f)
            //            //{
            //            //    Debug.Log("Mining 5 " + targetPosition);
            //            //    GetComponent<MiningController>().Mine(neighbourPosition, GetComponent<PlayerController>().MiningStrength);
            //            //    return true;
            //            //}
            //            PathNode rightNeighbour = Pathfinding.Instance.GetNode(targetPosition + Vector3.right);
            //            if(rightNeighbour.isMineable)
            //            {
            //                Debug.Log("Mining 4 " + targetPosition);
            //                GetComponent<MiningController>().Mine(Pathfinding.Instance.GetWorldPosition(rightNeighbour.x, rightNeighbour.y), GetComponent<PlayerController>().MiningStrength);
            //                return true;
            //            }
            //            PathNode leftNeighbour = Pathfinding.Instance.GetNode(targetPosition + Vector3.left);
            //            if (leftNeighbour.isMineable)
            //            {
            //                Debug.Log("Mining 4 " + targetPosition);
            //                GetComponent<MiningController>().Mine(Pathfinding.Instance.GetWorldPosition(leftNeighbour.x, leftNeighbour.y), GetComponent<PlayerController>().MiningStrength);
            //                return true;
            //            }
            //            continue;
            //        }
            //    }
        }
        return false;
    }

    private void FixedUpdate()
    {
        Move();

        if (isServer && GetComponent<PlayerController>().IsReady)
        {
            if (!isSwinging && !swingInMotion)
                RotateArm();
            else if (!swingInMotion)
            {
                StartCoroutine(SwingAt());
            }
            //PathfindingTest();

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
        if (!isServer)
            return;

        if (!GetComponent<PlayerController>().IsReady)
            return;
        
        Flip(previousInput.x);
        rb2d.velocity += previousInput * movementSpeed;
        
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
        //if (horizontal > 0 && !facingRight || horizontal < 0 && facingRight)
        //{
        //    facingRight = !facingRight;

        //    Vector3 theScale = transform.localScale;
        //    theScale.x *= -1;
        //    transform.localScale = theScale;
        //}

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

    private bool IsJumping
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