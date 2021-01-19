using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GrassPlanetUnderworldChunk : MonoBehaviour
{

    /*
   * 
  0 = Dirt
  1 = Stone
  2 = 
  3 = Tree
  4 = Copper
  5 = Iron

   * */
    [SerializeField] private LevelGeneratorLayered mapGen;
    int height;
    int width;
    int firstChunkHeight;
    int secondChunkHeight;



    [Tooltip("The settings of this chunk")]
    [SerializeField]
    public List<MapSettings> chunkSettings = new List<MapSettings>();
    private List<Tilemap> chunkList;


    public void GenerateGrassPlanetUnderWorldChunk(Tilemap chunk)
    {
        height = mapGen.height;
        width = mapGen.width;
        CreateFeatures(chunk);
        CreateCaves(chunk);
        CreateCopper(chunk);
        CreateIron(chunk);
        CreateGold(chunk);
        //CreateCoal(chunk);
        chunkList = mapGen.chunks;
        RemoveLoneBlocks(chunk);
    }

    void CreateFeatures(Tilemap tilemap)
    {
        int spawnPositionX = 0;
        int spawnPositionY = 0;
        int spawnWidthX = width;
        int spawnHeightY = height;

        int numberOfLoops = 24; // Always 12 loops
        int amountToLeave = chunkSettings[0].fillAmount + 40;
        int[,] mapCoords = new int[width, height];
        float seed = UnityEngine.Random.Range(0f, 1f);

        // mapGen.GenerateMapFeatures(chunkSettings[0], new Vector2Int(spawnPositionX, spawnPositionY), spawnWidthX, spawnHeightY, false, amountToFill, 0, tilemap);

        for (int i = 0; i < numberOfLoops - 1; i++)
        {
            mapCoords = MapFunctions.GenerateCellularAutomata(spawnWidthX, spawnHeightY, seed, amountToLeave, false);
            mapCoords = MapFunctions.SmoothVNCellularAutomata(mapCoords, false, amountToLeave);
            MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[1], new Vector2Int(spawnPositionX, spawnPositionY));
            // mapGen.GenerateMapFeatures(chunkSettings[0], new Vector2Int(spawnPositionX, spawnPositionY += spawnHeightY - 2), spawnWidthX, spawnHeightY, false, amountToFill, 0, tilemap);
            // mapGen.GenerateMapFeatures(chunkSettings[2], new Vector2Int(spawnPositionX, spawnPositionY + (spawnHeightY / 10)), spawnWidthX, height, false, amountToLeave, 0, tilemap);
            mapCoords = MapFunctions.GenerateArray(spawnWidthX, spawnHeightY, false);
            mapCoords = MapFunctions.RandomWalkCaveCustom(mapCoords, seed, amountToLeave, numberOfLoops * 4);
            MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[1], new Vector2Int(spawnPositionX, spawnPositionY));
           // spawnPositionY += spawnHeightY - 2;
            seed = UnityEngine.Random.Range(0f, 1f);
        }

        //mapCoords = MapFunctions.GenerateArray(spawnWidthX, spawnHeightY, false);
        //mapCoords = MapFunctions.RandomWalkCaveCustom(mapCoords, seed, amountToLeave - 30, numberOfLoops * 8);
        //MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[0], new Vector2Int(spawnPositionX, spawnPositionY));
        //amountToLeave = chunkSettings[0].fillAmount + chunkSettings[0].fillAmount / 2;

        //mapCoords = MapFunctions.GenerateCellularAutomata(spawnWidthX, 0, seed, amountToLeave, false);
        //mapCoords = MapFunctions.SmoothVNCellularAutomata(mapCoords, false, chunkSettings[0].smoothAmount);
        //MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[1], new Vector2Int(spawnPositionX, spawnPositionY));
        //  mapGen.GenerateMapFeatures(chunkSettings[0], new Vector2Int(spawnPositionX, 0), spawnWidthX, height, false, amountToFill, 0, tilemap);

    }

    void CreateCaves(Tilemap tilemap)
    {

        int numberOfLoops = UnityEngine.Random.Range((width + height) / 15, (width + height) / 10); // This becomes 66 caves in a 500x500 chunk


        for (int i = 0; i < numberOfLoops; i++)
        {

            int spawnPositionY = UnityEngine.Random.Range(0, height / 2);
            int spawnHeightY = height / 2;
            int spawnPositionX = UnityEngine.Random.Range(0, width / 2);
            int spawnWidthX = width / 2;


            //if (spawnPositionX > (width / 2 - 20))
            //{
            //    spawnPositionX -= width / 100;
            //    spawnWidthX = width / 100 - 1;
            //}

            //if (spawnPositionY > (height / 2 - 20))
            //{
            //    spawnPositionY -= (height / 100);
            //    spawnHeightY = height / 100 - 1;
            //}

            //if (spawnPositionX + spawnWidthX >= width / 2)
            //{
            //    int checker = (spawnPositionX + spawnWidthX - width / 2) - 1;
            //    spawnWidthX += checker;

            //}

            //if (spawnPositionY + spawnHeightY >= height / 2)
            //{
            //    int checker = (spawnPositionY + spawnHeightY - height / 2) - 1;
            //    spawnHeightY += checker;

            //}
            int[,] mapCoords = new int[width, height];
            float seed = UnityEngine.Random.Range(0f, 1f);
            int removeAmount = chunkSettings[2].fillAmount;
            // mapGen.GenerateMainMap(chunkSettings[2], new Vector2Int(spawnPositionX, spawnPositionY), spawnWidthX, spawnHeightY, false, tilemap);
            mapCoords = MapFunctions.GenerateArray(Mathf.Clamp(spawnWidthX, 1, width / 2), Mathf.Clamp(spawnHeightY, 1, height / 2), false);
          //  Debug.Log(mapCoords.GetLength(0) + " width in chunk " + mapCoords.GetLength(1) + " height in chunk");
            mapCoords = MapFunctions.RandomWalkCaveCustom(mapCoords, seed, removeAmount, numberOfLoops);
            MapFunctions.RemoveExistingBlocks(mapCoords, tilemap, new Vector2Int(spawnPositionX, spawnPositionY));

        }

    }

    void CreateCoal(Tilemap tilemap)
    {
        int spawnPositionX = 0;
        int spawnPositionY = 0;
        int spawnWidthX = width;
        int spawnHeightY = height + (height / 100);

        int amountToLeave = chunkSettings[0].fillAmount;
        float seed = UnityEngine.Random.Range(0f, 1f);
        int[,] mapCoords = new int[width, height];


        spawnPositionY = 0;
        amountToLeave = chunkSettings[0].fillAmount + chunkSettings[0].fillAmount - 10;
        mapCoords = MapFunctions.GenerateCellularAutomata(spawnWidthX, spawnHeightY, seed, amountToLeave, false);
        mapCoords = MapFunctions.SmoothVNCellularAutomata(mapCoords, false, amountToLeave);
        MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[7], new Vector2Int(spawnPositionX, spawnPositionY));

    }

    void CreateGold(Tilemap tilemap)
    {
        int spawnPositionX = 0;
        int spawnPositionY = 0;
        int spawnWidthX = width;
        int spawnHeightY = height;

        int amountToLeave = chunkSettings[0].fillAmount;
        float seed = UnityEngine.Random.Range(0f, 1f);
        int[,] mapCoords = new int[width, height];


        spawnPositionY = 0;
        amountToLeave = chunkSettings[0].fillAmount + chunkSettings[0].fillAmount - 7;
        mapCoords = MapFunctions.GenerateCellularAutomata(spawnWidthX, spawnHeightY, seed, amountToLeave, false);
        mapCoords = MapFunctions.SmoothVNCellularAutomata(mapCoords, false, amountToLeave);
        MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[6], new Vector2Int(spawnPositionX, spawnPositionY));

    }

    void CreateIron(Tilemap tilemap)
    {
        int spawnPositionX = 0;
        int spawnPositionY = 0;
        int spawnWidthX = width;
        int spawnHeightY = height / 10;

        int numberOfLoops = 10;
        int amountToLeave = chunkSettings[0].fillAmount;
        float seed = UnityEngine.Random.Range(0f, 1f);
        int[,] mapCoords = new int[width, height];
        amountToLeave = chunkSettings[0].fillAmount + chunkSettings[0].fillAmount - 15;

        for (int i = 0; i < numberOfLoops; i++)
        {

            mapCoords = MapFunctions.GenerateCellularAutomata(spawnWidthX, spawnHeightY, seed, amountToLeave, false);
            mapCoords = MapFunctions.SmoothVNCellularAutomata(mapCoords, false, amountToLeave);
            MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[5], new Vector2Int(spawnPositionX, spawnPositionY));
            amountToLeave += 2;
            seed = UnityEngine.Random.Range(0f, 1f);
            spawnPositionY += spawnHeightY - 2;
        }
        spawnPositionY = 0;
        amountToLeave = chunkSettings[0].fillAmount + chunkSettings[0].fillAmount - 7;
        mapCoords = MapFunctions.GenerateCellularAutomata(spawnWidthX, height, seed, amountToLeave, false);
        mapCoords = MapFunctions.SmoothVNCellularAutomata(mapCoords, false, amountToLeave);
        MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[5], new Vector2Int(spawnPositionX, spawnPositionY));

    }
    void CreateCopper(Tilemap tilemap)
    {
        int spawnPositionX = 0;
        int spawnPositionY = 0;
        int spawnWidthX = width;
        int spawnHeightY = height / 10;

        int numberOfLoops = 10;
        int amountToLeave = chunkSettings[0].fillAmount;
        float seed = UnityEngine.Random.Range(0f, 1f);
        int[,] mapCoords = new int[width, height];
        amountToLeave = chunkSettings[0].fillAmount + chunkSettings[0].fillAmount - 5;

        for (int i = 0; i < numberOfLoops; i++)
        {

            mapCoords = MapFunctions.GenerateCellularAutomata(spawnWidthX, spawnHeightY, seed, amountToLeave, false);
            mapCoords = MapFunctions.SmoothVNCellularAutomata(mapCoords, false, amountToLeave);
            MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[4], new Vector2Int(spawnPositionX, spawnPositionY));
            seed = UnityEngine.Random.Range(0f, 1f);
            spawnPositionY += spawnHeightY - 2;
        }
        spawnPositionY = 0;
        amountToLeave = chunkSettings[0].fillAmount + chunkSettings[0].fillAmount - 9;
        mapCoords = MapFunctions.GenerateCellularAutomata(spawnWidthX, height, seed, amountToLeave, false);
        mapCoords = MapFunctions.SmoothVNCellularAutomata(mapCoords, false, amountToLeave);
        MapFunctions.ChangeExistingBlocks(mapCoords, tilemap, mapGen.tiles[4], new Vector2Int(spawnPositionX, spawnPositionY));

    }

    void RemoveLoneBlocks(Tilemap chunk)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height + (height / 2); y++)
            {

                if (chunk.HasTile(new Vector3Int(x, y, 0)) && !chunk.HasTile(new Vector3Int(x + 1, y, 0)) && !chunk.HasTile(new Vector3Int(x - 1, y, 0)) && !chunk.HasTile(new Vector3Int(x, y + 1, 0)) && !chunk.HasTile(new Vector3Int(x, y - 1, 0)))
                {
                    chunk.SetTile(new Vector3Int(x, y, 0), null);
                }
            }

        }

        // (chunk.HasTile(new Vector3Int(x, y, 0)) && !chunk.HasTile(new Vector3Int(x + 1, y, 0)) && !chunk.HasTile(new Vector3Int(x - 1, y, 0)) && !chunk.HasTile(new Vector3Int(x, y + 1, 0)) && !chunk.HasTile(new Vector3Int(x, y - 1, 0)))

    }
}
