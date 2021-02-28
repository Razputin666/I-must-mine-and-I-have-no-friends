using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using Mirror;


public enum LightDirections
{
    North, East, South, West
}
public class ShadowCasting : NetworkBehaviour
{
    private List<Vector2Int> visibleTiles;
    private List<float> temporaryLight;
    [HideInInspector] public Quadrant quadrant;
    [HideInInspector] public static event EventHandler<Vector2Int> OnlightUpdated;

    [SerializeField] private int range;

    [SerializeField] private TileBase darkTile;
    [Range(0, 5f)]
    [SerializeField] private float lightStrength;

    public override void OnStartServer()
    {
        quadrant = new Quadrant(transform);

        visibleTiles = new List<Vector2Int>();
        temporaryLight = new List<float>();
    }


    private void LateUpdate()
    {
       // VisibleTiles.Add(Vector2Int.FloorToInt(transform.position));
        //for (int i = 0; i < VisibleTiles.Count; i++)
        //{
        //    TileMapManager.Instance.shadowMap.SetColor(new Vector3Int(VisibleTiles[i].x, VisibleTiles[i].y, 0), Color.black);
        //}
       // VisibleTiles.Clear();
       // ComputeLighttest();
       // TileMapManager.Instance.shadowMap.SetColor(Vector3Int.FloorToInt(transform.position), lightSource);


    }
    private void OnEnable()
    {
        TilemapSyncer.OnTileMapUpdated += TilemapSyncer_OnTileMapUpdated;
        visibleTiles = new List<Vector2Int>();
        temporaryLight = new List<float>();
    }

    private void TilemapSyncer_OnTileMapUpdated(object sender, Vector3 updatedTile)
    {
        if (Vector3.Distance(transform.position, updatedTile) < range)
        {
            ComputeLight();
        }
    }

