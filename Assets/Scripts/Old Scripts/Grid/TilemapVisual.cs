using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TilemapVisual : MonoBehaviour 
{
    [System.Serializable]
    public struct TilemapSpriteUV
    {
        public TilemapOLD.TilemapObject.TilemapSprite _tilemapSprite;
        public Vector2Int uv00Pixels;
        public Vector2Int uv11Pixels;
    }

    private struct UVCoords
    {
        public Vector2 uv00;
        public Vector2 uv11;
    }
 
    [SerializeField]
    private TilemapSpriteUV[] tilemapSpriteUVArray;
    private Grid<TilemapOLD.TilemapObject> grid;
    private Mesh mesh;
    private bool updateMesh;
    private Dictionary<TilemapOLD.TilemapObject.TilemapSprite, UVCoords> uvCoordsDictionary;

    private bool colliderHasGenerated;


    //[SerializeField]
    //private TerrainGenerator terrainGenerator;

    //[SerializeField]
    //private ColliderManager colliderManager;
    
    [SerializeField]
    public GameObject tileBlock;
    GameObject[,] tileBlocks;
    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //Get the width and height from material.
        Texture texture = GetComponent<MeshRenderer>().material.mainTexture;
        float textureWidth = texture.width;
        float textureHeight = texture.height;
        uvCoordsDictionary = new Dictionary<TilemapOLD.TilemapObject.TilemapSprite, UVCoords>();
        foreach (TilemapSpriteUV tilemapSpriteUV in tilemapSpriteUVArray)
        {
            uvCoordsDictionary[tilemapSpriteUV._tilemapSprite] = new UVCoords
            {
                //Normalize the Uv Coordinates
                uv00 = new Vector2((float)tilemapSpriteUV.uv00Pixels.x / textureWidth, (float)tilemapSpriteUV.uv00Pixels.y / textureHeight),
                uv11 = new Vector2((float)tilemapSpriteUV.uv11Pixels.x / textureWidth, (float)tilemapSpriteUV.uv11Pixels.y / textureHeight)
            };
        }
    }

    public void SetGrid(TilemapOLD tilemap, Grid<TilemapOLD.TilemapObject> grid)
    {
        this.grid = grid;
        UpdateTilemapVisual();

        grid.OnGridObjectChanged += Grid_OnGridValueChanged;
        //Subscribe to event
        tilemap.OnLoaded += Tilemap_OnLoaded;
    }

    public void Tilemap_OnLoaded(object sender, System.EventArgs e)
    {
        updateMesh = true;
    }

    private void Grid_OnGridValueChanged(object sender, Grid<TilemapOLD.TilemapObject>.OnGridObjectChangedEventArgs e)
    {
        updateMesh = true;
    }

    private void LateUpdate()
    {
        if(updateMesh)
        {
            updateMesh = false;
            UpdateTilemapVisual();
        }
    }

    //private void Update()
    //{

    //    if (created)
    //    {
    //        float x = Camera.main.transform.position.x;
    //        float y = Camera.main.transform.position.x;

    //        Vector3 worldPosBL = grid.GetWorldPosition(0, 0);
    //        Vector3 worldPosBR = grid.GetWorldPosition(grid.GetWidth(), 0);
    //        Vector3 worldPosTL = grid.GetWorldPosition(0, grid.GetHeight());
    //        Vector3 worldPosTR = grid.GetWorldPosition(grid.GetWidth(), grid.GetHeight());

    //        //Debug.LogError("1: " + worldPosBL + " : " + worldPosBR);
    //        //Debug.LogError("2: " + worldPosTL + " : " + worldPosTR);

    //        if ((x >= worldPosBL.x && x <= worldPosTR.x) && (y >= worldPosBL.y && y <= worldPosTR.y))
    //        {
    //            Debug.Log("1: " + worldPosBL + " : " + worldPosTR);
    //            Debug.Log("2: " + x + " : " + y);
    //            GetComponent<MeshRenderer>().enabled = true;
    //        }
    //        else
    //        {
    //            GetComponent<MeshRenderer>().enabled = false;
    //        }
    //    }
    //}

    //IEnumerator BoxColliderCreator()
    //{
    //    Collider2D coll = gameObject.AddComponent<Collider2D>();

    //    for (int x = 0; x < grid.GetWidth(); x++)
    //    {
    //        for (int y = 0; y < grid.GetHeight(); y++)
    //        {
    //            Tilemap.TilemapObject gridObject = grid.GetGridObject(x, y);

    //            if (gridObject.GetBoxCollider2D() == null)
    //            {
    //                BoxCollider2D boxColl = gameObject.AddComponent<BoxCollider2D>();
    //                gridObject.SetBoxCollider2D(boxColl);
    //                Debug.Log(boxColl);
    //            }
    //        }
    //    }
    //    yield return null;
    //}

    private void UpdateTilemapVisual()
    {
        //StartCoroutine(BoxColliderCreator());
        MeshUtils.CreateEmptyMeshArrays(grid.GetWidth() * grid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        //if (tileBlocks != null)
        //    Array.Clear(tileBlocks, 0, tileBlocks.Length - 1);

        if (!colliderHasGenerated)
            tileBlocks = new GameObject[grid.GetWidth(),grid.GetHeight()];

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for(int y = 0; y < grid.GetHeight(); y++)
            {
                int index = x * grid.GetHeight() + y;
                
                Vector3 quadSize = new Vector3(1, 1) * grid.GetCellSize();
                Vector3 baseSize = quadSize;
                TilemapOLD.TilemapObject gridObject = grid.GetGridObject(x, y);

                TilemapOLD.TilemapObject.TilemapSprite tilemapSprite = gridObject.GetTilemapSprite();

                if (!colliderHasGenerated)
                {

                    tileBlocks[x, y] = Instantiate(tileBlock, grid.GetWorldPosition(x, y), Quaternion.identity, gameObject.transform);

                }
                Vector2 gridUV00, gridUV11;
                if(tilemapSprite == TilemapOLD.TilemapObject.TilemapSprite.None)
                {
                    //if no tile set the texture coordinates to 0 and dont render
                    gridUV00 = Vector2.zero;
                    gridUV11 = Vector2.zero;
                    quadSize = Vector3.zero;
                    tileBlocks[x, y].SetActive(false);
                    tileBlocks[x, y].GetComponent<BoxCollider2D>().enabled = false;
                }
                else
                {
                    //Lookup the uv Coordinates for the tilemapSprite.
                    UVCoords uvCoords = uvCoordsDictionary[tilemapSprite];

                  //  Instantiate(tileBlock, grid.GetWorldPosition(x, y), Quaternion.identity, gameObject.transform);
                    gridUV00 = uvCoords.uv00;
                    gridUV11 = uvCoords.uv11;
                    tileBlocks[x, y].SetActive(true);
                    tileBlocks[x, y].GetComponent<BoxCollider2D>().enabled = true;
                }
                MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(x, y) + quadSize * 0.5f, 0f, quadSize, gridUV00, gridUV11);
            }
            
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        Grid theGrid = gameObject.GetComponent<Grid>();
        Vector3 sizeOfCell = new Vector3(grid.GetCellSize(), grid.GetCellSize());
        theGrid.cellSize = sizeOfCell;
        BoxCollider2D collider = gameObject.GetComponent<BoxCollider2D>();
       // CompositeCollider2D tileCollider = gameObject.GetComponent<CompositeCollider2D>();
        Vector2 sizeOfMap = new Vector2(250, 250); // sizeOfMap ska vara (width * 10) / 2, (height * 10) / 2
        collider.size = sizeOfMap /100;
        // gameObject.GetComponent<MeshRenderer>().transform.position
        colliderHasGenerated = true;
        // colliderManager.bwatevs(tileBlocks, grid.GetWidth(), grid.GetHeight());
    }



    public TilemapOLD.TilemapObject GetGridObjectAtXY(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }
    public TilemapOLD.TilemapObject GetGridObjectAtXY(Vector3 position)
    {
        return grid.GetGridObject(position);
    }

}