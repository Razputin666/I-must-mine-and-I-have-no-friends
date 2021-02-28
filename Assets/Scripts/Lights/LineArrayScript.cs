//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using Mirror;
//using Unity.Collections;

//public class LineArrayScript : NetworkBehaviour
//{
//   [SerializeField] private Worldgeneration wGen;
//   [SerializeField] private LayerMask layermask;
//    private const int range = 30;

//    private int[,] origin;
//    private int[,] isBlocking;
//    private List<Vector2Int> visibleTiles;
//    private Quadrant[] quadrants;

//    // Start is called before the first frame update
//    public override void OnStartServer()
//    {
//        wGen = GameObject.Find("WorldGen").GetComponent<Worldgeneration>();
//        visibleTiles = new List<Vector2Int>();
//        quadrants = new Quadrant[4];
//        Fraction frac = new Fraction(-1, 0);
//        Debug.Log(frac + " fraction");
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//       // Debug.Log(GetPositionInArray(transform.position
//    }

//    //private void funfun()
//    //{
//    //    Collider2D[] hitArray = Physics2D.OverlapCircleAll(transform.position, range, layermask);
//    //    foreach (Collider2D hits in hitArray)
//    //    {
            
//    //    }
//    //}
//    private void Compute_Fov(int[,] is_blocking, int[,] mark_visible)
//    {
//        Vector2Int origin = Vector2Int.FloorToInt(transform.position);
//        //Quadrant[] quadrants = new Quadrant[4];
//        for (int i = 0; i < 4; i++)
//        {
//            Quadrant quadrant = new Quadrant(i, origin);

//            Fraction fracOne = new Fraction(-1, 0);
//            Fraction fracTwo = new Fraction(1, 0);

//            Row firstRow = new Row(1, fracOne, fracTwo);
//            scan(firstRow);

//        }
//    }

//    private void scan(Row row)
//    {
//        Vector2 prevTile = Vector2.zero;
//        //foreach (Vector2Int tile in row.tiles)
//        //{
//        //   // if (isWall(tile) || )
//        //}
//    }

//    private bool isWall(Vector2Int tile)
//    {
//        if (TileMapManager.Instance.worldArray[tile.x, tile.y] == (int)BlockTypeConversion.Empty)
//        {
//            return false;
//        }
//        Vector2Int quadPos = quadrants[0].Transform(tile);

//        return (TileMapManager.Instance.worldArray[quadPos.x, quadPos.y] > (int)BlockTypeConversion.Empty);
//    }

//    private void reveal(Vector2Int tile)
//    {
//        Vector2Int quadPos = quadrants[0].Transform(tile);
//        visibleTiles.Add(quadPos);
//    }

//    private bool isFloor(Vector2Int tile)
//    {
//        if (TileMapManager.Instance.worldArray[tile.x, tile.y] == (int)BlockTypeConversion.Empty)
//        {
//            return false;
//        }
//        Vector2Int quadPos = quadrants[0].Transform(tile);

//        return !(TileMapManager.Instance.worldArray[quadPos.x, quadPos.y] > (int)BlockTypeConversion.Empty);
//    }
//    private bool isSymmetric(Vector2Int[] row, Vector2Int tile)
//    {
//        return false;
//    }
//    public Fraction Slopes(Vector2Int tile, Row row)
//    {
//        row.depth = tile.x;
//        row.column = tile.y;
//        Fraction frac;
//        return frac = new Fraction(1, 2);
//    }

//    private int GetPositionInArray(Vector3 position)
//    {
//        int positionInArray = Mathf.FloorToInt(position.x) * wGen.GetWorldHeight + Mathf.FloorToInt(position.y);

//        return positionInArray;
//    }
//    private Vector3 GetPositionFromArray(int positionInArray)
//    {
//        Vector3 position = new Vector3(Mathf.FloorToInt(positionInArray / wGen.GetWorldHeight), (positionInArray / wGen.GetWorldHeight) - Mathf.FloorToInt(positionInArray / wGen.GetWorldHeight) * wGen.GetWorldHeight);
//        return position;
//    }
//}




////public class Quadrant
////{
////    public int cardinal;
////    public Vector2Int position;
////    public enum Directions
////    {
////        north, east, south, west
////    }

////    public Quadrant(int cardinal, Vector2Int origin)
////    {
////        this.cardinal = cardinal;
////        position = origin;
////    }

////    public Vector2Int Transform(Vector2Int tile)
////    {
////        Vector2Int rowsAndColumns = tile;
////        if (this.cardinal == (int)Directions.north)
////        {
////            return new Vector2Int(position.x + rowsAndColumns.y, position.y - rowsAndColumns.x); 
////        }
////        if (this.cardinal == (int)Directions.south)
////        {
////            return new Vector2Int(position.x + rowsAndColumns.y, position.y + rowsAndColumns.x);
////        }
////        if (this.cardinal == (int)Directions.east)
////        {
////            return new Vector2Int(position.x + rowsAndColumns.x, position.y + rowsAndColumns.y);
////        }
////        if (this.cardinal == (int)Directions.west)
////        {
////            return new Vector2Int(position.x - rowsAndColumns.x, position.y - rowsAndColumns.y);
////        }
////        else
////        {
////            return tile;
////        }
////    }

////}

////public class Row
////{
////    public Vector2Int rowDepthCollections;
////    public int depth;
////    public Fraction startSlope;
////    public Fraction endSlope;
////    public int column;

////    public Row(int depth, Fraction startSlope, Fraction endSlope)
////    {
////        this.depth = depth;
////        this.startSlope = startSlope;
////        this.endSlope = endSlope;
////    }
////    //public IEnumerator tiles()
////    //{
        
////    //    yield return new Vector2Int(depth, )
////    //}

////}

//public class Fraction
//{
//    public int numerator;
//    public int denominator;

//    public Fraction(int numerator, int denominator)
//    {
//        this.numerator = numerator;
//        this.denominator = denominator;
//    }

//    public Fraction(Fraction fraction)
//    {
//        numerator = fraction.numerator;
//        denominator = fraction.denominator;
//    }

//    public override bool Equals(object obj)
//    {
//        Fraction other = obj as Fraction;
//        return (numerator == other.numerator && denominator == other.denominator);
//    }

//    public static bool operator ==(Fraction f1, Fraction f2)
//    {
//        return f1.Equals(f2);
//    }

//    public static bool operator !=(Fraction f1, Fraction f2)
//    {
//        return !(f1 == f2);
//    }

//    public override int GetHashCode()
//    {
//        return numerator * denominator;
//    }

//    public override string ToString()
//    {
//        return numerator + "/" + denominator;
//    }

//    //Helper function, simplifies a fraction.
//    public Fraction Simplify()
//    {
//        for (int divideBy = denominator; divideBy > 0; divideBy--)
//        {
//            bool divisible = true;

//            if ((int)(numerator / divideBy) * divideBy != numerator)
//            {
//                divisible = false;
//            }
//            else if ((int)(denominator / divideBy) * divideBy != denominator)
//            {
//                divisible = false;
//            }
//            else if (divisible)
//            {
//                numerator /= divideBy;
//                denominator /= divideBy;
//            }
//        }
//        return this;
//    }
//}