    private void OnDisable()
    {
        TilemapSyncer.OnTileMapUpdated -= TilemapSyncer_OnTileMapUpdated;
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            ComputeLight();
            transform.hasChanged = false;
        }
    }

    #region HelperFunctions
    public Vector2Int[] Tiles(Row row)
    {
        int minColumn = RoundTiesUp(row.depth * row.startSlope);
        int maxColumn = RoundTiesDown(row.depth * row.endSlope);

        Vector2Int[] tiles = new Vector2Int[maxColumn + 1 - minColumn];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Vector2Int(row.depth, minColumn + i);
        }
        return tiles;
    } 
    private int RoundTiesUp(float number)
    {
        return Mathf.FloorToInt(number + 0.5f);
    }
    private int RoundTiesDown(float number)
    {
        return Mathf.CeilToInt(number - 0.5f);
    }
    private bool IsSymmetric(Row row, Vector2Int tile)
    {
        return tile.y >= row.depth * row.startSlope && tile.y <= row.depth * row.endSlope;
    }
    private bool IsWall(Vector2Int tile)
    {
        if (tile == new Vector2Int(-100, -100))
        {
            return false;
        }
        Vector2Int tileToCheck =  quadrant.QuadTransform(tile);
        return TileMapManager.Instance.worldArray[Mathf.Clamp(tileToCheck.x, 0, TileMapManager.Instance.shadowArray.GetUpperBound(0)), Mathf.Clamp(tileToCheck.y, 0, TileMapManager.Instance.shadowArray.GetUpperBound(1))] >= 1;
    }
    private bool IsFloor(Vector2Int tile)
    {
        if (tile == new Vector2Int(-100, -100))
        {
            return false;
        }
        Vector2Int tileToCheck = quadrant.QuadTransform(tile);
        return !(TileMapManager.Instance.worldArray[Mathf.Clamp(tileToCheck.x, 0, TileMapManager.Instance.shadowArray.GetUpperBound(0)), Mathf.Clamp(tileToCheck.y, 0, TileMapManager.Instance.shadowArray.GetUpperBound(1))] >= 1);
    }
    private float Slope(Vector2Int tile)
    {
        return (2f * tile.y - 1f) / (2f * tile.x);
    }
    #endregion
    private void ComputeLight()
    {

        if (transform.position.x > TileMapManager.Instance.shadowArray.GetUpperBound(0) && transform.position.y > TileMapManager.Instance.shadowArray.GetUpperBound(1))
            return;

        quadrant = new Quadrant(transform);

        for (int i = 0; i < visibleTiles.Count; i++)
        {
            TileMapManager.Instance.shadowArray[Mathf.Clamp(visibleTiles[i].x, 0, TileMapManager.Instance.shadowArray.GetUpperBound(0)), Mathf.Clamp(visibleTiles[i].y, 0, TileMapManager.Instance.shadowArray.GetUpperBound(1))] -= Mathf.Clamp(lightStrength - temporaryLight[i], 0, 1f);
        }

        visibleTiles.Clear();
        temporaryLight.Clear();

        for (int i = 0; i < 4; i++)
        {
            quadrant.direction = (LightDirections)i;
            Row firstRow = new Row(1, -1, 1);

            LightScan(firstRow);
        }


        visibleTiles.Add(Vector2Int.FloorToInt(transform.position));
        TileMapManager.Instance.shadowArray[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)] += Mathf.Clamp(lightStrength, 0, 1f);
        temporaryLight.Add(0);
        OnlightUpdated?.Invoke(this, Vector2Int.FloorToInt(transform.position));
        

    }
    private void LightScan(Row row)
    {
        
        if (row.depth > range)
        {
            return;
        }
        Vector2Int prevTile = new Vector2Int(-100, -100);

        Vector2Int[] tiles = Tiles(row);

        

        for (int i = 0; i < tiles.Length; i++)
        {           
            if (IsWall(tiles[i]) || IsSymmetric(row, tiles[i]) && !visibleTiles.Contains(quadrant.QuadTransform(tiles[i])))
            {
                float diminish = Mathf.Clamp(Vector2.Distance(transform.position, quadrant.QuadTransform(tiles[i])), 0, range * lightStrength) / range;

                temporaryLight.Add(diminish);
                visibleTiles.Add(quadrant.QuadTransform(tiles[i]));

                TileMapManager.Instance.shadowArray[Mathf.Clamp(quadrant.QuadTransform(tiles[i]).x, 0 , TileMapManager.Instance.shadowArray.GetUpperBound(0)), Mathf.Clamp(quadrant.QuadTransform(tiles[i]).y, 0, TileMapManager.Instance.shadowArray.GetUpperBound(1))] += Mathf.Clamp(lightStrength - diminish, 0 , 1f);

            }
            if (IsWall(prevTile) && IsFloor(tiles[i]))
            {
                row.startSlope = Slope(tiles[i]);
            }
            if (IsFloor(prevTile) && IsWall(tiles[i]))
            {
                Row nextRow = row.Next();
                nextRow.endSlope = Slope(tiles[i]);
                LightScan(nextRow);
            }
            prevTile = tiles[i];
        }
        if (IsFloor(prevTile))
        {
            LightScan(row.Next());
        }
        return;
    }

}
public class Quadrant : MonoBehaviour
{
    public LightDirections direction;
    private Transform source;

    public Quadrant(Transform source)
    {
        this.source = source;
    }

    public Vector2Int QuadTransform(Vector2Int tile)
    {
        //Debug.Log(ShadowCasting.source.position + " shadowcasting source");
        switch (direction)
        {
            case LightDirections.North:
                return new Vector2Int(Mathf.FloorToInt(source.position.x) + tile.y, Mathf.FloorToInt(source.position.y) - tile.x);
            case LightDirections.East:
                return new Vector2Int(Mathf.FloorToInt(source.position.x) + tile.y, Mathf.FloorToInt(source.position.y) + tile.x);
            case LightDirections.South:
                return new Vector2Int(Mathf.FloorToInt(source.position.x) + tile.x, Mathf.FloorToInt(source.position.y) + tile.y);
            case LightDirections.West:
                return new Vector2Int(Mathf.FloorToInt(source.position.x) - tile.x, Mathf.FloorToInt(source.position.y) + tile.y);
            default:
                return tile;
        }
    }
}
public class Row
{
    public float startSlope;
    public float endSlope;
    public int depth;


    public Row(int depth, float startSlope, float endSlope)
    {
        this.depth = depth;
        this.startSlope = startSlope;
        this.endSlope = endSlope;
    }
    public Row Next()
    {
        Row nextRow = new Row(depth + 1, startSlope, endSlope); ;
        return nextRow;
    }


}


