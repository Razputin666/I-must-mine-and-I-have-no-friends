using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FrogBehaviour : EnemyBehaviour
{

    
    [SerializeField] private MiningController miningMode;
    [SerializeField] private Transform arm;
    [SerializeField] private PathfindingCustom pathfinding;
    [SerializeField] private Transform item;

    private float findTargetTimer;
    private float distanceCheckTimer;
    private EvilBeavisBaseController frogBase;

    private int blockAmount;
    private int oreAmount;
    private Vector3 frogTarget;
    private Vector3Int targetBlockIntPos;

    private float nextDistance;
    private float previousDistance;

    protected override void Start()
    {
        base.Start();
        frogBase = GameObject.FindWithTag("EnemyBase").GetComponent<EvilBeavisBaseController>();
        SetAggressiveMode();
        enemyTypes = EnemyBehaviour.EnemyTypes.Frog;
    }

    // Update is called once per frame
    void Update()
    {
        //ArmDirection();
        if (!limbMovement.isSwinging)
        {
            MoveBody();
        }
        findTargetTimer += Time.deltaTime;
        distanceCheckTimer += Time.deltaTime;
        if (findTargetTimer > 0.1f)
        {
            previousDistance = 0f;
            previousDistance = Vector2.Distance(transform.position, target);
            target = base.FindTarget();
            Debug.DrawLine(transform.position, target);
           
            findTargetTimer = 0f;

        }
        if (distanceCheckTimer > 5f)
        {
            nextDistance = 0f;
            nextDistance = Vector2.Distance(transform.position, target);
            distanceCheckTimer = 0f;
        }

        if (foundPlayer && miningMode.enabled == true)
        {
            SetAggressiveMode();
        }
        else if (!foundPlayer && miningMode.enabled == false)
        {
            SetMiningMode();
        }
        Debug.Log(Mathf.Abs(nextDistance) - Mathf.Abs(previousDistance));
        if (Mathf.Abs(nextDistance) - Mathf.Abs(previousDistance) < 5f)
            Mining();
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

    protected override Vector3 AlternativeTarget()
    {
        blockAmount = GetBlockAmount();
        oreAmount = GetOreAmount();
        if (blockAmount >= 50 || oreAmount >= 10)
        {
            return target = frogBase.transform.position;
        }
        else
        {
            
            return target = frogBase.targetedOre[frogBase.targetedOre.Count - 1];
            //  distanceToTarget = Vector3.Distance(frogBase.targetedOre[frogBase.targetedOre.Count - 1], transform.position);
        }
    }


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
    private void Mining()
    {
        Vector3 distanceToTarget = target - transform.position;


        if (distanceToTarget.x > 1)
        {
            distanceToTarget.x = 1;
        }
        else if (distanceToTarget.x < -1)
        {
            distanceToTarget.x = -1;
        }
        else
        {
            distanceToTarget.x = 0;
        }
        if (distanceToTarget.y > 1)
        {
            distanceToTarget.y = 1;
        }
        else if (distanceToTarget.y < -1)
        {
            distanceToTarget.y = -1;
        }
        else
        {
            distanceToTarget.y = 0;
        }



        Vector3Int enemyIntPos = Vector3Int.FloorToInt(new Vector3(transform.position.x, transform.position.y - 1));
        targetBlockIntPos = enemyIntPos + new Vector3Int((int)distanceToTarget.x, (int)distanceToTarget.y, 0);
        targetBlockIntPos.z = 0;
        Vector3Int distanceFromEnemy = targetBlockIntPos - enemyIntPos;

        Tilemap chunk = miningMode.GetChunk(targetBlockIntPos);

        bool foundBlock = false;
        if (chunk == null)
        {
            for (int j = 1; j <= 3 && foundBlock == false; j++)
            {
                Vector3Int[] newBlockPos = GetNextBlocks(distanceToTarget, j);
                for (int i = 0; i < newBlockPos.Length; i++)
                {
                    
                    chunk = miningMode.GetChunk(newBlockPos[i]);
                    if (chunk != null && chunk.HasTile(chunk.WorldToCell(newBlockPos[i])))
                    {
                        targetBlockIntPos = newBlockPos[i];
                        foundBlock = true;

                        break;
                    }
                }
            }
        }
        else if (chunk.HasTile(targetBlockIntPos))
        {
            foundBlock = true;

        }
        else
        {
            for (int j = 0; j <= 3 && foundBlock == false; j++)
            {
                Vector3Int[] newBlockPos = GetNextBlocks(distanceToTarget, j);
                for (int i = 0; i < newBlockPos.Length; i++)
                {
                    
                    chunk = miningMode.GetChunk(newBlockPos[i]);
                    if (chunk != null && chunk.HasTile(chunk.WorldToCell(newBlockPos[i])))
                    {

                        targetBlockIntPos = newBlockPos[i];

                        foundBlock = true;

                        break;
                    }
                }
            }
        }
        
        if (!foundBlock)
        {
            
            return;
        }
        //  if (enemy.DistanceGained >= transform.position.magnitude)
        // {
        // DestroySurroundingBlocks();
        // }
        if (distanceFromEnemy.x > -10 && distanceFromEnemy.x < 10 && distanceFromEnemy.y > -10 && distanceFromEnemy.y < 10)
        {
            
            target = targetBlockIntPos;
            miningMode.Mine(targetBlockIntPos, stats.mineStrength);

        }
    }


    private Vector3Int[] GetNextBlocks(Vector3 distanceToTarget, int blocksOut)
    {
        Vector3Int[] newBlockPos = new Vector3Int[8];
        if (distanceToTarget.x > 0 && distanceToTarget.y > 0)
        {
            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
            Debug.Log(1);
        }

        else if (distanceToTarget.x > 0 && distanceToTarget.y < 0)
        {
            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
            Debug.Log(2);
        }

        else if (distanceToTarget.x < 0 && distanceToTarget.y < 0)
        {
            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
            Debug.Log(3);
        }

        else if (distanceToTarget.x < 0 && distanceToTarget.y > 0)
        {
            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
            Debug.Log(4);
        }

        else

        {
            if (distanceToTarget.x > 0)
            {
                newBlockPos[0] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
                newBlockPos[1] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
                newBlockPos[2] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
                newBlockPos[3] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
                newBlockPos[4] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
                newBlockPos[5] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
                newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
                newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
                Debug.Log(5);
            }

            else if (distanceToTarget.x < 0)
            {
                newBlockPos[0] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
                newBlockPos[1] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
                newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
                newBlockPos[3] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
                newBlockPos[4] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
                newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
                newBlockPos[6] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
                newBlockPos[7] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
                Debug.Log(6);
            }

            if (distanceToTarget.y > 0)
            {
                newBlockPos[0] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
                newBlockPos[1] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
                newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
                newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
                newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
                newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
                newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
                newBlockPos[7] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
                Debug.Log(7);
            }

            else if (distanceToTarget.y < 0)
            {
                newBlockPos[0] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
                newBlockPos[1] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
                newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
                newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
                newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
                newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
                newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
                newBlockPos[7] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
                Debug.Log(8);
            }
        }

        
        return newBlockPos;
    }

    private void SetMiningMode()
    {
        item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Diamond Pick 1");
        miningMode.enabled = true;
        item.GetComponent<PolygonCollider2D>().enabled = false;
    }

    private void SetAggressiveMode()
    {
        item.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Items/Diamond Sword");
        miningMode.enabled = false;
        item.GetComponent<PolygonCollider2D>().enabled = true;
    }
    #endregion

    private int GetBlockAmount()
    {
        int items = 0;
        List<int> listOfItems = new List<int>();

        if (GetInventory.FindItemsInInventory("Common") != null)
        {
            for (int i = 0; i < GetInventory.FindItemsInInventory("Common").Count; i++)
            {
                listOfItems.Add(GetInventory.FindItemsInInventory("Common")[i].Amount);
            }
            for (int j = 0; j < listOfItems.Count; j++)
            {
                items += listOfItems[j];
            }
        }

        return items;
    }
    private int GetOreAmount()
    {
        int items = 0;
        List<int> listOfItems = new List<int>();
        if (GetInventory.FindItemsInInventory("Ore") != null)
        {
            for (int i = 0; i < GetInventory.FindItemsInInventory("Ore").Count; i++)
            {
                listOfItems.Add(GetInventory.FindItemsInInventory("Ore")[i].Amount);
            }
            for (int j = 0; j < listOfItems.Count; j++)
            {
                items += listOfItems[j];
            }
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
