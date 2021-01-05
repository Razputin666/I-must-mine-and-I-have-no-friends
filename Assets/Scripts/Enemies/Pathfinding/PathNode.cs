using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private GridGen<PathNode> grid;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;
    
    public PathNode cameFromNode;
    public List<PathNode> neighbourNodes;

    public bool isWalkable;
    public bool isMineable;

    public PathNode(GridGen<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.isWalkable = true;
        this.isMineable = false;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public override string ToString()
    {
        return x + "," + y;
    }
}