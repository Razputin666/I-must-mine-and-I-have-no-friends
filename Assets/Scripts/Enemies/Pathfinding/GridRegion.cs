using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridRegion
{
    private int x;
    private int y;

    private int width;
    private int height;

    private GridGen<GridNode> subGrid;

    public GridRegion(int height, int width, Vector3 startPosition)
    {
        this.x = Mathf.FloorToInt(startPosition.x);
        this.y = Mathf.FloorToInt(startPosition.y);

        this.width = width;
        this.height = height;

        subGrid = new GridGen<GridNode>(width, height, 1f, startPosition, (GridGen<GridNode> grid, int x, int y) => new GridNode(grid, x, y));
    }

    public GridGen<GridNode> Grid
    {
        get { return subGrid; }
    }
}