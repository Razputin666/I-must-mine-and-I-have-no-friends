using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

public class PathfindingDots : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private const int MINING_COST_PER_STR = 15;

    private void Start()
    {
        FindPath(new int2(0, 0), new int2(3, 1));
    }

    public void FindPath(int2 startPosition, int2 endPosition)
    {
        int2 gridSize = new int2(4, 4);

        NativeArray<PNode> pathNodeArray = new NativeArray<PNode>(gridSize.x * gridSize.y, Allocator.Temp);

        for (int x = 0; x < gridSize.y; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                PNode pathNode = new PNode();
                pathNode.x = x;
                pathNode.y = y;

                pathNode.index = CalculateIndex(x, y, gridSize.x);

                pathNode.gCost = int.MaxValue;
                pathNode.hCost = CalculateDistanceCost(new int2(x, y), endPosition);

                pathNode.isWalkable = true;
                pathNode.cameFromNodeIndex = -1;
                pathNode.hasBlock = true;

                pathNodeArray[pathNode.index] = pathNode;
            }
        }

        PNode walkablePathNode = pathNodeArray[CalculateIndex(1, 0, gridSize.x)];
        walkablePathNode.SetIsWalkable(false);
        pathNodeArray[CalculateIndex(1, 0, gridSize.x)] = walkablePathNode;

        walkablePathNode = pathNodeArray[CalculateIndex(1, 1, gridSize.x)];
        walkablePathNode.SetIsWalkable(false);
        pathNodeArray[CalculateIndex(1, 1, gridSize.x)] = walkablePathNode;

        walkablePathNode = pathNodeArray[CalculateIndex(1, 2, gridSize.x)];
        walkablePathNode.SetIsWalkable(false);
        pathNodeArray[CalculateIndex(1, 2, gridSize.x)] = walkablePathNode;

        NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(new int2[]
        {
            new int2(-1, 0), // Left
            new int2(+1, 0), // Right
            new int2(0, -1), // Down
            new int2(-1, -1), // Down Left
            new int2(-1, +1), // Up left
            new int2(+1, -1), // Down Right
            new int2(+1, +1), // Up Right
        }, Allocator.Temp);
    
        int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

        PNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
        startNode.gCost = 0;
        startNode.CalculateFCost();

        pathNodeArray[startNode.index] = startNode;

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        openList.Add(startNode.index);

        while (openList.Length > 0)
        {
            int currentNodeIndex = GetLowestFCostNodeIndex(openList, pathNodeArray);

            PNode currentNode = pathNodeArray[currentNodeIndex];

            if (currentNodeIndex == endNodeIndex)
            {
                //Reached our destination
                break;
            }

            //Remove current Node from Open List
            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }

            closedList.Add(currentNodeIndex);

            for (int i = 0; i < neighbourOffsetArray.Length; i++)
            {
                int2 neighbourOffset = neighbourOffsetArray[i];
                int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                {
                    continue; // Continue to the next Neighbour
                }

                int neighboutNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                if (closedList.Contains(neighboutNodeIndex))
                    continue; // Already Searched this Node

                PNode neighbourNode = pathNodeArray[neighboutNodeIndex];
                
                if (!neighbourNode.isWalkable)
                    continue; // Not walkable

                ////Check if the node below can be walked on
                //if (IsPositionInsideGrid(new int2(neighbourPosition.x, neighbourPosition.y - 1), gridSize.x))
                //{
                //    int groundIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y - 1, gridSize.x);
                //    PNode node = pathNodeArray[groundIndex];
                //    if (!node.hasBlock)
                //    {
                //        continue;
                //    }
                //}

                int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);

                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNodeIndex = currentNodeIndex;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.CalculateFCost();
                    pathNodeArray[neighboutNodeIndex] = neighbourNode;

                    if (!openList.Contains(neighbourNode.index))
                        openList.Add(neighbourNode.index);
                }

            }
        }

        PNode endNode = pathNodeArray[endNodeIndex];
        
        if (endNode.cameFromNodeIndex == -1)
        {
            //Didn't find a path

            Debug.Log("Didn't find a path");
        }
        else
        {
            //Found Path
            NativeList<int2> path = CalculatePath(pathNodeArray, endNode);
            foreach (int2 pathPosition in path)
            {
                Debug.Log(pathPosition);
            }
            path.Dispose();
        }

        openList.Dispose();
        closedList.Dispose();
        pathNodeArray.Dispose();
        neighbourOffsetArray.Dispose();
    }

    private NativeList<int2> CalculatePath(NativeArray<PNode> pathNodeArray, PNode endNode)
    {
        if (endNode.cameFromNodeIndex == -1)
        {
            return new NativeList<int2>(Allocator.Temp);
        }
        else
        {
            //Found Path
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            path.Add(new int2(endNode.x, endNode.y));

            PNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1)
            {
                PNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                path.Add(new int2(cameFromNode.x, cameFromNode.y));
                currentNode = cameFromNode;
            }
            return path;
        }
    }

    private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
    {
        return
            gridPosition.x >= 0 &&
            gridPosition.y >= 0 &&
            gridPosition.x < gridSize.x &&
            gridPosition.y < gridSize.x;
    }

    private int CalculateIndex(int x, int y, int gridWidth)
    {
        return x + y * gridWidth;
    }

    private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
    {
        int xDistance = Mathf.Abs(aPosition.x - bPosition.x);
        int yDistance = Mathf.Abs(aPosition.y - bPosition.y);
        int remaining = Mathf.Abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private int GetLowestFCostNodeIndex(NativeList<int> openList, NativeArray<PNode> pathNodeArray)
    {
        PNode lowestFCostNode = pathNodeArray[openList[0]];
        for (int i = 1; i < openList.Length; i++)
        {
            PNode testPathNode = pathNodeArray[openList[i]];
            if (testPathNode.fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = testPathNode;
            }
        }

        return lowestFCostNode.index;
    }
}
