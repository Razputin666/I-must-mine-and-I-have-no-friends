using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GrassPlanetOverworldChunk : MonoBehaviour
{

    [SerializeField] private LevelGeneratorLayered mapGen;
    int height;
    int width;

    [Tooltip("The settings of this chunk")]
    [SerializeField]
    public List<MapSettings> chunkSettings = new List<MapSettings>();

    public void GenerateGrassPlanetOverworldChunk(Tilemap tilemap)
    {
        height = mapGen.height;
        width = mapGen.width;
        CreateTopTerrain(tilemap);
        CreateFeatures(tilemap);
        CreateCaves(tilemap);

    }

    void CreateTopTerrain(Tilemap tilemap)
    {
        int spawnPositionX = 0;
        int spawnPositionY = mapGen.height - 1;
        int spawnWidthX = width;
        int spawnWidthY = height / 10;

        int numberOfLoops = 10; // Always 10 loops
        int minWalk = UnityEngine.Random.Range(2, 7);
        float hillModifier = 0.01f;
        mapGen.GenerateMapFeatures(chunkSettings[3], new Vector2Int(spawnPositionX, spawnPositionY), spawnWidthX, spawnWidthY, true, minWalk, hillModifier, tilemap);

        for (int i = 0; i < numberOfLoops - 1; i++)
        {
            minWalk = UnityEngine.Random.Range(2, 7);
            mapGen.GenerateMapFeatures(chunkSettings[3], new Vector2Int(spawnPositionX, spawnPositionY += UnityEngine.Random.Range(0, 2)), spawnWidthX, spawnWidthY, true, minWalk, hillModifier, tilemap);
        }

    }

    void CreateFeatures(Tilemap tilemap)
    {
        int startPositionX = 0;
        int startPositionY = 0;
        int spawnWidth = width;
        int spawnHeight = height / 10;

        int numberOfLoops = 10; // Always 10 loops
        int amountToFill = chunkSettings[0].fillAmount;

        mapGen.GenerateMapFeatures(chunkSettings[0], new Vector2Int(startPositionX, startPositionY), spawnWidth, spawnHeight, false, amountToFill, 0, tilemap);

        for (int i = 0; i < numberOfLoops - 1; i++)
        {
            amountToFill += 3;
            mapGen.GenerateMapFeatures(chunkSettings[0], new Vector2Int(startPositionX, startPositionY += spawnHeight - 2), spawnWidth, spawnHeight, false, amountToFill, 0, tilemap);
            mapGen.GenerateMapFeatures(chunkSettings[2], new Vector2Int(startPositionX, startPositionY), spawnWidth, spawnHeight, false, amountToFill, 0, tilemap);

        }
        amountToFill = chunkSettings[0].fillAmount + chunkSettings[0].fillAmount / 2;
        mapGen.GenerateMapFeatures(chunkSettings[0], new Vector2Int(startPositionX, 0), spawnWidth, height, false, amountToFill, 0, tilemap);

    }

    void CreateCaves(Tilemap tilemap)
    {

        int numberOfLoops = UnityEngine.Random.Range((width + height) / 40, (width + height) / 20); // This becomes 66 caves in a 500x500 chunk


        for (int i = 0; i < numberOfLoops; i++)
        {

            int spawnPositionY = UnityEngine.Random.Range(0, height / 2);
            int spawnHeightY = height / 2;
            int spawnPositionX = UnityEngine.Random.Range(0, width / 2);
            int spawnWidthX = width / 2;


            if (spawnPositionX > (width / 2 - 20))
            {
                spawnPositionX -= width / 100;
                spawnWidthX = width / 100 - 1;
            }

            if (spawnPositionY > (height / 2 - 20))
            {
                spawnPositionY -= (height / 100);
                spawnHeightY = height / 100 - 1;
            }

            if (spawnPositionX + spawnWidthX >= width / 2)
            {
                int checker = (spawnPositionX + spawnWidthX - width / 2) - 1;
                spawnWidthX += checker;

            }

            if (spawnPositionY + spawnHeightY >= height / 2)
            {
                int checker = (spawnPositionY + spawnHeightY - height / 2) - 1;
                spawnHeightY += checker;

            }

            mapGen.GenerateMainMap(chunkSettings[2], new Vector2Int(spawnPositionX, spawnPositionY), spawnWidthX, spawnHeightY, false, tilemap);
        }
    }

    void CreateOres(Tilemap tilemap)
    {
        int startPositionX = 0;
        int startPositionY = 0;
        int spawnWidth = width;
        int spawnHeight = height / 10;

        int numberOfLoops = 5;
        int amountToFill = chunkSettings[0].fillAmount;

        mapGen.GenerateMapFeatures(chunkSettings[0], new Vector2Int(startPositionX, startPositionY), spawnWidth, spawnHeight, false, amountToFill, 0, tilemap);

        for (int i = 0; i < numberOfLoops - 1; i++)
        {
            amountToFill += 3;
            mapGen.GenerateMapFeatures(chunkSettings[0], new Vector2Int(startPositionX, startPositionY += spawnHeight - 2), spawnWidth, spawnHeight, false, amountToFill, 0, tilemap);
            mapGen.GenerateMapFeatures(chunkSettings[2], new Vector2Int(startPositionX, startPositionY), spawnWidth, spawnHeight, false, amountToFill, 0, tilemap);

        }
        amountToFill = chunkSettings[0].fillAmount + chunkSettings[0].fillAmount / 2;
        mapGen.GenerateMapFeatures(chunkSettings[0], new Vector2Int(startPositionX, 0), spawnWidth, height, false, amountToFill, 0, tilemap);

    }

}
