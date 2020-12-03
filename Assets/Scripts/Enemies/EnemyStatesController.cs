using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyStatesController : MonoBehaviour
{

    private EnemyController enemy;
    private MiningController miningMode;
    private TileMapChecker tileMapChecker;
    private Tilemap targetedBlock;


    Vector3Int[] blockPositions;
    Vector3Int targetBlockIntPos;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<EnemyController>();
        tileMapChecker = GetComponentInChildren<TileMapChecker>();
        miningMode = GetComponentInChildren<FacePlayer>().GetComponentInChildren<MiningController>();
        GameObject tileMapAtStart = GameObject.FindGameObjectWithTag("TileMap");
        TargetedBlock = tileMapAtStart.GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (enemy.enemyStates)
        {
            case EnemyController.EnemyStates.FrogMining:

                

                Vector3 temp = enemy.player.transform.position - transform.position;

                if (temp.x > 0)
                {
                    temp.x = 1;
                }
                else if(temp.x < 0)
                {
                    temp.x = -1;
                }
                if(temp.y > 0)
                {
                    temp.y = 1;
                }
                else if (temp.y < 0)
                {
                    temp.y = -1;
                }

                
                targetBlockIntPos.z = 0;
                Vector3Int enemyIntPos = Vector3Int.FloorToInt(transform.position);
                targetBlockIntPos = Vector3Int.FloorToInt(transform.position + temp);
                Vector3Int distanceFromEnemy = targetBlockIntPos - enemyIntPos;
               // TargetedBlock = tileMapChecker.currentTilemap;


                if(!TargetedBlock.HasTile(targetBlockIntPos))
                {
                    bool foundBlock = false;

                    for (int j = 1; j <= 2 && !foundBlock; j++)
                    {
                        Vector3Int[] newBlockPos = GetNextBlocks(temp, j);


                        for (int i = 0; i < newBlockPos.Length; i++)
                        {
                            if (targetedBlock.HasTile(newBlockPos[i]))
                            {
                                targetBlockIntPos = newBlockPos[i];
                                foundBlock = true;
                                break;
                            }
                        }
                    }


                }

                if (distanceFromEnemy.x > -5 && distanceFromEnemy.x < 5 && distanceFromEnemy.y > -5 && distanceFromEnemy.y < 5)
                {
                    miningMode.Mine(targetBlockIntPos, enemy.miningStrength);
                    //for (int i = 1; i <= 5; i++)
                    //{
                    //    Vector3Int[] lookingForPlayer = GetNextBlocks(temp, i);
                    //    for (int j = 0; j < lookingForPlayer.Length; j++)
                    //    {
                    //        if (targetedBlock.HasTile(lookingForPlayer[j]))
                    //        {
                    //            break;
                    //        }

                    //        else if (!targetedBlock.HasTile(lookingForPlayer[j]) && Vector3Int.FloorToInt(enemy.player.transform.position) == lookingForPlayer[j])
                    //        {
                    //            Debug.Log("sees player");
                    //            enemy.SetAggressiveMode();
                    //        }
                    //    }
                    //}
                }

                break;
            case EnemyController.EnemyStates.FrogAggressive:
                break;
            default:
                break;
        }
    }

    public Tilemap TargetedBlock
    {
        get
        {
            return targetedBlock;

        }

        set
        {
            targetedBlock = value;
        }
    }

    #region NextBlockGetter
    private Vector3Int[] GetNextBlocks(Vector3 temp, int blocksOut)
    {
        Vector3Int[] newBlockPos = new Vector3Int[8];

        if(temp.x > 0 && temp.y > 0)
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

        else if(temp.x > 0 && temp.y <= 0)
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

        else if(temp.x <= 0 && temp.y <= 0)
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

        else if(temp.x <= 0 && temp.y > 0)
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
            if (temp.x > 0)
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

            else if (temp.x <= 0)
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

            if (temp.y > 0)
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

            else if (temp.y <= 0)
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
