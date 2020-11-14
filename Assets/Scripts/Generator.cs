using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public int _width;
    public int _height;
    public int _distance;
    public int _space;

    [SerializeField]
    public GameObject[] _tilePrefabs;
    public GameObject _tileContainer;

    enum TILE_TYPES
    {
        GRASS, DIRT, STONE
    }
    public byte[,] blocks;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        GenTerrain();

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (blocks[x, y] == 2)
                {
                    GameObject newTile = Instantiate(_tilePrefabs[(int)TILE_TYPES.DIRT], new Vector2(x, y), Quaternion.identity);
                    newTile.transform.parent = _tileContainer.transform;
                }
                else if (blocks[x, y] == 1)
                {
                    GameObject newTile = Instantiate(_tilePrefabs[(int)TILE_TYPES.STONE], new Vector2(x, y), Quaternion.identity);
                    newTile.transform.parent = _tileContainer.transform;
                }
                else if (blocks[x, y] == 3)
                {
                    GameObject newTile = Instantiate(_tilePrefabs[(int)TILE_TYPES.GRASS], new Vector2(x, y), Quaternion.identity);
                    newTile.transform.parent = _tileContainer.transform;
                }
            }
        }

    }
    int NoiseInt(int x, int y, float scale, float mag, float exp)
    {
        return (int)Mathf.Pow((Mathf.PerlinNoise(x / scale, y / scale) * mag), (exp));
    }

    void GenTerrain()
    {
        blocks = new byte[_width, _height];

        for (int px = 0; px < blocks.GetLength(0); px++)
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

            for (int py = 0; py < blocks.GetLength(1); py++)
            {
                if (py < stone)
                {
                    blocks[px, py] = 1;
                    //The next three lines make dirt spots in random places
                    if (NoiseInt(px, py, 12, 16, 1) > 10)
                    {  //dirt spots
                        blocks[px, py] = 2;

                    }
                    //The next three lines remove dirt and rock to make caves in certain places
                    if (NoiseInt(px, py * 2, 16, 14, 1) > 10)
                    { //Caves
                        blocks[px, py] = 0;

                    }
                }
                else if (py < dirt)
                {
                    blocks[px, py] = 2;
                }
            }
        }
    }
}