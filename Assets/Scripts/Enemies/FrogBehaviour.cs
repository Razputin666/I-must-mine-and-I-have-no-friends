using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
public class FrogBehaviour : EnemyBehaviour
{
    [SerializeField] private MiningController miningMode;
    [SerializeField] private Transform arm;
    //[SerializeField] private PathfindingCustom pathfinding;
    [SerializeField] private Transform item;

    private float findTargetTimer;
    private float distanceCheckTimer;
    

    private int blockAmount;
    private int oreAmount;

    private float nextDistance;
    private float previousDistance;

    //Path Variables
    private Vector3 previousPosition;
    private float timeStoodStill;
    private Vector2 moveDir;
    private float playerSearchTimer;
    private float movedCheckTimer;

    //Item pickup variables
    [SerializeField] private float itemPickupRange;
    private float itemPickupCooldown;
    private const float itemPickupCooldownDefault = 0.5f;

    public override void OnStartServer()
    {
        Init();
        
        SetIdleMode();
        enemyTypes = EnemyBehaviour.EnemyTypes.Frog;
        //Randomize starting direction
        moveDir = Random.Range(0, 100) > 50 ? Vector2.right : Vector2.left;
        timeStoodStill = 0f;
        playerSearchTimer = 0f;
        movedCheckTimer = 0f;
        previousPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer)
            return;

        playerSearchTimer += Time.deltaTime;
        itemPickupCooldown += Time.deltaTime;
        movedCheckTimer += Time.deltaTime;
        CheckState();

        if (itemPickupCooldown >= itemPickupCooldownDefault) // Check for item drops.
        {
            CheckItemCollision();
            itemPickupCooldown = 0f;
        }
        if (movedCheckTimer > 0.2f) // update the previous position every 0.2s to check if we have moved enough while in idle state.
        {
            previousPosition = transform.position;
            movedCheckTimer = 0f;
        }
        //Check for a player every 3s.
        if (playerSearchTimer >= 3f)
        {
            if(FindPlayerTarget()) // if we found a player the set our state to aggressive
            {
                SetState(EnemyStates.FrogAggressive);
            }
            else // else get an alternate target
            {
                AlternativeTarget();
            }
            playerSearchTimer = 0f;
        }

        //ArmDirection();
        //if (!limbMovement.isSwinging)
        //{
        //    MoveBody();
        //}
        //findTargetTimer += Time.deltaTime;
        //distanceCheckTimer += Time.deltaTime;
        //if (findTargetTimer > 0.1f)
        //{
        //    previousDistance = 0f;
        //    previousDistance = Vector2.Distance(transform.position, target);
        //    if(FindPlayerTarget())
        //    {
        //        SetAggressiveMode();
        //    }
        //    //Debug.DrawLine(transform.position, target);

        //    findTargetTimer = 0f;

        //}
        //if (distanceCheckTimer > 5f)
        //{
        //    nextDistance = 0f;
        //    nextDistance = Vector2.Distance(transform.position, target);
        //    distanceCheckTimer = 0f;
        //}

