using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using Unity.Jobs;
using Unity.Collections;

public class DefaultPlanetGeneration : Worldgeneration
{
    //public override void OnStartServer()
    //{
    //    Init();
    //    NativeArray<int>[] chunkArray = new NativeArray<int>[horizontalChunks * verticalChunks];
    //    for (int i = 0; i < chunkArray.Length; i++)
    //    {
    //        chunkArray[i] = new NativeArray<int>(width * height, Allocator.TempJob);
    //    }
    //    GenerateJobs(chunkArray);
    //    for (int i = 0; i < horizontalChunks; i++)
    //    {
    //        // int index = i * horizontalChunks + verticalChunks - 1;
    //        int index = (verticalChunks - 1) + verticalChunks * i;
    //        chunkArray[index] = CreateTopTerrain(chunkArray[index]);
    //    }
    //    Render(chunkArray);



    //}

    public override void OnStartServer()
    {
        Init();
        
        NativeArray<int> worldArray = new NativeArray<int>(horizontalChunks * width * verticalChunks * height, Allocator.TempJob);
        NativeArray<int> topLayer = new NativeArray<int>(horizontalChunks * width * height, Allocator.TempJob);
        NativeArray<int>[] chunkArray = new NativeArray<int>[horizontalChunks * verticalChunks];
        NativeArray<int> cavesArray = new NativeArray<int>(horizontalChunks * width * verticalChunks * height, Allocator.TempJob);

        GenerateJobs(worldArray);
        GenerateJobs(cavesArray);
        CreateTopTerrain(topLayer);
        
        //Copy values from the array with the topWalk values to the array with the entire world
        for (int x = 0; x < (horizontalChunks * width); x++)
        {
            for (int y = 0; y < height; y++)
            {
                int topLayerIndex = x * height + y;
                int index = x * verticalChunks * height + y + (verticalChunks - 1) * height;
                worldArray[index] = topLayer[topLayerIndex]; 
            }
        }
        CreateCaveTerrain(cavesArray);
        for (int x = 0; x < width * horizontalChunks; x++)
        {
            for (int y = 0; y < height * verticalChunks; y++)
            {
                int cavesIndex = x * height + y;
                worldArray[cavesIndex] = cavesArray[cavesIndex];
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
                        int index = x * verticalChunks * height + y + j * height + i * verticalChunks * height * width;
                        //Copy Tile from the world array to the chunkArray
                        currentChunk[chunkIndexPos] = worldArray[index];
                    }
                }
            }
        }



        Render(chunkArray);

        //Cleanup
        for (int i = 0; i < chunkArray.Length; i++)
        {
            chunkArray[i].Dispose();
        }
        topLayer.Dispose();
        worldArray.Dispose();
    }


    private NativeArray<int> CreateTopTerrain(NativeArray<int> mapCoords)
    {
        int heighRandomize = UnityEngine.Random.Range(2, 7);
        float seed = UnityEngine.Random.Range(0f, 1f);
        //Seed our random
        System.Random rand = new System.Random(seed.GetHashCode());

        //Determine the start position
        int lastHeight = Random.Range(height - heighRandomize, height);

        //Used to determine which direction to go
        int nextMove;
        //Used to keep track of the current sections width
        int sectionWidth = 0;

        //Work through the array width
        for (int x = 0; x < horizontalChunks * width; x++)
        {
            //Determine the next move
            nextMove = rand.Next(2);

            //Only change the height if we have used the current height more than the minimum required section width
            if (nextMove == 0 && lastHeight > 0 && sectionWidth > Random.Range(3,6))
            {
                lastHeight--;
                sectionWidth = 0;
            }
            else if (nextMove == 1 && lastHeight < height - 1 && sectionWidth > Random.Range(3, 6))
            {
                lastHeight++;
                sectionWidth = 0;
            }
            //Increment the section width
            sectionWidth++;

            //Work our way from the height down to 0
            for (int y = lastHeight; y >= 0; y--)
            {
                mapCoords[x * height + y] = 1;
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

        float seed = UnityEngine.Random.Range(0f, 1f);
        //Seed our random
        System.Random rand = new System.Random(seed.GetHashCode());

        //Define our start x position
        int floorX = Random.Range(0, (horizontalChunks * width) - 1);
        //Define our start y position
        int floorY = Random.Range(0, (verticalChunks * height) - 1);
        //Determine our required floorAmount
        int reqFloorAmount = (verticalChunks * height * horizontalChunks * width) / 10;
        Debug.Log(reqFloorAmount);
        //Used for our while loop, when this reaches our reqFloorAmount we will stop tunneling
        int floorCount = 0;
        // Debug.Log(map.GetLength(0) + " width " + map.GetLength(1) + " height");
        //Set our start position to not be a tile (0 = no tile, 1 = tile)
        mapCoords[floorX * height + floorY] = 1;
        //Increase our floor count
        floorCount++;
        int maxLoops = verticalChunks + horizontalChunks;
        int numberOfLoops = maxLoops;
        while (floorCount < reqFloorAmount && maxLoops > 0)
        {
            //Determine our next direction
            int randDir = rand.Next(8);
            floorX = Random.Range(0, (horizontalChunks * width) - 1);
            floorY = Random.Range(0, (verticalChunks * height) - 1);

            switch (randDir)
            {
                case 0: //North-West
                    //Ensure we don't go off the map
                    if ((floorY + 1) < verticalChunks * height && (floorX - 1) > 0)
                    {
                        //Move the y up 
                        floorY++;
                        //Move the x left
                        floorX--;

                        //Check if the position is a tile
                        if (mapCoords[floorX * height + floorY] == 1)
                        {
                            //Change it to not a tile
                            mapCoords[floorX * height + floorY] = 0;
                            //Increase floor count
                            floorCount++;
                            maxLoops = numberOfLoops;
                        }
                    }
                    break;
                case 1: //North
                    //Ensure we don't go off the map
                    if ((floorY + 1) < verticalChunks * height)
                    {
                        //Move the y up
                        floorY++;

                        //Check if the position is a tile
                        if (mapCoords[floorX * height + floorY] == 1)
                        {
                            //Change it to not a tile
                            mapCoords[floorX * height + floorY] = 0;
                            //Increase the floor count
                            floorCount++;
                            maxLoops = numberOfLoops;
                        }
                    }
                    break;
                case 2: //North-East
                    //Ensure we don't go off the map
                    if ((floorY + 1) < verticalChunks * height && (floorX + 1) < horizontalChunks * width)
                    {
                        //Move the y up
                        floorY++;
                        //Move the x right
                        floorX++;

                        //Check if the position is a tile
                        if (mapCoords[floorX * height + floorY] == 1)
                        {
                            //Change it to not a tile
                            mapCoords[floorX * height + floorY] = 0;
                            //Increase the floor count
                            floorCount++;
                            maxLoops = numberOfLoops;
                        }
                    }
                    break;
                case 3: //East
                    //Ensure we don't go off the map
                    if ((floorX + 1) < horizontalChunks * width)
                    {
                        //Move the x right
                        floorX++;

                        //Check if the position is a tile
                        if (mapCoords[floorX * height + floorY] == 1)
                        {
                            //Change it to not a tile
                            mapCoords[floorX * height + floorY] = 0;
                            //Increase the floor count
                            floorCount++;
                            maxLoops = numberOfLoops;
                        }
                    }
                    break;
                case 4: //South-East
                    //Ensure we don't go off the map
                    if ((floorY - 1) > 0 && (floorX + 1) < horizontalChunks * width)
                    {
                        //Move the y down
                        floorY--;
                        //Move the x right
                        floorX++;

                        //Check if the position is a tile
                        if (mapCoords[floorX * height + floorY] == 1)
                        {
                            //Change it to not a tile
                            mapCoords[floorX * height + floorY] = 0;
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
                        if (mapCoords[floorX * height + floorY] == 1)
                        {
                            //Change it to not a tile
                            mapCoords[floorX * height + floorY] = 0;
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
                        if (mapCoords[floorX * height + floorY] == 1)
                        {
                            //Change it to not a tile
                            mapCoords[floorX * height + floorY] = 0;
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
                        if (mapCoords[floorX * height + floorY] == 1)
                        {
                            //Change it to not a tile
                            mapCoords[floorX * height + floorY] = 0;
                            //Increase the floor count
                            floorCount++;
                            maxLoops = numberOfLoops;
                        }
                    }
                    break;
            }
            maxLoops--;
        }

        return mapCoords;
    }

    #region oldcode
    public static int[,] RandomWalkTopSmoothed(int[,] map, float seed, int minSectionWidth, int randomizedRange)
    {
        //Seed our random
        System.Random rand = new System.Random(seed.GetHashCode());

        //Determine the start position
        int lastHeight = Random.Range(map.GetUpperBound(1) - randomizedRange, map.GetUpperBound(1));

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
