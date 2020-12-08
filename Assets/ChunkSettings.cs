using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Tilemaps;
using Unity.Collections;
using Unity.Burst;
using System;

public class ChunkSettings : MonoBehaviour
{
    float grassGrowthTimer;
    public Tilemap tileChunk;
    [SerializeField] public TileBase[] tilesChunk;
    [SerializeField] private bool SetJobs;
    private LevelGeneratorLayered mapGen;
    private int width;
    private int height;

    private void Start()
    {
        mapGen = GameObject.Find("LevelGeneration").GetComponent<LevelGeneratorLayered>();
        width = (int)gameObject.transform.position.x + mapGen.width;
        height = (int)gameObject.transform.position.y + mapGen.height;
        tileChunk = GetComponent<Tilemap>();
        TreeGrowth();
        Debug.Log(width + " width " + height + " height");

        StartCoroutine(GrassGrowth());

    }

    public IEnumerator GrassGrowth()
    {
        for (int i = 0; i < width; i++)
        {
            yield return new WaitForSeconds(0.01f);
            for (int j = height; j < height * 2; j++)
            {
               // yield return new WaitForSeconds(0.02f);
                if (tileChunk.HasTile(new Vector3Int(i, j, 0)) && j > height && !tileChunk.HasTile(new Vector3Int(i,j + 1, 0)) && tileChunk.GetTile(new Vector3Int(i, j, 0)) == tilesChunk[3])
                {
                    int randomizer = UnityEngine.Random.Range(1, 10);
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.01f, 0.2f));
                    if (randomizer > 5)
                        tileChunk.SetTile(new Vector3Int(i, j, 0), tilesChunk[0]);

                }
                else if (tileChunk.HasTile(new Vector3Int(i, j, 0)) && j > height && !tileChunk.HasTile(new Vector3Int(i, j + 1, 0)) && tileChunk.GetTile(new Vector3Int(i, j, 0)) == tilesChunk[0])
                {
                    int randomizer = UnityEngine.Random.Range(1, 10);
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.01f, 0.2f));
                    if (randomizer > 5)
                        tileChunk.SetTile(new Vector3Int(i, j + 1, 0), tilesChunk[2]);
                }
                


            }
        }  
    }

    public void TreeGrowth()
    {
        {

        
            for (int i = 0; i < width; i++)
            {
                int randomizer = UnityEngine.Random.Range(1, 100);
                if (randomizer > 95)
                    for (int j = height; j < height * 2; j++)
                {
                    // yield return new WaitForSeconds(0.02f);
                    if (tileChunk.HasTile(new Vector3Int(i, j, 0)) && j > height && !tileChunk.HasTile(new Vector3Int(i, j + 1, 0)))
                    {
                        int randomValue = UnityEngine.Random.Range(1, 6);
                           
                        for (int k = 1; k < randomValue; k++)
                        {
                                if(!tileChunk.HasTile(new Vector3Int(i, j + 1, 0)))
                                {
                                    tileChunk.SetTile(new Vector3Int(i, j + k, 0), tilesChunk[1]);
                                }
                                
                        }
                        
                    
                    }


                }
            }
        }
    }

    private void Update()
    {
       
        grassGrowthTimer += Time.deltaTime;
        
        if(grassGrowthTimer > 30)
        {
           StartCoroutine(GrassGrowth());
            grassGrowthTimer = 0f;
        }
    }


    //private JobHandle CheckChunks(int i)
    //{
    //    CheckChunk chunkCheck = new CheckChunk
    //    {
    //        width = i,
    //        height = 250,


    //    };
    //    return chunkCheck.Schedule();

    //}
}
//[BurstCompile]
//public struct CheckChunk : IJob
//{

//    public int width;
//    public int height;
//    //public TileBase[] tiles;

//    public void Execute()
//    {


//        for (int j = 0; j < height; j++)
//        {

//        }

//    }
//}