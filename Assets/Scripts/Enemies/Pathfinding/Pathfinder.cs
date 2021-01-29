using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Pathfinder : NetworkBehaviour
{
    private List<Vector3> pathVectorList;

    private int currentPathIndex;
    public Vector2 Direction { get; private set; }

    public override void OnStartServer()
    {
        currentPathIndex = 0;
    }

    public void CalculatePath(Vector3 startWorldPositon, Vector3 targetWorldPosition)
    {
        currentPathIndex = 0;
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        stopwatch.Start();

        pathVectorList = Pathfinding.Instance.FindPath(
            startWorldPositon,
            targetWorldPosition);

        stopwatch.Stop();

        System.TimeSpan timeTaken = stopwatch.Elapsed;
        Debug.Log("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));

        stopwatch.Reset();

        stopwatch.Start();
        List<Vector3> path = PathfindingDots.Instance.FindPath(
            Vector3Int.FloorToInt(startWorldPositon), 
            Vector3Int.FloorToInt(targetWorldPosition));

        stopwatch.Stop();

        timeTaken = stopwatch.Elapsed;
        Debug.Log("Time taken dots: " + timeTaken.ToString(@"m\:ss\.fff"));

        //for (int i = 0; i < path.Count; i++)
        //{
        //    Debug.Log(i + " path1: " + path[i] + " path2: " + pathVectorList[i]);
        //}

        if (pathVectorList != null && pathVectorList.Count > 1)
            pathVectorList.RemoveAt(0);
    }

    public void Move()
    {
        if (pathVectorList == null)
            return;

        Vector3 targetPosition = pathVectorList[currentPathIndex];
        Vector3Int adjustedTransform = Vector3Int.FloorToInt(transform.position + Vector3.down);
        if (PathRequiresMining(targetPosition))
        {

        }
        else if (Vector3.Distance(adjustedTransform, targetPosition) > 1.0f)
        {
            Debug.Log(targetPosition + " " + adjustedTransform);
            Vector3 moveDir = (targetPosition - adjustedTransform).normalized;

            if (moveDir.y > 0.5f)
                GetComponent<PlayerMovementController>().IsJumping = true;
            else
                GetComponent<PlayerMovementController>().IsJumping = false;

            moveDir.y = 0;
            Direction = moveDir;
            float distanceBefore = Vector3.Distance(transform.position, targetPosition);
            //rb2d.velocity += new Vector2(moveDir.x, moveDir.y) * movementSpeed;
            //transform.position += moveDir * 5f * Time.deltaTime;
        }
        else
        {
            currentPathIndex++;
            if (currentPathIndex >= pathVectorList.Count)
            {
                GetComponent<PlayerMovementController>().IsJumping = false;
                pathVectorList = null;
                Direction = Vector2.zero;
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

            if (adjustedTransform.y + 1 == targetPosition.y)//Check if we are moving upwards diagonally
            {
                if (targetPosition.x == adjustedTransform.x + 1 || targetPosition.x == adjustedTransform.x - 1)
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
            else if (targetPosition.y == adjustedTransform.y - 1) //Check if we are moving downwards
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
}
