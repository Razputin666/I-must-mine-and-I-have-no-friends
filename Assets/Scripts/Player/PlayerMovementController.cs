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

    private int currentPathIndex;
    private List<Vector3> pathVectorList;
    private Pathfinder pathfinder;

    public override void OnStartServer()
    {
        rb2d = GetComponent<Rigidbody2D>();
        jumpController = GetComponent<JumpController>();
        //faceMouse = GetComponentInChildren<FaceMouse>();
        armTransform = transform.Find("Gubb_arm");
        rb2d.simulated = true;
        pathfinder = GetComponent<Pathfinder>();
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
        if(isServer && GetComponent<PlayerController>().IsReady)
        {
            RotateArm();
            //PathfindingTest();
            pathfinder.Move();
        }

        if (isClient)
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlayerController player = GetComponent<PlayerController>();

                CmdSetTargetPath(player.mousePosInWorld);
            }
            if (Input.GetMouseButtonDown(1))
            {
                foreach (Vector3 pos in pathVectorList)
                {
                    Tilemap tilemap = GetTilemap(pos);

                    TileMapManager.Instance.UpdateTilemap(tilemap.name, tilemap.WorldToCell(pos), "Stone Block");
                }
            }
        }
    }
    #region PF Testing
    [Command]
    private void CmdSetTargetPath(Vector3 targetPositon)
    {
        pathfinder.CalculatePath(transform.position + Vector3.down, targetPositon);
        //currentPathIndex = 0;
        ////Debug.Log(targetPositon);
        //pathVectorList = Pathfinding.Instance.FindPath(transform.position + Vector3.down, targetPositon);

        //if (pathVectorList != null && pathVectorList.Count > 1)
        //    pathVectorList.RemoveAt(0);

        //Debug.Log(transform.position + Vector3.down);
        //foreach (Vector3 pos in pathVectorList)
        //{
        //    Tilemap tilemap = GetTilemap(pos);
        //    if(tilemap != null)
        //        TileMapManager.Instance.UpdateTilemap(tilemap.name, tilemap.WorldToCell(pos), "Stone Block");
        //}
    }
    [Server]
    private Tilemap GetTilemap(Vector2 worldPosition)
    {
        List<Tilemap> tilemaps = TileMapManager.Instance.Tilemaps;
        foreach (Tilemap tilemap in tilemaps)
        {
            BoundsInt bounds = tilemap.cellBounds;
            Vector3 tilemapPos = tilemap.transform.position;

            if (Inside(tilemapPos, bounds.size, worldPosition))
                return tilemap;
        }

        return null;
    }

    [Server]
    private bool Inside(Vector3 pos, Vector3Int size, Vector2 point)
    {
        //Debug.Log("Tilemap Pos: " + pos);
        //Debug.Log("tilemap Bounds pos: " + new Vector3(pos.x + size.x, pos.y + size.y));
        //Debug.Log("MousePos: " + point);
        if (pos.x <= point.x &&
            point.x <= pos.x + size.x &&
            pos.y <= point.y &&
            point.y <= pos.y + size.y)
        {
            return true;
        }

        return false;
    }

    private void PathfindingTest()
    {
        if(pathVectorList != null)
        {
            Vector3 targetPosition = pathVectorList[currentPathIndex];
            Vector3Int adjustedTransform = Vector3Int.FloorToInt(transform.position + Vector3.down);
            if(PathRequiresMining(targetPosition))
            {

            }
            else if (Vector3.Distance(adjustedTransform, targetPosition) > 1.0f)
            {
                Debug.Log(targetPosition + " " + adjustedTransform);
                Vector3 moveDir = (targetPosition - adjustedTransform).normalized;
                Debug.Log(moveDir);
                if (moveDir.y > 0.5f)
                    IsJumping = true;
                else
                    IsJumping = false;
                moveDir.y = 0;
                previousInput = moveDir;
                float distanceBefore = Vector3.Distance(transform.position, targetPosition);
                //rb2d.velocity += new Vector2(moveDir.x, moveDir.y) * movementSpeed;
                //transform.position += moveDir * 5f * Time.deltaTime;
            }
            else
            {
                Debug.Log("Dist: " + Vector3.Distance(adjustedTransform, targetPosition));
                currentPathIndex++;
                if(currentPathIndex >= pathVectorList.Count)
                {
                    IsJumping = false;
                    pathVectorList = null;
                    previousInput = Vector2.zero;
                }
            }
        }
    }
    
    private bool PathRequiresMining(Vector3 targetPosition)
    {
        PathNode currentNode = Pathfinding.Instance.GetNode(targetPosition);
        if (currentNode.hasBlock)
        {
            Debug.Log("Mining 1 " + targetPosition);
            GetComponent<MiningController>().Mine(targetPosition, GetComponent<PlayerController>().MiningStrength);

            return true;
        }
        else
        {
            Vector3Int adjustedTransform = Vector3Int.FloorToInt(transform.position + Vector3.down);
            Debug.Log(targetPosition);
            Debug.Log(adjustedTransform);
            Vector3 moveDir = (targetPosition - adjustedTransform).normalized;
            Debug.Log(moveDir);

            Vector3 pos;

            if(adjustedTransform.y + 1 == targetPosition.y)//Check if we are moving upwards diagonally
            {
                if(targetPosition.x == adjustedTransform.x + 1 || targetPosition.x == adjustedTransform.x - 1)
                {
                    for (int i = 0; i < 4; i++) // Then remove blocks 4 tiles high so we can move
                    {
                        pos = targetPosition + Vector3.up * (i + 1);
                        PathNode upNeighbour = Pathfinding.Instance.GetNode(pos);

                        Debug.Log(upNeighbour);
                        if (upNeighbour != null && upNeighbour.hasBlock)
                        {
                            Debug.Log("Mining 2 " + pos);
                            GetComponent<MiningController>().Mine(pos, GetComponent<PlayerController>().MiningStrength);
                            return true;
                        }
                    }
                }
            }
            else if(targetPosition.y == adjustedTransform.y - 1) //Check if we are moving downwards
            {
                if (targetPosition.x == adjustedTransform.x + 1 || targetPosition.x == adjustedTransform.x - 1) // if we are moving diagonally downwards
                {
                    for (int i = 0; i < 4; i++) // Then remove blocks 4 tiles high so we can move
                    {
                        pos = targetPosition + Vector3.up * (i + 1);
                        PathNode upNeighbour = Pathfinding.Instance.GetNode(pos);
                        
                        Debug.Log(upNeighbour);
                        if (upNeighbour != null && upNeighbour.hasBlock)
                        {
                            Debug.Log("Mining 3 " + pos);
                            GetComponent<MiningController>().Mine(pos, GetComponent<PlayerController>().MiningStrength);
                            return true;
                        }
                    }
                }
                else // if we are moving straight down
                {
                    pos = targetPosition + Vector3.right;
                    //Then remove 1 block to the left and right of our target 
                    PathNode rightNeighbour = Pathfinding.Instance.GetNode(pos);
                    if (rightNeighbour != null && rightNeighbour.hasBlock)
                    {
                        Debug.Log("Mining 4 " + pos);
                        GetComponent<MiningController>().Mine(pos, GetComponent<PlayerController>().MiningStrength);
                        return true;
                    }
                    pos = targetPosition + Vector3.left;
                    PathNode leftNeighbour = Pathfinding.Instance.GetNode(pos);
                    if (leftNeighbour != null && leftNeighbour.hasBlock)
                    {
                        Debug.Log("Mining 5 " + pos);
                        GetComponent<MiningController>().Mine(pos, GetComponent<PlayerController>().MiningStrength);
                        return true;
                    }
                }
                 
            }
            else //if we are not moving up or down we are moving straight sideways
            {
                for (int i = 0; i < 4; i++) //Then remove block 2 tiles up from target
                {
                    pos = targetPosition + Vector3.up * (i + 1);
                    PathNode upNeighbour = Pathfinding.Instance.GetNode(pos);
                    if (upNeighbour != null && upNeighbour.hasBlock)
                    {
                        Debug.Log("Mining 6 " + pos);
                        GetComponent<MiningController>().Mine(pos, GetComponent<PlayerController>().MiningStrength);
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion
    private void FixedUpdate() => Move();

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

        previousInput = pathfinder.Direction;

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