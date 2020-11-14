using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public int _distance;
    public int _space;

    public int _width;
    public int _height;

    [SerializeField]
    public GameObject[] _tilePrefabs;
    public GameObject _tileContainer;

    enum TILE_TYPES
    {
        GRASS, DIRT, STONE
    }

    void Start()
    {
        Generation();
    }

    void Generation()
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
}