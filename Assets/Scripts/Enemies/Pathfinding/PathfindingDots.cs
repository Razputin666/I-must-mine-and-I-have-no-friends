using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public class PathfindingDots
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private const int MINING_COST_PER_STR = 15;
    public static PathfindingDots Instance { private set; get; }

    public GridGen<GridNode> pathfindingGrid;
    private GridRegion[] pathfindingRegions;

    private int gridWidth;
    private int gridHeight;
    public PathfindingDots(int width, int height, Vector3 startPosition)
    {
        if (Instance != this && Instance != null)
            Debug.Log("Wrong instance");

        Instance = this;
        //pathfindingGrid = new GridGen<GridNode>(width, height, 1f, startPosition, (GridGen<GridNode> grid, int x, int y) => new GridNode(grid, x, y));

        gridWidth = width;
        gridHeight = height;
        CreateRegions(width, height, startPosition);
    }

    private void CreateRegions(int width, int height, Vector3 startPos)
    {
        Vector3 regionStart;
        int regionWidth = 50;
        int regionHeight = 50;
        int regionsX = width / regionWidth;
        int regionsY = height / regionHeight;

        if (width % regionWidth > 0)
            regionsX++;
        if (height % regionHeight > 0)
            regionsY++;

        Debug.Log(regionsX + "," + regionsY);
        pathfindingRegions = new GridRegion[regionsX * regionsY];
        for (int x = 0; x < regionsX; x++)
        {
            for (int y = 0; y < regionsY; y++)
            {
                int index = x * regionsY + y;
                regionStart = new Vector3(x * regionWidth, y * regionHeight);
                int currentWidth;
                int currentHeight;

                if ((x + 1) * regionWidth <= width)
                    currentWidth = 50;
                else
                    currentWidth = width - x * regionWidth;//Last region has a shorter width

                if ((y + 1) * regionHeight <= height)
                    currentHeight = 50;
                else
                    currentHeight = height - y * regionHeight;//Last region has a shorter height

                //Debug.Log(index);
                //Debug.Log(regionStart + ", " + currentWidth + ", " + currentHeight);
                GridRegion region = new GridRegion(currentWidth, currentHeight, startPos + regionStart);
                pathfindingRegions[index] = region;
            }
        }
    }

    public void UpdateGridWalkable(Vector3 worldPosition, bool isWalkable)
    {
        GridNode pathNode = pathfindingGrid.GetGridObject(worldPosition);

        pathNode.IsWalkable = isWalkable;
    }

    public void UpdateGridMineable(Vector3 worldPosition, bool hasBlock)
    {
        ////GridNode pathNode = pathfindingGrid.GetGridObject(worldPosition);

        //pathfindingGrid.GetXY(worldPosition, out int x, out int y);
        //if(y + 1 < pathfindingGrid.GetHeight()) // Check the node above is inside grid
        //{
        //    UpdateGridWalkable(worldPosition + Vector3.up, hasBlock);
        //}

        int gridX = (int)worldPosition.x / gridWidth;
        int gridY = (int)worldPosition.y / gridHeight;
        
        int index = gridX * gridHeight + gridY;

        GridRegion gridRegion = pathfindingRegions[index];
        GridNode pathNode = gridRegion.Grid.GetGridObject(worldPosition);

        pathNode.HasBlock = hasBlock;
    }

    public List<Vector3> FindPath(Vector3Int startWorldPosition, Vector3Int endWorldPosition)
    {
        int gridWidth = pathfindingGrid.GetWidth();
        int gridHeight = pathfindingGrid.GetHeight();
        
        int2 gridSize = new int2(gridWidth, gridHeight);

        pathfindingGrid.GetXY(startWorldPosition, out int startX, out int startY);
        pathfindingGrid.GetXY(endWorldPosition, out int endX, out int endY);

        int2 startPosition = new int2(startX, startY);
        int2 endPosition = new int2(endX, endY);
        NativeArray<PNode> pathNodeArray = GetPathNodeArray();
        
        //NativeArray<PNode> tmpPathNodeArray = new NativeArray<PNode>(pathNodeArray, Allocator.TempJob);
        FindPathJob findPathJob = new FindPathJob
        {
            gridSize = gridSize,
            pathNodeArray = pathNodeArray,
            startPosition = startPosition,
            endPosition = endPosition
        };
        
        JobHandle jobHandle = findPathJob.Schedule();

        jobHandle.Complete();
        int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridWidth);
        PNode endNode = pathNodeArray[endNodeIndex];
        
        NativeArray<int2> pathNative = CalculatePath(pathNodeArray, endNode);
        
        GridGen<GridNode> grid = pathfindingGrid;
        List<Vector3> path = new List<Vector3>();
        for (int i = 0; i < pathNative.Length; i++)
        {
            Vector3 worldPosition = grid.GetWorldPosition(pathNative[i].x, pathNative[i].y);
            path.Add(new Vector3(worldPosition.x, worldPosition.y));
        }

        path.Reverse();

        pathNodeArray.Dispose();

        return path;
    }

    private NativeArray<PNode> GetPathNodeArray()
    {
        GridGen<GridNode> grid = pathfindingGrid;
        
        int2 gridSize = new int2(grid.GetWidth(), grid.GetHeight());
        NativeArray<PNode> pathNodeArray = new NativeArray<PNode>(gridSize.x * gridSize.y, Allocator.TempJob);

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PNode pathNode = new PNode();
                pathNode.x = x;
                pathNode.y = y;
                pathNode.index = CalculateIndex(x, y, gridSize.x);

                pathNode.gCost = int.MaxValue;

                pathNode.hasBlock = grid.GetGridObject(x, y).HasBlock;
                pathNode.isWalkable = grid.GetGridObject(x, y).IsWalkable;
                pathNode.cameFromNodeIndex = -1;

                pathNodeArray[pathNode.index] = pathNode;
            }
        }

        return pathNodeArray;
    }

    private struct FindPathJob : IJob
    {
        public int2 gridSize;
        public NativeArray<PNode> pathNodeArray;
        
        public int2 startPosition;
        public int2 endPosition;
        public void Execute()
        {
            for(int i = 0; i < pathNodeArray.Length; i++)
            {
                PNode pathNode = pathNodeArray[i];
                pathNode.hCost = CalculateDistanceCost(new int2(pathNode.x, pathNode.y), endPosition);
                pathNode.cameFromNodeIndex = -1;

                pathNodeArray[i] = pathNode;
            }
            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); // Left
            neighbourOffsetArray[1] = new int2(+1, 0); // Right
            neighbourOffsetArray[2] = new int2(0, -1); // Down
            neighbourOffsetArray[3] = new int2(-1, -1); // Down Left
            neighbourOffsetArray[4] = new int2(-1, +1); // Up left
            neighbourOffsetArray[5] = new int2(+1, -1); // Down Right
            neighbourOffsetArray[6] = new int2(+1, +1); // Up Right

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
                        continue; // Continue to the next Neighbour

                    int neighboutNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                    if (closedList.Contains(neighboutNodeIndex))
                        continue; // Already Searched this Node

                    PNode neighbourNode = pathNodeArray[neighboutNodeIndex];

                    if (!neighbourNode.isWalkable)
                        continue; // Not walkable

                    //Check if the node below can be walked on
                    if (neighbourPosition.y - 1 >= 0)
                    {
                        int groundIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y - 1, gridSize.x);
                        PNode node = pathNodeArray[groundIndex];
                        if (!node.hasBlock)
                        {
                            closedList.Add(groundIndex);
                            continue;
                        }
                    }
                    int nodesNeededToMine = GetNodesNeededToMine(pathNodeArray, currentNode, neighbourNode, gridSize);

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);

                    tentativeGCost += nodesNeededToMine * MINING_COST_PER_STR;

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
            openList.Dispose();
            closedList.Dispose();
            neighbourOffsetArray.Dispose();
        }
    }

    private static int GetNodesNeededToMine(NativeArray<PNode> pathNodeArray, PNode currentNode, PNode neighbourNode, int2 gridSize)
    {
        int nodesNeededToMine = 0;

        // if we are checking a node on the same height (to the left or right of the current node)
        if (currentNode.y == neighbourNode.y)
        {
            //Check if the node to the left or right is the neighbour node
            if (currentNode.x + 1 == neighbourNode.x || currentNode.x - 1 == neighbourNode.x)
            {
                //Check if 1 node above needs mining
                if (neighbourNode.y + 1 < gridSize.y)
                {
                    int neighbourUpIndex = CalculateIndex(neighbourNode.x, neighbourNode.y + 1, gridSize.x);
                    PNode upNode = pathNodeArray[neighbourUpIndex];
                    if (upNode.hasBlock)
                        nodesNeededToMine++;

                }
                //Check if 2 node above needs mining
                if (neighbourNode.y + 2 < gridSize.y)
                {
                    int neighbourUpIndex = CalculateIndex(neighbourNode.x, neighbourNode.y + 2, gridSize.x);
                    PNode up2Node = pathNodeArray[neighbourUpIndex];
                    if (up2Node.hasBlock)
                        nodesNeededToMine++;
                }
            }
        }
        else if (currentNode.y + 1 == neighbourNode.y) //Check if we are going up
        {
            if (currentNode.x + 1 == neighbourNode.x || currentNode.x - 1 == neighbourNode.x) // Check if the neightbour is to the left or right of the current node
            {
                for (int i = 0; i < 4; i++) //Check nodes 4 tiles up if they need to be mined in order to be able to move
                {
                    if (neighbourNode.y + i < gridSize.y)
                    {
                        int neighbourUpIndex = CalculateIndex(neighbourNode.x, neighbourNode.y + i, gridSize.x);
                        PNode upNode = pathNodeArray[neighbourUpIndex];
                        if (upNode.hasBlock)
                            nodesNeededToMine++;
                    }
                }
            }
        }
        else if (currentNode.y - 1 == neighbourNode.y) // Check if the neighbourNode is below us
        {
            if (currentNode.x + 1 == neighbourNode.x || currentNode.x - 1 == neighbourNode.x)
            {
                for (int i = 0; i < 4; i++) //Check nodes 4 tiles up if the need to be mined in order to be able to move
                {
                    if (neighbourNode.y + i < gridSize.y)
                    {
                        int neighbourUpIndex = CalculateIndex(neighbourNode.x, neighbourNode.y + i, gridSize.x);
                        PNode upNode = pathNodeArray[neighbourUpIndex];
                        if(upNode.hasBlock)
                            nodesNeededToMine++;
                    }
                }
            }
            else
            {
                //Check if the node to the left and right below us needs mining
                if (neighbourNode.x - 1 >= 0)
                {
                    int neighbourLeftIndex = CalculateIndex(neighbourNode.x - 1, neighbourNode.y, gridSize.x);
                    PNode leftNode = pathNodeArray[neighbourLeftIndex];
                    if(leftNode.hasBlock)
                        nodesNeededToMine++;
                }
                if (neighbourNode.x + 1 < gridSize.x)
                {
                    int neighbourRightIndex = CalculateIndex(neighbourNode.x + 1, neighbourNode.y, gridSize.x);
                    PNode rightNode = pathNodeArray[neighbourRightIndex];
                    if (rightNode.hasBlock)
                        nodesNeededToMine++;
                }
            }
        }
        return nodesNeededToMine;
    }

    private static NativeList<int2> CalculatePath(NativeArray<PNode> pathNodeArray, PNode endNode)
    {
        return new NativeList<int2>(Allocator.Temp);
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

    private static bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
    {
        return
            gridPosition.x >= 0 &&
            gridPosition.y >= 0 &&
            gridPosition.x < gridSize.x &&
            gridPosition.y < gridSize.y;
    }

    private static int CalculateIndex(int x, int y, int gridWidth)
    {
        return x + y * gridWidth;
    }

    private static int CalculateDistanceCost(int2 aPosition, int2 bPosition)
    {
        int xDistance = Mathf.Abs(aPosition.x - bPosition.x);
        int yDistance = Mathf.Abs(aPosition.y - bPosition.y);
        int remaining = Mathf.Abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private static int GetLowestFCostNodeIndex(NativeList<int> openList, NativeArray<PNode> pathNodeArray)
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