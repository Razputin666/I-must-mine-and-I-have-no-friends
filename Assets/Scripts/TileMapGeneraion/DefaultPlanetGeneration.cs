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
        
        NativeArray<int> chunkArray = new NativeArray<int>(horizontalChunks * width * verticalChunks * height, Allocator.TempJob);
        NativeArray<int> topLayer = new NativeArray<int>(horizontalChunks * width * height, Allocator.TempJob);
        NativeArray<int>[] chunks = new NativeArray<int>[horizontalChunks * verticalChunks];

        GenerateJobs(chunkArray);

        for (int x = 0; x < horizontalChunks * width; x++)
        {

            for (int y = 0; y < height; y++)
            {
                int topLayerIndex = x * height + y;
                int index = x * verticalChunks * height + y + (verticalChunks - 1) * height;
                topLayer[topLayerIndex] = chunkArray[index];
            }
        }

        topLayer = CreateTopTerrain(topLayer);

        for (int x = 0; x < horizontalChunks * width; x++)
        {

            for (int y = 0; y < height; y++)
            {
                int topLayerIndex = x * height + y;
                int index = x * verticalChunks * height + y + (verticalChunks - 1) * height;
               // int index = (verticalChunks - 1) * height + topLayerIndex;
                
                // index += verticalChunks * height * (x - 1);
                
               // topLayer[topLayerIndex] = chunkArray[index];
                chunkArray[index] = topLayer[topLayerIndex]; 
            }
        }

        //for (int i = 0; i < chunks.Length; i++)
        //{
        //    chunks[i] = new NativeArray<int>(width * height, Allocator.TempJob);
        //    for (int x = 0; x < width; x++)
        //    {

        //        for (int y = 0; y < height; y++)
        //        {
                        
        //            int chunkIndex = x * height + y;
        //           // int index = i * height + (verticalChunks * height) * x + chunkIndex;
        //            int index = x * verticalChunks * height + y + (i % verticalChunks) * height;
        //            chunks[i][chunkIndex] = chunkArray[index];
        //        }
        //    }
        //}
        for (int i = 0; i < verticalChunks; i++)
        {
            for (int j = 0; j < horizontalChunks; j++)
            {
                int currentChunkIndex = j * verticalChunks + i;
                chunks[currentChunkIndex] = new NativeArray<int>(width * height, Allocator.TempJob);
                for (int x = 0; x < width; x++)
                {

                    for (int y = 0; y < height; y++)
                    {

                        int chunkIndexPos = y * width + x;
                        // int index = i * height + (verticalChunks * height) * x + chunkIndex;
                        int index = x * verticalChunks * height + y + i  * height + j * verticalChunks * height * width;
                        chunks[currentChunkIndex][chunkIndexPos] = chunkArray[index];
                    }
                }
            }
        }

        Render(chunks);
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Dispose();
        }
        topLayer.Dispose();
        chunkArray.Dispose();


    }

    //public override void OnStartServer()
    //{
    //    Init();
    //    NativeArray<int>[] chunkArray = new NativeArray<int>[horizontalChunks * verticalChunks];
    //    //  NativeArray<int> toplayer = new NativeArray<int>(horizontalChunks * width * height, Allocator.Temp);
    //    NativeArray<int>[] toplayer = new NativeArray<int>[1];
    //    NativeArray<int>[] toplayerChunks = new NativeArray<int>[horizontalChunks];
    //    toplayer[0] = new NativeArray<int>(horizontalChunks * width * height, Allocator.TempJob);
    //    for (int i = 0; i < chunkArray.Length; i++)
    //    {
    //        chunkArray[i] = new NativeArray<int>(width * height, Allocator.TempJob);
    //    }
    //    GenerateJobs(chunkArray);
    //    //GenerateJobs(toplayer);
    //    toplayer[0] = CreateTopTerrain(toplayer[0]);

    //    for (int i = 0; i < horizontalChunks; i++)
    //    {
    //        int index = verticalChunks - 1 + verticalChunks * i;
    //      //  chunkArray[index] = new NativeArray<int>(width * height, Allocator.Temp);
    //       // toplayerChunks[i] = new NativeSlice<int>(toplayer[0], toplayer[0].Length / horizontalChunks * i, toplayer[0].Length / horizontalChunks * i + 1);
    //        toplayerChunks[i] = new NativeArray<int>((toplayer[0].Length / horizontalChunks * (i + 1)) - (toplayer[0].Length / horizontalChunks * i), Allocator.TempJob);
    //        chunkArray[index] = toplayerChunks[i];

    //    }

    //    Render(chunkArray);
    //}

    private NativeArray<int> CreateTopTerrain(NativeArray<int> mapCoords)
    {
        int heighRandomize = UnityEngine.Random.Range(2, 7);
        float seed = UnityEngine.Random.Range(0f, 1f);
        //Seed our random
        System.Random rand = new System.Random(seed.GetHashCode());

        //Determine the start position
        int lastHeight = Random.Range(height - heighRandomize, height);

        //Used to determine which direction to go
        int nextMove = 0;
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
            //for (int y = lastHeight; y > 0; y--)
            //{
            //    mapCoords[x * height + y] = 1;

            //}

            for (int y = height - 1; y > lastHeight; y--)
            {
                mapCoords[x * height + y] = 0;
            }
        }
        return mapCoords;
    }


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
}
