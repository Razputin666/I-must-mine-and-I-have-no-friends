using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    //Used in v1
    [SerializeField]
    private int _width;
    [SerializeField]
    private int _height;
    public int _distance;
    public int _space;
    [SerializeField]
    private float _cellsize = 5f;
    [SerializeField]
    private int _chunkWidth = 10;
    [SerializeField]
    private int _chunkHeight = 10;

    [SerializeField]
    public GameObject[] _tilePrefabs;
    public GameObject _tileContainer;


    //Used in v2
    private Tilemap _tilemap;

    private TilemapChunks _tilemapChunks;
    [SerializeField]
    private TilemapVisual _tilemapVisual;

    private Tilemap.TilemapObject.TilemapSprite _tilemapSprite;

    Tilemap.TilemapObject.TilemapSprite[,] tilemapSpritearray;

    [SerializeField]
    private bool useExperimental = false;
    enum TILE_TYPES
    {
        GRASS, DIRT, STONE
    }

    void Start()
    {
        if(useExperimental)
        {
            GenerateTilesV2();

            _tilemapChunks = new TilemapChunks(_width, _height, _chunkWidth, _chunkHeight, _cellsize, Vector3.zero, tilemapSpritearray, _tilemapVisual);
            //_tilemap = new Tilemap(_width, _height, 5f, Vector3.zero, tilemapSpritearray);
            //_tilemap.SetTilemapVisual(_tilemapVisual);
        }
        else
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        _distance = _height;
        for (int w = 0; w < _width; w++)
        {
            int lowernum = _distance - 1;
            int heighernum = _distance + 2;
            _distance = Random.Range(lowernum, heighernum);
            _space = Random.Range(12, 20);
            int stonespace = _distance - _space;

            for (int j = 0; j < stonespace; j++)
            {
                GameObject newStoneTile = Instantiate(_tilePrefabs[(int)TILE_TYPES.STONE], new Vector3(w, j), Quaternion.identity);
                newStoneTile.transform.parent = _tileContainer.transform;
            }
            for (int j = stonespace; j < _distance; j++)
            {
                GameObject newDirtTile = Instantiate(_tilePrefabs[(int)TILE_TYPES.DIRT], new Vector3(w, j), Quaternion.identity);
                newDirtTile.transform.parent = _tileContainer.transform;
            }

            GameObject newGrassTile = Instantiate(_tilePrefabs[(int)TILE_TYPES.GRASS], new Vector3(w, _distance), Quaternion.identity);
            newGrassTile.transform.parent = _tileContainer.transform;
        }
    }

    private void Update()
    {
        if(useExperimental)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                _tilemap.Save();
                Debug.Log("Saved!");
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                _tilemap.Load();
                Debug.Log("Loaded!");
            }
        }
    }

    int NoiseInt(int x, int y, float scale, float mag, float exp)
    {
        return (int)Mathf.Pow((Mathf.PerlinNoise(x / scale, y / scale) * mag), (exp));
    }

    private void GenerateTilesV2()
    {
        tilemapSpritearray = new Tilemap.TilemapObject.TilemapSprite[_width, _height];

        //int w = _width / 4;
        //for(int x = 0; x < tilemapSpritearray.GetLength(0);  x++)
        //{
        //    for (int y = 0; y < tilemapSpritearray.GetLength(1); y++)
        //    {
        //        if(x < w)
        //        {
        //            tilemapSpritearray[x, y] = Tilemap.TilemapObject.TilemapSprite.Grass;
        //        }
        //        else if(x < w * 2)
        //        {
        //            tilemapSpritearray[x, y] = Tilemap.TilemapObject.TilemapSprite.Dirt;
        //        }
        //        else if(x < w * 3)
        //        {
        //            tilemapSpritearray[x, y] = Tilemap.TilemapObject.TilemapSprite.Stone;
        //        }
        //        else
        //        {
        //            tilemapSpritearray[x, y] = Tilemap.TilemapObject.TilemapSprite.None;
        //        }
        //    }
        //}

        for (int px = 0; px < tilemapSpritearray.GetLength(0); px++)
        {
            int grass = NoiseInt(px, 0, 80, 5, 1);

            // Layer one has a scale of 80 making it quite smooth with large rolling hills, 
            // the magnitude is 15 so the hills are at most 15 high (but in practice they're usually around 12 at the most) 
            // and at the least 0 and the exponent is 1 so no change is applied exponentially.
            int stone = NoiseInt(px, 0, 80, 15, 1);
            //The next layer has a smaller scale so it's more choppy (but still quite tame) 
            //and has a larger magnitude so a higher max height. This ends up being the most prominent layer making the hills.
            stone += NoiseInt(px, 0, 50, 30, 1);
            //The third layer has an even smaller scale so it's even noisier but it's magnitude is 10 so its max height is lower, 
            //it's mostly for adding some small noise to the stone to make it look more natural. Lastly we add 75 to the stone to raise it up.
            stone += NoiseInt(px, 0, 10, 10, 1);
            stone += 75;

            //The dirt layer has to be mostly higher than the stone so the magnitudes here are higher 
            //but the scales are 100 and 50 which gives us rolling hills with little noise. Again we add 75 to raise it up.
            int dirt = NoiseInt(px, 0, 100f, 35, 1);
            dirt += NoiseInt(px, 100, 50, 30, 1);
            dirt += 75;

            for (int py = 0; py < tilemapSpritearray.GetLength(1); py++)
            {
                if (py < stone)
                {
                    tilemapSpritearray[px, py] = Tilemap.TilemapObject.TilemapSprite.Stone;
                    //The next three lines make dirt spots in random places
                    if (NoiseInt(px, py, 12, 16, 1) > 10)
                    {  //dirt spots
                        tilemapSpritearray[px, py] = Tilemap.TilemapObject.TilemapSprite.Dirt;
                    }
                    //The next three lines remove dirt and rock to make caves in certain places
                    if (NoiseInt(px, py * 2, 16, 14, 1) > 10)
                    { //Caves
                        tilemapSpritearray[px, py] = Tilemap.TilemapObject.TilemapSprite.None;
                    }
                }
                else if (py < dirt)
                {
                    tilemapSpritearray[px, py] = Tilemap.TilemapObject.TilemapSprite.Grass;
                }
            }
        }
    }
}