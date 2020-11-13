using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public int width;
    public int height;
    public int distance;
    public int space;

    public GameObject Grass;
    public GameObject Dirt;
    public GameObject Stone;

    public GameObject _tileContainer;

    public float heightpoint;
    public float heightpoint2;

    void Start ()
    {
        Generation();
    }

    void Generation()
    {
        distance = height;
        for (int w=0; w<width; w++)
        {
            int lowernum = distance - 1;
            int heighernum = distance + 2;
            distance = Random.Range(lowernum, heighernum);
            space = Random.Range(12, 20);
            int stonespace = distance - space;

            for (int j=0; j<stonespace; j++)
            {
                GameObject newStoneTile = Instantiate(Stone, new Vector3(w, j), Quaternion.identity);
                newStoneTile.transform.parent = _tileContainer.transform;
            }
            for (int j=stonespace; j<distance; j++)
            {
                GameObject newDirtTile = Instantiate(Dirt, new Vector3(w, j), Quaternion.identity);
                newDirtTile.transform.parent = _tileContainer.transform;
            }
            GameObject newGrassTile = Instantiate(Grass, new Vector3(w, distance), Quaternion.identity);
            newGrassTile.transform.parent = _tileContainer.transform;
        }
    }
}
