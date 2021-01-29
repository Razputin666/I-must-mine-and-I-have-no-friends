using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    private GridGen<GridNode> grid;
    private int x;
    private int y;

    private bool isWalkable;
    private bool hasBlock;

    public GridNode(GridGen<GridNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
        hasBlock = true;
    }

    public bool IsWalkable
    {
        get { return isWalkable; }
        set 
        { 
            isWalkable = value;
            grid.TriggerGridObjectChanged(x, y);
        }
    }

    public bool HasBlock
    {
        get { return hasBlock; }
        set 
        { 
            hasBlock = value;
            grid.TriggerGridObjectChanged(x, y);
        }
    }
}
