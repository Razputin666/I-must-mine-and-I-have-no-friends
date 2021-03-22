﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

public class DefaultPlanetGeneration : Worldgeneration
{
    private const int skyChunks = 5;

    protected override void Init()
    {
        base.Init();

        tilebaseLookup = new Dictionary<string, TileBase>();
        blocks = Resources.LoadAll<TileBase>("Tilebase");
        foreach (var tilebase in blocks)
        {
            tilebaseLookup.Add(tilebase.name, tilebase);
        }

        sightBlock = Instantiate(foreGroundPrefab, grid.transform);
        sightBlock.GetComponent<TilemapSyncer>().SetName("Vision Chunk");
        NetworkServer.Spawn(sightBlock);
        sightBlock.transform.position = new Vector2(startPosition.x, startPosition.y);


        for (int x = 0; x < horizontalChunks; x++)
        {
            startPosition.y = 0;
            for (int y = 0; y < verticalChunks; y++)
            {
                int index = x * verticalChunks + y;
                GameObject chunk = Instantiate(chunkPrefab, grid.transform);
                chunk.GetComponent<TilemapSyncer>().SetName("Chunk_" + index);
                TileMapManager.Instance.AddTileChunk(chunk.GetComponent<Tilemap>());
                NetworkServer.Spawn(chunk);
                chunk.transform.position = new Vector2(startPosition.x, startPosition.y);
                startPosition.y += height;
                //if (y == verticalChunks - 1)
                //{
                //    for (int i = 0; i < verticalChunks; i++)
                //    {
                //        GameObject chunkSky = Instantiate(chunkPrefab, grid.transform);
                //        chunkSky.GetComponent<TilemapSyncer>().SetName("Chunk_" + index * i + 1);
                //        TileMapManager.Instance.AddTileChunk(chunkSky.GetComponent<Tilemap>());
                //        NetworkServer.Spawn(chunkSky);
                //        chunkSky.transform.position = new Vector2(startPosition.x, startPosition.y);
                //        startPosition.y += height;
                //    }
                //}
            }
            startPosition.x += width;
        }
    }
    public override void OnStartServer()
    {
        Init();


        NativeArray<int> worldArray = new NativeArray<int>(GetWorldWidth * GetWorldHeight, Allocator.TempJob);
        //NativeArray<int> worldArrayHalf = new NativeArray<int>(GetWorldWidth * GetWorldHeight / 2, Allocator.TempJob);
        NativeArray<int> topLayer = new NativeArray<int>(GetWorldWidth * height, Allocator.TempJob);
        NativeArray<int>[] chunkArray = new NativeArray<int>[horizontalChunks * verticalChunks];

        GenerateJobs(worldArray);
        CreateTopTerrain(topLayer);
        CreateCaveTerrain(worldArray);
        AddCommonBlocks(worldArray);
        AddOreBlocks(worldArray, (int)BlockTypeConversion.CopperBlock);
        AddOreBlocks(worldArray, (int)BlockTypeConversion.IronBlock);
        AddOreBlocks(worldArray, (int)BlockTypeConversion.CoalBlock);
        AddOreBlocks(worldArray, (int)BlockTypeConversion.GoldBlock);

        //Copy values from the array with the topWalk values to the array with the entire world

        for (int x = 0; x < (GetWorldWidth); x++)
        {
            for (int y = 0; y < height; y++)
            {
                int topLayerIndex = x * height + y;
                int index = x * GetWorldHeight + y + (verticalChunks - 1) * height;
                if (topLayer[topLayerIndex] == (int)BlockTypeConversion.Empty)
                {
                    worldArray[index] = topLayer[topLayerIndex];
                }
            }
        }



        //Copy worldarray(contains the entire world) to Chunk(same size as a tilemap)
        for (int i = 0; i < horizontalChunks; i++)
        {
            for (int j = 0; j < verticalChunks; j++)
            {
                //Calculate the correct index of the Chunk.
                int currentChunkIndex = i * verticalChunks + j;
                chunkArray[currentChunkIndex] = new NativeArray<int>(width * height, Allocator.TempJob);
                NativeArray<int> currentChunk = chunkArray[currentChunkIndex];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        //Calculate the indexPosition of the Tiles
                        int chunkIndexPos = x * height + y;
                        //Calculate the index position from the array that has the entire world
                        int index = x * (GetWorldHeight) + y + j * height + i * (GetWorldHeight) * width;
                        //Copy Tile from the world array to the chunkArray
                        currentChunk[chunkIndexPos] = worldArray[index];
                    }
                }
            }
        }

        Render(chunkArray);

        int skyHeight = skyChunks * height;
        int[,] worldAs2D = new int[GetWorldWidth, GetWorldHeight + skyHeight];
        NativeArray<int> fullArray = new NativeArray<int>(worldArray.Length + (skyHeight * GetWorldWidth), Allocator.Persistent);
        startPosition = new Vector2Int(0, GetWorldHeight);
        for (int x = 0; x < horizontalChunks; x++)
        {
            startPosition.y = GetWorldHeight - 1;
            for (int y = 0; y < skyChunks; y++)
            {
                int index = (x * (verticalChunks) + y) + verticalChunks * horizontalChunks;
                GameObject chunk = Instantiate(chunkPrefab, grid.transform);
                chunk.GetComponent<TilemapSyncer>().SetName("SkyChunk_" + index);
                TileMapManager.Instance.AddTileChunk(chunk.GetComponent<Tilemap>());
                NetworkServer.Spawn(chunk);
                chunk.transform.position = new Vector2(startPosition.x, startPosition.y);
                startPosition.y += height;
            }
            startPosition.x += width;
        }
        for (int x = 0; x < GetWorldWidth; x++)
        {
            for (int y = 0; y < GetWorldHeight; y++)
            {
                worldAs2D[x, y] = worldArray[x * GetWorldHeight + y];
                fullArray[(x * GetWorldHeight + y) + (x * skyHeight)] = worldArray[x * GetWorldHeight + y];
            }
        }


        // worldAs2D = new int[worldAs2D.GetUpperBound(0),worldAs2D.GetUpperBound(1) + (skyChunks * height)];
        verticalChunks += skyChunks;
        //int[] skyChunkArray = new int[GetWorldWidth * (skyChunks * height)];
        // worldArray.CopyFrom(skyChunkArray);
        TileMapManager.Instance.shadowArray = new NativeArray<float3>(GetWorldWidth * GetWorldHeight, Allocator.Persistent);
        TileMapManager.Instance.worldArray = worldAs2D;
        TileMapManager.Instance.nativeWorldArray = fullArray;
        TileMapManager.Instance.shadowArray2D = new float[GetWorldWidth, GetWorldHeight];
        
        //Cleanup
        for (int i = 0; i < chunkArray.Length; i++)
        {
            chunkArray[i].Dispose();
        }
        topLayer.Dispose();
        worldArray.Dispose();
      //  InvokeRepeating("UpdateMap", 1f, 1f);

    }

    private NativeArray<int> AddCommonBlocks(NativeArray<int> mapCoords)
    {

        //float rnd = Random.Range(0.01f, 0.2f);
        const float modifier = 0.15f;
        int newPoint;
        for (int x = 0; x < GetWorldWidth; x++)
        {
            for (int y = 0; y < GetWorldHeight; y++)
            {
                int edgeWall = UnityEngine.Random.Range(0, 1);
                if ((x == 0 || y == 0 || x == GetWorldWidth - 1 || y == GetWorldHeight - 1) && edgeWall > 0)
                {
                    //Keep the edges as walls
                    mapCoords[x * GetWorldHeight + y] = (int)BlockTypeConversion.StoneBlock;
                }
                else
                {
                    //Generate a new point using perlin noise, then round it to a value of either 0 or 1
                    int intRnd = UnityEngine.Random.Range(0, GetWorldHeight * 2);
                    // int clampedHeight = Mathf.Clamp(y, height * verticalChunks / 2, height * verticalChunks);
                    newPoint = Mathf.RoundToInt(Mathf.PerlinNoise(x * modifier, y * modifier));
                    if (newPoint == 1 && mapCoords[x * GetWorldHeight + y] > 0 && intRnd > y)
                    {

                        mapCoords[x * GetWorldHeight + y] = (int)BlockTypeConversion.StoneBlock;
                    }
                    else if (mapCoords[x * GetWorldHeight + y] > 0)
                    {
                        mapCoords[x * GetWorldHeight + y] = (int)BlockTypeConversion.DirtBlock;
                    }

                }
            }
        }
        //for (int x = 0; x < width * horizontalChunks; x++)
        //{
        //    for (int y = 0; y < height * verticalChunks; y++)
        //    {
        //        int intRnd = Random.Range(0, height * verticalChunks);
        //        int clampedHeight = Mathf.Clamp(y, height * verticalChunks / 2, height * verticalChunks);
        //        int index = x * height * verticalChunks + y;
        //        if (mapCoords[index] >= 1 && clampedHeight < intRnd)
        //        {
        //            mapCoords[index] = (int)BlockTypeConversion.Stone;
        //        }
        //    }
        //}
        return mapCoords;
    }



    private NativeArray<int> AddOreBlocks(NativeArray<int> mapCoords, int block)
    {
        int worldWidth = GetWorldWidth;
        int worldHeight = GetWorldHeight;
        int modifier = OreModifier(block);


        float seed = UnityEngine.Random.Range(0f, 1f);

        //Seed our random
        System.Random rand = new System.Random(seed.GetHashCode());

        //Define our start x position
        int floorX = UnityEngine.Random.Range(0, (worldWidth) - 1);
        //Define our start y position
        int floorY = UnityEngine.Random.Range(0, (worldHeight * modifier / copperModifier) - 1);
        //Determine our required floorAmount
        int reqFloorAmount = modifier / oreAmount; //(worldHeight * worldWidth) / modifier;
        //Debug.Log(reqFloorAmount);
        //Used for our while loop, when this reaches our reqFloorAmount we will stop tunneling
        int floorCount = 0;
        // Debug.Log(map.GetLength(0) + " width " + map.GetLength(1) + " height");
        //Set our start position to not be a tile (0 = no tile, 1 = tile)
        mapCoords[floorX * worldHeight + floorY] = 1;
        //Increase our floor count
        floorCount++;
        //int maxLoops = Random.Range(verticalChunks + horizontalChunks / 10, verticalChunks * horizontalChunks / 10);
        int maxLoops = verticalChunks * horizontalChunks;
        int numberOfLoops = maxLoops;

        for (int i = 0; i < numberOfLoops * modifier / copperModifier; i++)
        {
            floorX = UnityEngine.Random.Range(0, (worldWidth) - 1);
            floorY = UnityEngine.Random.Range(0, (worldHeight * modifier / copperModifier) - 1);
            floorCount = 1;
            maxLoops = numberOfLoops;

            while (floorCount < reqFloorAmount && maxLoops > 0)
            {
                //Determine our next direction
                int randDir = rand.Next(8);

                switch (randDir)
                {
                    case 0: //North-West
                        //Ensure we don't go off the map
                        if ((floorY + 1) < worldHeight && (floorX - 1) > 0)
                        {
                            //Move the y up 
                            floorY++;
                            //Move the x left
                            floorX--;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] >= 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = block;
                                //Increase floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 1: //North
                        //Ensure we don't go off the map
                        if ((floorY + 1) < worldHeight)
                        {
                            //Move the y up
                            floorY++;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] >= 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = block;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 2: //North-East
                        //Ensure we don't go off the map
                        if ((floorY + 1) < worldHeight && (floorX + 1) < worldWidth)
                        {
                            //Move the y up
                            floorY++;
                            //Move the x right
                            floorX++;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] >= 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = block;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 3: //East
                        //Ensure we don't go off the map
                        if ((floorX + 1) < worldWidth)
                        {
                            //Move the x right
                            floorX++;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] >= 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = block;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 4: //South-East
                        //Ensure we don't go off the map
                        if ((floorY - 1) > 0 && (floorX + 1) < worldWidth)
                        {
                            //Move the y down
                            floorY--;
                            //Move the x right
                            floorX++;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] >= 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = block;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 5: //South
                        //Ensure we don't go off the map
                        if ((floorY - 1) > 0)
                        {
                            //Move the y down
                            floorY--;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] >= 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = block;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 6: //South-West
                        //Ensure we don't go off the map
                        if ((floorY - 1) > 0 && (floorX - 1) > 0)
                        {
                            //Move the y down
                            floorY--;
                            //move the x left
                            floorX--;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] >= 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = block;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 7: //West
                        //Ensure we don't go off the map
                        if ((floorX - 1) > 0)
                        {
                            //Move the x left
                            floorX--;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] >= 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = block;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                }

                maxLoops--;
            }
        }
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                if (x * worldHeight + y + 1 < worldHeight * worldWidth && x * worldHeight + y - 1 > 0 && (x + 1) * worldHeight + y < worldHeight * worldWidth && (x - 1) * worldHeight + y > 0)
                {
                    if (mapCoords[x * worldHeight + y + 1] == 0 && mapCoords[x * worldHeight + y - 1] == 0 && mapCoords[(x + 1) * worldHeight + y] == 0 && mapCoords[(x - 1) * worldHeight + y] == 0)
                    {
                        mapCoords[x * worldHeight + y] = 0;
                    }
                }
            }
        }
        return mapCoords;
    }


    private NativeArray<int> CreateTopTerrain(NativeArray<int> mapCoords)
    {
        int heighRandomize = UnityEngine.Random.Range(2, 7);
        float seed = UnityEngine.Random.Range(0f, 1f);
        //Seed our random
        System.Random rand = new System.Random(seed.GetHashCode());

        //Determine the start position
        int lastHeight = UnityEngine.Random.Range(height - heighRandomize, height);

        //Used to determine which direction to go
        int nextMove;
        //Used to keep track of the current sections width
        int sectionWidth = 0;

        //Work through the array width
        for (int x = 0; x < GetWorldWidth; x++)
        {
            //Determine the next move
            nextMove = rand.Next(2);

            //Only change the height if we have used the current height more than the minimum required section width
            if (nextMove == 0 && lastHeight > 0 && sectionWidth > UnityEngine.Random.Range(3, 6))
            {
                lastHeight--;
                sectionWidth = 0;
            }
            else if (nextMove == 1 && lastHeight < height - 1 && sectionWidth > UnityEngine.Random.Range(3, 6))
            {
                lastHeight++;
                sectionWidth = 0;
            }
            //Increment the section width
            sectionWidth++;

            //Work our way from the height down to 0
            for (int y = lastHeight; y >= 0; y--)
            {
                mapCoords[x * height + y] = (int)BlockTypeConversion.DirtBlock;
            }

            //for (int y = height - 1; y > lastHeight; y--)
            //{
            //    mapCoords[x * height + y] = 0;
            //}
        }
        return mapCoords;
    }



    private NativeArray<int> CreateCaveTerrain(NativeArray<int> mapCoords)
    {
        int worldWidth = GetWorldWidth;
        int worldHeight = GetWorldHeight;

        float seed = UnityEngine.Random.Range(0f, 1f);
        //Seed our random
        System.Random rand = new System.Random(seed.GetHashCode());

        //Define our start x position
        int floorX = UnityEngine.Random.Range(0, (worldWidth) - 1);
        //Define our start y position
        int floorY = UnityEngine.Random.Range(0, (worldHeight) - 1);
        //Determine our required floorAmount
        int reqFloorAmount = (worldHeight * worldWidth) * 20 / 100;
        //Debug.Log(reqFloorAmount);
        //Used for our while loop, when this reaches our reqFloorAmount we will stop tunneling
        int floorCount = 0;
        // Debug.Log(map.GetLength(0) + " width " + map.GetLength(1) + " height");
        //Set our start position to not be a tile (0 = no tile, 1 = tile)
        mapCoords[floorX * worldHeight + floorY] = 1;
        //Increase our floor count
        floorCount++;
        int maxLoops = verticalChunks * horizontalChunks;
        int numberOfLoops = maxLoops;

        for (int i = 0; i < numberOfLoops; i++)
        {
            floorX = UnityEngine.Random.Range(0, (worldWidth) - 1);
            floorY = UnityEngine.Random.Range(0, (worldHeight) - 1);
            maxLoops = numberOfLoops;

            while (floorCount < reqFloorAmount && maxLoops > 0)
            {
                //Determine our next direction
                int randDir = rand.Next(8);

                switch (randDir)
                {
                    case 0: //North-West
                        //Ensure we don't go off the map
                        if ((floorY + 1) < worldHeight && (floorX - 1) > 0)
                        {
                            //Move the y up 
                            floorY++;
                            //Move the x left
                            floorX--;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] == 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = 0;
                                //Increase floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 1: //North
                        //Ensure we don't go off the map
                        if ((floorY + 1) < worldHeight)
                        {
                            //Move the y up
                            floorY++;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] == 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = 0;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 2: //North-East
                        //Ensure we don't go off the map
                        if ((floorY + 1) < worldHeight && (floorX + 1) < worldWidth)
                        {
                            //Move the y up
                            floorY++;
                            //Move the x right
                            floorX++;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] == 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = 0;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 3: //East
                        //Ensure we don't go off the map
                        if ((floorX + 1) < worldWidth)
                        {
                            //Move the x right
                            floorX++;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] == 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = 0;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 4: //South-East
                        //Ensure we don't go off the map
                        if ((floorY - 1) > 0 && (floorX + 1) < worldWidth)
                        {
                            //Move the y down
                            floorY--;
                            //Move the x right
                            floorX++;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] == 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = 0;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 5: //South
                        //Ensure we don't go off the map
                        if ((floorY - 1) > 0)
                        {
                            //Move the y down
                            floorY--;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] == 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = 0;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 6: //South-West
                        //Ensure we don't go off the map
                        if ((floorY - 1) > 0 && (floorX - 1) > 0)
                        {
                            //Move the y down
                            floorY--;
                            //move the x left
                            floorX--;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] == 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = 0;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                    case 7: //West
                        //Ensure we don't go off the map
                        if ((floorX - 1) > 0)
                        {
                            //Move the x left
                            floorX--;

                            //Check if the position is a tile
                            if (mapCoords[floorX * worldHeight + floorY] == 1)
                            {
                                //Change it to not a tile
                                mapCoords[floorX * worldHeight + floorY] = 0;
                                //Increase the floor count
                                floorCount++;
                                maxLoops = numberOfLoops;
                            }
                        }
                        break;
                }

                maxLoops--;
            }
        }
        //int previousPos = 0;

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                if (x * worldHeight + y + 1 < worldHeight * worldWidth && x * worldHeight + y - 1 > 0 && (x + 1) * worldHeight + y < worldHeight * worldWidth && (x - 1) * worldHeight + y > 0)
                {
                    if (mapCoords[x * worldHeight + y + 1] == 0 && mapCoords[x * worldHeight + y - 1] == 0 && mapCoords[(x + 1) * worldHeight + y] == 0 && mapCoords[(x - 1) * worldHeight + y] == 0)
                    {
                        // if (mapCoords[(x + 1) * worldHeight + y + 1] == 0 && mapCoords[(x - 1) * worldHeight + y + 1] == 0 && mapCoords[(x + 1) * worldHeight + y - 1] == 0 && mapCoords[(x - 1) * worldHeight + y - 1] == 0)
                        // {
                        mapCoords[x * worldHeight + y] = 0;
                        // }

                    }
                }
            }

        }

        return mapCoords;
    }

    private int OreModifier(int block)
    {
        BlockTypeConversion modifierConversion = new BlockTypeConversion();
        switch (modifierConversion = (BlockTypeConversion)block)
        {
            case BlockTypeConversion.Empty:
                return 0;
            case BlockTypeConversion.DirtBlock:
                return 0;
            case BlockTypeConversion.StoneBlock:
                return 0;
            case BlockTypeConversion.CopperBlock:
                return copperModifier;
            case BlockTypeConversion.IronBlock:
                return ironModifier;
            case BlockTypeConversion.CoalBlock:
                return coalModifier;
            case BlockTypeConversion.GoldBlock:
                return goldModifier;
            default:
                return 0;
        }
    }

    //private NativeArray<int> GrassGrowth(NativeArray<int> worldArray)
    //{
    //    for (int x = 0; x < width * horizontalChunks; x++)
    //    {
    //        for (int y = (height * verticalChunks) - (height * 2); y < height * verticalChunks; y++)
    //        {
    //            if (worldArray[x * height * verticalChunks + y] == (int)BlockTypeConversion.GrassBlock && worldArray[x * height * verticalChunks + y + 1] == (int)BlockTypeConversion.Empty)
    //            {
    //                int randomizer = UnityEngine.Random.Range(1, 10);
    //                if (randomizer > 5)
    //                {
    //                    worldArray[x * height * verticalChunks + y + 1] = (int)BlockTypeConversion.Plant;
    //                }
    //            }
    //            else if (worldArray[x * height * verticalChunks + y] == (int)BlockTypeConversion.DirtBlock && worldArray[x * height * verticalChunks + y + 1] == (int)BlockTypeConversion.Empty)
    //            {
    //                int randomizer = UnityEngine.Random.Range(1, 10);
    //                if (randomizer > 5)
    //                {
    //                    worldArray[x * height * verticalChunks + y + 1] = (int)BlockTypeConversion.GrassBlock;
    //                }
    //            }
    //        }
    //    }
    //    return worldArray;
    //}
    #region oldcode
    public static int[,] RandomWalkTopSmoothed(int[,] map, float seed, int minSectionWidth, int randomizedRange)
    {
        //Seed our random
        System.Random rand = new System.Random(seed.GetHashCode());

        //Determine the start position
        int lastHeight = UnityEngine.Random.Range(map.GetUpperBound(1) - randomizedRange, map.GetUpperBound(1));

        //Used to determine which direction to go
        int nextMove = 0;
        //Used to keep track of the current sections width
        int sectionWidth = 0;

        //Work through the array width
        for (int x = 0; x <= width; x++)
        {

            //Determine the next move
            nextMove = rand.Next(2);

            //Only change the height if we have used the current height more than the minimum required section width
            if (nextMove == 0 && lastHeight > 0 && sectionWidth > minSectionWidth)
            {
                lastHeight--;
                sectionWidth = 0;
            }
            else if (nextMove == 1 && lastHeight < map.GetUpperBound(1) && sectionWidth > minSectionWidth)
            {
                lastHeight++;
                sectionWidth = 0;
            }
            //Increment the section width
            sectionWidth++;

            //Work our way from the height down to 0
            for (int y = lastHeight; y >= 0; y--)
            {
                map[x, y] = 1;
            }
        }

        //Return the modified map
        return map;
    }


    #endregion
}
