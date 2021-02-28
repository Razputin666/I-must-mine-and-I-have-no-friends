//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Row : MonoBehaviour
//{
//    public float startSlope;
//    public float endSlope;
//    public int depth;

//    public Row(int depth, float startSlope, float endSlope)
//    {
//        this.depth = depth;
//        this.startSlope = startSlope;
//        this.endSlope = endSlope;
//    }
//    public Row Next()
//    {
//        Row nextRow = new Row(depth + 1, startSlope, endSlope); ;
//        return nextRow;
//    }
//    public Vector2Int[] Tiles()
//    {
//        int minColumn = RoundTiesUp(depth * startSlope);
//        int maxColumn = RoundTiesDown(depth * endSlope);

//        Vector2Int[] tiles = new Vector2Int[maxColumn + 1 - minColumn];

//        for (int i = 0; i < tiles.Length; i++)
//        {
//            tiles[i] = new Vector2Int(depth, i) + Vector2Int.FloorToInt(transform.position);
//        }
//        return tiles;
//    }
//    private int RoundTiesUp(float number)
//    {
//        return Mathf.FloorToInt(number + 0.5f);
//    }
//    private int RoundTiesDown(float number)
//    {
//        return Mathf.CeilToInt(number - 0.5f);
//    }

//}


