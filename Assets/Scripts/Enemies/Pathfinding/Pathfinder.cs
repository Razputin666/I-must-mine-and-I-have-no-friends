using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Pathfinder : NetworkBehaviour
{
    private List<Vector3> pathVectorList;

    private int currentPathIndex;
    public Vector2 Direction { get; private set; }

    [HideInInspector]public bool pathCompleted;

    private MiningController miningController;
    private EnemyBehaviour enemyController;
    public override void OnStartServer()
    {
        pathVectorList = null;
        pathCompleted = false;
        currentPathIndex = 0;
        miningController = GetComponent<MiningController>();
        enemyController = GetComponent<EnemyBehaviour>();
    }

    public bool CalculatePath(Vector3 startWorldPositon, Vector3 targetWorldPosition)
    {
        if(pathVectorList != null && pathVectorList.Count > 0)
        {
            //Debugging
            //Go through every Node in our previous path and reset the color.
            for (int i = 0; i < pathVectorList.Count; i++)
            {
                TileMapManager.Instance.ChangeTileColor(pathVectorList[i], Color.white);
            }
        }

        currentPathIndex = 0;
        
        Utils.Timer.StartTimer("Pathfinding");
        pathVectorList = PathfindingDots.Instance.FindPath(
            Vector3Int.FloorToInt(startWorldPositon), 
            Vector3Int.FloorToInt(targetWorldPosition));

        Utils.Timer.StopTimer("Pathfinding");

        Utils.Timer.PrintTimer("Pathfinding");

        //Debugging
        //Go through every node in our path and set the color to green for debugging.
        for (int i = 1; i < pathVectorList.Count; i++)
        {
            TileMapManager.Instance.ChangeTileColor(pathVectorList[i], Color.green);
        }

        //If we found a path then remove the first node since it is our current position.
        if (pathVectorList != null && pathVectorList.Count > 1)
        {
            pathVectorList.RemoveAt(0);
            pathCompleted = false;
            return true;
        }

        return false;
    }

    //public bool SetPath(List<Vector3> path)
    //{
    //    pathVectorList = path;
    //    if (pathVectorList != null && pathVectorList.Count > 1)
    //    {
    //        pathVectorList.RemoveAt(0);
    //        pathCompleted = false;
    //        return true;
    //    }
    //    return false;
    //}

    public void Move()
    {
        if (pathVectorList == null)
            return;
        // If we have gone through the pathfinding then the path is complete
        if(currentPathIndex >= pathVectorList.Count)
        {
            pathVectorList = null;
            Direction = Vector2.zero;
            pathCompleted = true;
            return;
        }

        Vector3 targetPosition = pathVectorList[currentPathIndex];
        Vector3Int adjustedTransform = Vector3Int.FloorToInt(transform.position + Vector3.zero);
        if (PathRequiresMining(targetPosition))
        {
            Debug.Log("Mining");
        }
        else if (Vector3.Distance(adjustedTransform, targetPosition) > 1.0f) // if the distance is to far away. Move closer
        {
            //Debug.Log(targetPosition);
            //if(PathfindingDots.Instance.GetNode(targetPosition).hasBlock)
            //{
            //    PathRequiresMining(targetPosition);
            //}
            //else
            //{
            Debug.Log("moving");
            Vector3 moveDir = (targetPosition - adjustedTransform).normalized;
            if (moveDir.y > 0.4f) // if the direction is upwards then jump.
                GetComponent<JumpController>().Jump();

            moveDir.y = 0;
            Direction = moveDir;
            //}
            
            //float distanceBefore = Vector3.Distance(transform.position, targetPosition);
        }
        else
        {
            currentPathIndex++;
        }
    }
    /// <summary>
    /// Check tiles on our next location if it needs to be mined and then check nearby tiles so it's walkable for the unit
    /// </summary>
    private bool PathRequiresMining(Vector3 targetPosition)
    {
        float unitHeight = 4f;
        PNode currentNode = PathfindingDots.Instance.GetNode(targetPosition);
        if (currentNode.hasBlock)
        {
            Debug.Log("Mining 1 " + targetPosition);
            miningController.Mine(targetPosition, enemyController.MiningStrength);

            return true;
        }
        else
        {
            Vector3Int adjustedTransform = Vector3Int.FloorToInt(transform.position + Vector3.down);

            if (adjustedTransform.y + 1 == targetPosition.y)//Check if we are moving upwards diagonally
            {
                if (targetPosition.x == adjustedTransform.x + 1 || targetPosition.x == adjustedTransform.x - 1)
                {
                    for (int i = 0; i < unitHeight; i++) // Then remove blocks 4 tiles high so we can move
                    {
                        Vector3 pos = targetPosition + Vector3.up * (i + 1);
                        //Check if we are inside the grid
                        if (!PathfindingDots.Instance.IsInsideGrid(pos))
                            continue;

                        PNode upNeighbour = PathfindingDots.Instance.GetNode(pos);

                        if (upNeighbour.hasBlock)
                        {
                            Debug.Log("Mining 2 " + pos);
                            miningController.Mine(pos, enemyController.MiningStrength);
                            return true;
                        }
                    }
                }
            }
            else if (targetPosition.y == adjustedTransform.y - 1) //Check if we are moving downwards
            {
                if (targetPosition.x == adjustedTransform.x + 1 || targetPosition.x == adjustedTransform.x - 1) // if we are moving diagonally downwards
                {
                    for (int i = 0; i < unitHeight; i++) // Then remove blocks 4 tiles high so we can move
                    {
                        Vector3 pos = targetPosition + Vector3.up * (i + 1);

                        //Check if we are inside the grid
                        if (!PathfindingDots.Instance.IsInsideGrid(pos))
                            continue;

                        PNode upNeighbour = PathfindingDots.Instance.GetNode(pos);

                        if (upNeighbour.hasBlock)
                        {
                            Debug.Log("Mining 3 " + pos);
                            miningController.Mine(pos, enemyController.MiningStrength);
                            return true;
                        }
                    }
                }
                else // if we are moving straight down
                {
                    Vector3 pos = targetPosition + Vector3.right;

                    //Check if we are inside the grid
                    if (PathfindingDots.Instance.IsInsideGrid(pos))
                    {
                        //Then remove 1 block to the left and right of our target 
                        PNode rightNeighbour = PathfindingDots.Instance.GetNode(pos);
                        if (rightNeighbour.hasBlock)
                        {
                            Debug.Log("Mining 4 " + pos);
                            miningController.Mine(pos, enemyController.MiningStrength);
                            return true;
                        }
                    }

                    pos = targetPosition + Vector3.left;

                    //Check if we are inside the grid
                    if (PathfindingDots.Instance.IsInsideGrid(pos))
                    {
                        PNode leftNeighbour = PathfindingDots.Instance.GetNode(pos);
                        if (leftNeighbour.hasBlock)
                        {
                            Debug.Log("Mining 5 " + pos);
                            miningController.Mine(pos, enemyController.MiningStrength);
                            return true;
                        }
                    }
                }
            }
            else if (targetPosition.y == adjustedTransform.y)//if we are not moving up or down we are moving straight sideways
            {
                if (targetPosition.x == adjustedTransform.x + 1 || targetPosition.x == adjustedTransform.x - 1)
                {
                    for (int i = 0; i < unitHeight; i++) //Then remove block 2 tiles up from target
                    {
                        Vector3 pos = targetPosition + Vector3.up * (i + 1);
                        //Check if we are inside the grid
                        if (PathfindingDots.Instance.IsInsideGrid(pos))
                        {
                            PNode upNeighbour = PathfindingDots.Instance.GetNode(pos);
                            if (upNeighbour.hasBlock)
                            {
                                Debug.Log("Mining 6 " + pos);
                                miningController.Mine(pos, enemyController.MiningStrength);
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
}