        //if (foundPlayer && miningMode.enabled == true)
        //{
        //    SetAggressiveMode();
        //}
        //else if (!foundPlayer && miningMode.enabled == false)
        //{
        //    SetMiningMode();
        //}
        //Debug.Log(Mathf.Abs(nextDistance) - Mathf.Abs(previousDistance));
        //if (Mathf.Abs(nextDistance) - Mathf.Abs(previousDistance) < 5f)
        //    Mining();
    }

    protected virtual void FixedUpdate()
    {
        if (!isServer)
            return;

        DeathCheck();
        
        moveDir.Normalize();
        Flip(moveDir.x);
        rb2d.velocity += moveDir * stats.moveSpeed * Time.fixedDeltaTime;

        MaxSpeedCheck();
    }

    //protected override Vector3 FindTarget()
    //{

    //        blockAmount = GetBlockAmount();
    //        oreAmount = GetOreAmount();
    //        if (blockAmount >= 50 || oreAmount >= 10)
    //        {
    //           return target = frogBase.transform.position - transform.position;  
    //        }
    //        else
    //        {
    //           return target = frogBase.targetedOre[frogBase.targetedOre.Count - 1] - transform.position;
    //            //  distanceToTarget = Vector3.Distance(frogBase.targetedOre[frogBase.targetedOre.Count - 1], transform.position);
    //        }
    //}

    /// <summary>
    /// Get an alternate target to a player target.
    /// </summary>
    protected override void AlternativeTarget()
    {
        blockAmount = GetBlockAmount();
        oreAmount = GetOreAmount();
        if (blockAmount >= 50 || oreAmount >= 10) // if we have enough ore or blocks the return back to base.
        {
            SetState(EnemyStates.FrogReturn);

            //PathfindingDots.Instance.CalculatePaths(
            //    this,
            //    Vector3Int.FloorToInt(transform.position),
            //    Vector3Int.FloorToInt(frogBase.transform.position));
            pathfinder.CalculatePath(
                Vector3Int.FloorToInt(transform.position),
                Vector3Int.FloorToInt(frogBase.transform.position));
        }
        else
        {
            Vector3 oreTarget = frogBase.GetOreTarget(transform.position);
            if(oreTarget != Vector3.zero) // if we got a position for ore
            {
                //SetState(EnemyStates.FrogMining);

                //PathfindingDots.Instance.CalculatePaths(
                //this,
                //Vector3Int.FloorToInt(transform.position),
                //Vector3Int.FloorToInt(oreTarget));
                Debug.Log(oreTarget);
                
                if (pathfinder.CalculatePath(
                Vector3Int.FloorToInt(transform.position),
                Vector3Int.FloorToInt(oreTarget)))
                {
                    SetState(EnemyStates.FrogMining); //If we found a path towards the ore the set our state to mining.
                }
                else // else set our state to idle.
                {
                    SetState(EnemyStates.FrogIdle);
                }
            }
            else
            {
                SetState(EnemyStates.FrogIdle);
            }
            
            //return target = frogBase.GetOreTarget();
            //  distanceToTarget = Vector3.Distance(frogBase.targetedOre[frogBase.targetedOre.Count - 1], transform.position);
        }
    }

    //public override void SetPath(List<Vector3> path)
    //{
    //    if (!pathfinder.SetPath(path))
    //    {
    //        SetState(prevState);
    //    }
    //}

    private void MoveBody()
    {
        Flip();
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(target.x, transform.position.y), stats.moveSpeed * Time.deltaTime);
        if (rb2d.velocity.x > 0f)
        {
            limbMovement.MoveFootTarget(Vector2.right);
        }
        else if (rb2d.velocity.x < 0f)
        {
            limbMovement.MoveFootTarget(Vector2.left);
        }

    }

    #region HugeCode
    //private void Mining()
    //{
    //    Vector3 distanceToTarget = target - transform.position;


    //    if (distanceToTarget.x > 1)
    //    {
    //        distanceToTarget.x = 1;
    //    }
    //    else if (distanceToTarget.x < -1)
    //    {
    //        distanceToTarget.x = -1;
    //    }
    //    else
    //    {
    //        distanceToTarget.x = 0;
    //    }
    //    if (distanceToTarget.y > 1)
    //    {
    //        distanceToTarget.y = 1;
    //    }
    //    else if (distanceToTarget.y < -1)
    //    {
    //        distanceToTarget.y = -1;
    //    }
    //    else
    //    {
    //        distanceToTarget.y = 0;
    //    }



    //    Vector3Int enemyIntPos = Vector3Int.FloorToInt(new Vector3(transform.position.x, transform.position.y - 1));
    //    targetBlockIntPos = enemyIntPos + new Vector3Int((int)distanceToTarget.x, (int)distanceToTarget.y, 0);
    //    targetBlockIntPos.z = 0;
    //    Vector3Int distanceFromEnemy = targetBlockIntPos - enemyIntPos;

    //    Tilemap chunk = miningMode.GetChunk(targetBlockIntPos);

    //    bool foundBlock = false;
    //    if (chunk == null)
    //    {
    //        for (int j = 1; j <= 3 && foundBlock == false; j++)
    //        {
    //            Vector3Int[] newBlockPos = GetNextBlocks(distanceToTarget, j);
    //            for (int i = 0; i < newBlockPos.Length; i++)
    //            {
                    
    //                chunk = miningMode.GetChunk(newBlockPos[i]);
    //                if (chunk != null && chunk.HasTile(chunk.WorldToCell(newBlockPos[i])))
    //                {
    //                    targetBlockIntPos = newBlockPos[i];
    //                    foundBlock = true;

    //                    break;
    //                }
    //            }
    //        }
    //    }
    //    else if (chunk.HasTile(targetBlockIntPos))
    //    {
    //        foundBlock = true;

    //    }
    //    else
    //    {
    //        for (int j = 0; j <= 3 && foundBlock == false; j++)
    //        {
    //            Vector3Int[] newBlockPos = GetNextBlocks(distanceToTarget, j);
    //            for (int i = 0; i < newBlockPos.Length; i++)
    //            {
                    
    //                chunk = miningMode.GetChunk(newBlockPos[i]);
    //                if (chunk != null && chunk.HasTile(chunk.WorldToCell(newBlockPos[i])))
    //                {

    //                    targetBlockIntPos = newBlockPos[i];

    //                    foundBlock = true;

    //                    break;
    //                }
    //            }
    //        }
    //    }
        
    //    if (!foundBlock)
    //    {
            
    //        return;
    //    }
    //    //  if (enemy.DistanceGained >= transform.position.magnitude)
    //    // {
    //    // DestroySurroundingBlocks();
    //    // }
    //    if (distanceFromEnemy.x > -10 && distanceFromEnemy.x < 10 && distanceFromEnemy.y > -10 && distanceFromEnemy.y < 10)
    //    {
            
    //        target = targetBlockIntPos;
    //        miningMode.Mine(targetBlockIntPos, stats.mineStrength);

    //    }
    //}

    //private Vector3Int[] GetNextBlocks(Vector3 distanceToTarget, int blocksOut)
    //{
    //    Vector3Int[] newBlockPos = new Vector3Int[8];
    //    if (distanceToTarget.x > 0 && distanceToTarget.y > 0)
    //    {
    //        newBlockPos[0] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
    //        newBlockPos[1] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
    //        newBlockPos[2] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
    //        newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
    //        newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
    //        newBlockPos[5] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
    //        newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
    //        newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
    //        Debug.Log(1);
    //    }

    //    else if (distanceToTarget.x > 0 && distanceToTarget.y < 0)
    //    {
    //        newBlockPos[0] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
    //        newBlockPos[1] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
    //        newBlockPos[2] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
    //        newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
    //        newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
    //        newBlockPos[5] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
    //        newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
    //        newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
    //        Debug.Log(2);
    //    }

    //    else if (distanceToTarget.x < 0 && distanceToTarget.y < 0)
    //    {
    //        newBlockPos[0] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
    //        newBlockPos[1] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
    //        newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
    //        newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
    //        newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
    //        newBlockPos[5] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
    //        newBlockPos[6] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
    //        newBlockPos[7] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
    //        Debug.Log(3);
    //    }

    //    else if (distanceToTarget.x < 0 && distanceToTarget.y > 0)
    //    {
    //        newBlockPos[0] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
    //        newBlockPos[1] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
    //        newBlockPos[2] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
    //        newBlockPos[3] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
    //        newBlockPos[4] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
    //        newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
    //        newBlockPos[6] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
    //        newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
    //        Debug.Log(4);
    //    }

    //    else

    //    {
    //        if (distanceToTarget.x > 0)
    //        {
    //            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
    //            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
    //            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
    //            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
    //            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
    //            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
    //            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
    //            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
    //            Debug.Log(5);
    //        }

    //        else if (distanceToTarget.x < 0)
    //        {
    //            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
    //            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
    //            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
    //            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
    //            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
    //            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
    //            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
    //            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
    //            Debug.Log(6);
    //        }

    //        if (distanceToTarget.y > 0)
    //        {
    //            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
    //            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
    //            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
    //            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
    //            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
    //            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
    //            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
    //            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
    //            Debug.Log(7);
    //        }

    //        else if (distanceToTarget.y < 0)
    //        {
    //            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
    //            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
    //            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
    //            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
    //            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
    //            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
    //            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
    //            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
    //            Debug.Log(8);
    //        }
    //    }

        
    //    return newBlockPos;
    //}

    private void SetMiningMode()
    {
        Debug.Log("Mining mode");
        prevState = state;
        state = EnemyStates.FrogMining;
        item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Diamond Pick 1");
        //item.GetComponent<PolygonCollider2D>().enabled = false;
    }

    private void SetAggressiveMode()
    {
        Debug.Log("Aggressive mode");
        prevState = state;
        state = EnemyStates.FrogAggressive;
        item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Diamond Sword");
        //item.GetComponent<PolygonCollider2D>().enabled = true;
    }
    private void SetIdleMode()
    {
        Debug.Log("Idle mode");
        prevState = state;
        state = EnemyStates.FrogIdle;
        item.GetComponent<SpriteRenderer>().sprite = null;
        //item.GetComponent<PolygonCollider2D>().enabled = false;
    }

    private void SetReturnMode()
    {
        Debug.Log("Return mode");
        prevState = state;
        state = EnemyStates.FrogReturn;
        item.GetComponent<SpriteRenderer>().sprite = null;
        //item.GetComponent<PolygonCollider2D>().enabled = false;
    }

    private void SetState(EnemyStates _state)
    {
        switch (_state)
        {
            case EnemyStates.FrogMining:
                SetMiningMode();
                break;
            case EnemyStates.FrogAggressive:
                SetAggressiveMode();
                break;
            case EnemyStates.FrogReturn:
                SetReturnMode();
                break;
            case EnemyStates.FrogIdle:
                SetIdleMode();
                break;
            default:
                break;
        }
    }

    private void CheckState()
    {
        switch (state)
        {
            case EnemyStates.FrogIdle:
                AlternativeTarget();
                if (Mathf.Abs(transform.position.x - previousPosition.x) < 0.1f)
                {
                    timeStoodStill += Time.deltaTime;

                    //Change direction if we stand still for too long
                    if (timeStoodStill >= 2f)
                    {
                        moveDir *= -1;
                        timeStoodStill = 0f;
                    }
                    else if (timeStoodStill >= 0.5f)//Try jumping if we are stuck
                    {
                        //GetComponent<JumpController>().Jump();
                    }
                }
                else
                {
                    timeStoodStill = 0f;
                }
                break;

            case EnemyStates.FrogAggressive:
                pathfinder.Move(); // Use the pathfinder to move

                //If the pathfinder is complete then get an alternate Target
                if (pathfinder.pathCompleted)
                {
                    AlternativeTarget();
                    break;
                }

                moveDir = pathfinder.Direction;

                break;
            case EnemyStates.FrogMining:
                pathfinder.Move(); // Use the pathfinder to move

                //If the pathfinder is complete then get an alternate Target
                if (pathfinder.pathCompleted)
                {
                    AlternativeTarget();
                    break;
                }

                moveDir = pathfinder.Direction;

                break;
            case EnemyStates.FrogReturn:
                pathfinder.Move(); // Use the pathfinder to move

                //If the pathfinder is complete then get an alternate Target
                if (pathfinder.pathCompleted)
                {
                    AlternativeTarget();
                    break;
                }

                moveDir = pathfinder.Direction;
                break;
            default:
                break;
        }
    }
    #endregion
    /// <summary>
    /// Check if we collide on the server with any items in the world and if so add it to the inventory and destroy it in the world
    /// </summary>
    [Server]
    private void CheckItemCollision()
    {
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, itemPickupRange);

        foreach (Collider2D collider2D in colliderArray)
        {
            if (collider2D.TryGetComponent<GroundItem>(out GroundItem groundItem))
            {
                Item newItem = new Item(groundItem.Item);
                //tell the client to add the item we collided with
                GetInventory.AddItem(newItem, newItem.Amount);
                NetworkServer.Destroy(collider2D.gameObject);
            }
        }
    }

    private int GetBlockAmount()
    {
        int items = 0;
        //List<int> listOfItems = new List<int>();

        if (GetInventory.FindItemsInInventory("Common") != null)
        {
            List<InventorySlot> commonItems = GetInventory.FindItemsInInventory("Common");
            for (int i = 0; i < commonItems.Count; i++)
            {
                items += commonItems[i].Amount;
                //listOfItems.Add(commonItems[i].Amount);
            }
            //for (int j = 0; j < listOfItems.Count; j++)
            //{
            //    items += listOfItems[j];
            //}
        }

        return items;
    }
    private int GetOreAmount()
    {
        int items = 0;
        //List<int> listOfItems = new List<int>();
        if (GetInventory.FindItemsInInventory("Ore") != null)
        {
            List<InventorySlot> oreItems = GetInventory.FindItemsInInventory("Ore");
            for (int i = 0; i < oreItems.Count; i++)
            {
                items += oreItems[i].Amount;
                //listOfItems.Add(oreItems[i].Amount);
            }
            //for (int j = 0; j < listOfItems.Count; j++)
            //{
            //    items += listOfItems[j];
            //}
        }

        return items;
    }

    public int OreAmount
    {
        get { return oreAmount; }

        set { oreAmount = value; }
    }

    public int BlockAmount
    {
        get { return blockAmount; }

        set { blockAmount = value; }
    }
}