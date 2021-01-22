using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private const int MINING_COST_PER_STR = 15;

    private GridGen<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    public static Pathfinding Instance { get; private set; }

    public Pathfinding(int width, int height, Vector3 startPosition)
    {
        if (Instance != this && Instance != null)
            Debug.Log("Wrong instance");

        Instance = this;
        grid = new GridGen<PathNode>(width, height, 1f, startPosition, (GridGen<PathNode> grid, int x, int y) => new PathNode(grid, x, y));
        Debug.Log("pathfinding start Position" + startPosition);
        CalculateNeighbours();
    }

    private void CalculateNeighbours()
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode currentNode = grid.GetGridObject(x, y);
                //Debug.Log("finding Neighbor: " + currentNode.ToString());
                currentNode.neighbourNodes = GetNeighbourList(currentNode);
            }
        }
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        if ((endX < 0 || endX > grid.GetWidth() || endY < 0 || endY > grid.GetHeight()) ||
            (startX < 0 || startX > grid.GetWidth() || startY < 0 || startY > grid.GetHeight()))
        {
            return null;
        }

        List<PathNode> path = FindPath(startX, startY, endX, endY);

        if (path == null)
            return null;
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();

            foreach (PathNode pathNode in path)
            {
                Vector3 worldPositon = grid.GetWorldPosition(pathNode.x, pathNode.y);

                vectorPath.Add(worldPositon);
            }
            return vectorPath;
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        if (endNode == null)
        {
            Debug.LogError("EndNode is null at: " + endX + "," + endY);
            return null;
        }
            
        openList = new List<PathNode>() { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);

                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            if(currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            //bool movingUp = false;
            
            foreach (PathNode neighbourNode in currentNode.neighbourNodes)
            {
                if (closedList.Contains(neighbourNode))
                    continue;

                if(!neighbourNode.isWalkable)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                if (neighbourNode.y - 1 >= 0)
                {
                    //Check if the node below can be walked on
                    PathNode node = GetNode(neighbourNode.x, neighbourNode.y - 1);
                    if (!node.hasBlock)
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }
                }

                int nodesNeededToMine = 0;

                //Check if we have a faster path from current node to the neighbour node than we had previously
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);

                if (currentNode.y == neighbourNode.y)
                {
                    //Check if the node to the left or right is the neighbour node
                    if (currentNode.x + 1 == neighbourNode.x || currentNode.x - 1 == neighbourNode.x)
                    {
                        //Check if nodes above and below needs mining
                        if (neighbourNode.y + 1 < grid.GetHeight() && GetNode(neighbourNode.x, neighbourNode.y + 1).hasBlock)
                            nodesNeededToMine++;

                        if (neighbourNode.y + 2 >= 0 && GetNode(neighbourNode.x, neighbourNode.y - 1).hasBlock)
                            nodesNeededToMine++;
                    }
                }
                else if (currentNode.y + 1 == neighbourNode.y)//Check if we are going up
                {
                    if (currentNode.x + 1 == neighbourNode.x || currentNode.x - 1 == neighbourNode.x)
                    {
                        for (int i = 0; i < 3; i++) //Check nodes 3 tiles up if they need to be mined in order to be able to move
                        {
                            if (neighbourNode.y + i < grid.GetHeight() && GetNode(neighbourNode.x, neighbourNode.y + i).hasBlock)
                                nodesNeededToMine++;
                        }
                    }
                }
                else if (currentNode.y - 1 == neighbourNode.y) // Check if the neighbourNode is below us
                {
                    if (currentNode.x + 1 == neighbourNode.x || currentNode.x - 1 == neighbourNode.x)
                    {
                        for (int i = 0; i < 3; i++) //Check nodes 3 tiles up if the need to be mined in order to be able to move
                        {
                            if (neighbourNode.y + i < grid.GetHeight() && GetNode(neighbourNode.x, neighbourNode.y + i).hasBlock)
                                nodesNeededToMine++;
                        }
                    }
                    else
                    {
                        //Check if the node to the left and right below us needs mining
                        if (neighbourNode.x - 1 >= 0 && GetNode(neighbourNode.x - 1, neighbourNode.y).hasBlock)
                            nodesNeededToMine++;

                        if (neighbourNode.x + 1 >= 0 && GetNode(neighbourNode.x + 1, neighbourNode.y).hasBlock)
                            nodesNeededToMine++;
                    }
                    
                }

                tentativeGCost += nodesNeededToMine * MINING_COST_PER_STR;

                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                        openList.Add(neighbourNode);
                }
                    
            }
        }
        //Could not find a path (Out of nodes on the openList)
        return null;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();
        //Up
        //if (currentNode.y + 1 < grid.GetHeight())
        //    neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

        if (currentNode.x - 1 >= 0)
        {
            //Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            //Left Down
            if (currentNode.y - 1 >= 0)
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            //Left Up
            if (currentNode.y + 1 < grid.GetHeight())
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        

        if (currentNode.x + 1  < grid.GetWidth())
        {
            //Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            //Right Down
            if (currentNode.y - 1 >= 0)
               neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            //Right Up
            if (currentNode.y + 1 < grid.GetHeight())
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }

        //Down
        if (currentNode.y - 1 >= 0)
            neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));

        return neighbourList;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();

        path.Add(endNode);

        PathNode currentNode = endNode;
        //while currentNode has a parent (while it's not the startNode)
        while(currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);

            currentNode = currentNode.cameFromNode;
        }

        path.Reverse();

        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];

        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }

        return lowestFCostNode;
    }

    public GridGen<PathNode> GetGrid()
    {
        return grid;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return grid.GetWorldPosition(x, y);
    }

    public PathNode GetNode(Vector3 worldPosition)
    {
        return grid.GetGridObject(worldPosition);
    }

    public PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    public void UpdateGridWalkable(Vector3 worldPosition, bool isWalkable)
    {
        PathNode pathNode = grid.GetGridObject(worldPosition);

        pathNode.isWalkable = isWalkable;
    }

    public void UpdateGridMineable(Vector3 worldPosition, bool isMineable)
    {
        PathNode pathNode = grid.GetGridObject(worldPosition);
        
        pathNode.hasBlock = isMineable;
    }
}