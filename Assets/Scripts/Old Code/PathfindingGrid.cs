//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PathfindingGrid
//{
//    public static PathfindingGrid Instance { private set; get; }

//    public GridGen<GridNode> pathfindingGrid;

//    public PathfindingGrid(int width, int height, Vector3 startPosition)
//    {
//        if (Instance != this && Instance != null)
//            Debug.Log("Wrong instance");

//        Instance = this;
//        pathfindingGrid = new GridGen<GridNode>(width, height, 1f, startPosition, (GridGen<GridNode> grid, int x, int y) => new GridNode(grid, x, y));

//    }
//}
