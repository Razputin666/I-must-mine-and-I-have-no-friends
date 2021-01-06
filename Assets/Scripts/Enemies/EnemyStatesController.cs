using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyStatesController : MonoBehaviour
{

    private EnemyController enemy;
    private MiningController miningMode;
    private TileMapChecker tileMapChecker;
    private Tilemap targetedChunk;


    Vector3Int[] blockPositions;
    Vector3Int targetBlockIntPos;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<EnemyController>();
        tileMapChecker = GetComponentInChildren<TileMapChecker>();
        miningMode = GetComponentInChildren<FacePlayer>().GetComponentInChildren<MiningController>();
        GameObject tileMapAtStart = GameObject.FindGameObjectWithTag("TileMap");
        TargetedChunk = tileMapAtStart.GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (enemy.enemyStates)
        {
            case EnemyController.EnemyStates.FrogMining:



                Vector3 distanceToTarget = enemy.target;


                if (distanceToTarget.x > 0)
                {
                    distanceToTarget.x = 1;
                }
                else if(distanceToTarget.x < 0)
                {
                    distanceToTarget.x = -1;
                }
                if(distanceToTarget.y > 0)
                {
                    distanceToTarget.y = 1;
                }
                else if (distanceToTarget.y < 0)
                {
                    distanceToTarget.y = -1;
                }

               
                
                Vector3Int enemyIntPos = Vector3Int.FloorToInt(transform.position);
                targetBlockIntPos = enemyIntPos + new Vector3Int ((int)distanceToTarget.x, (int)distanceToTarget.y, 0);
                targetBlockIntPos.z = 0;
                Vector3Int distanceFromEnemy = targetBlockIntPos - enemyIntPos;

                Tilemap chunk = miningMode.GetChunk(targetBlockIntPos);

                bool foundBlock = false;
                if (chunk == null)
                {
                    for (int j = 1; j <= 2 && foundBlock == false; j++)
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
               else if(chunk.HasTile(targetBlockIntPos))
               {
                    foundBlock = true;
                    
               }
               else
               {
                    for (int j = 0; j <= 2 && foundBlock == false; j++)
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

                if(!foundBlock)
                {

                    return;
                }
              //  if (enemy.DistanceGained >= transform.position.magnitude)
               // {
                   // DestroySurroundingBlocks();
               // }
                if (distanceFromEnemy.x > -10 && distanceFromEnemy.x < 10 && distanceFromEnemy.y > -10 && distanceFromEnemy.y < 10)
                {
                    miningMode.Mine(targetBlockIntPos, enemy.miningStrength);
                 
                }

                break;
            case EnemyController.EnemyStates.FrogAggressive:
                break;
            default:
                break;
        }
    }

  

    public Tilemap TargetedChunk
    {
        get
        {
            return targetedChunk;

        }

        set
        {
            targetedChunk = value;
        }
    }

    #region NextBlockGetter
    private Vector3Int[] GetNextBlocks(Vector3 distanceToTarget, int blocksOut)
    {
        Vector3Int[] newBlockPos = new Vector3Int[8];

        if(distanceToTarget.x > 0 && distanceToTarget.y > 0)
        {
            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
        }

        else if(distanceToTarget.x > 0 && distanceToTarget.y <= 0)
        {
            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
        }

        else if(distanceToTarget.x <= 0 && distanceToTarget.y <= 0)
        {
            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
        }

        else if(distanceToTarget.x <= 0 && distanceToTarget.y > 0)
        {
            newBlockPos[0] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
            newBlockPos[1] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
            newBlockPos[2] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
            newBlockPos[3] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
            newBlockPos[4] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
            newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
            newBlockPos[6] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
            newBlockPos[7] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
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
            }

            else if (distanceToTarget.x <= 0)
            {
                newBlockPos[0] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
                newBlockPos[1] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
                newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
                newBlockPos[3] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
                newBlockPos[4] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
                newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
                newBlockPos[6] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
                newBlockPos[7] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
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
            }

            else if (distanceToTarget.y <= 0)
            {
                newBlockPos[0] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y - blocksOut, 0);// down
                newBlockPos[1] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y - blocksOut, 0);//bottomright
                newBlockPos[2] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y - blocksOut, 0); //bottomleft
                newBlockPos[3] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y, 0);// right
                newBlockPos[4] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y, 0);// left
                newBlockPos[5] = new Vector3Int(targetBlockIntPos.x + blocksOut, targetBlockIntPos.y + blocksOut, 0); // topright
                newBlockPos[6] = new Vector3Int(targetBlockIntPos.x - blocksOut, targetBlockIntPos.y + blocksOut, 0);//topleft
                newBlockPos[7] = new Vector3Int(targetBlockIntPos.x, targetBlockIntPos.y + blocksOut, 0);// up
            }
        }
     

        return newBlockPos;
    }
    #endregion
}
