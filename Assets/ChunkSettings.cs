using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Tilemaps;
using Unity.Collections;
using Unity.Burst;
using System;
using Mirror;
public class ChunkSettings : NetworkBehaviour
{
    float grassGrowthTimer;
    public Tilemap tileChunk;
    [SerializeField] public TileBase[] tilesChunk;
    [SerializeField] private bool SetJobs;
    private LevelGeneratorLayered mapGen;
    private int width;
    private int height;

    public void Start()
    {
        if(isServer)
        {
            Debug.Log("started");
            mapGen = GameObject.Find("LevelGeneration").GetComponent<LevelGeneratorLayered>();
            width = (int)gameObject.transform.position.x + mapGen.width;
            height = (int)gameObject.transform.position.y + mapGen.height;
            tileChunk = GetComponent<Tilemap>();
            TreeGrowth();

            StartCoroutine(GrassGrowth());
        }
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
                    {
                        Debug.Log("grassBlock");
                        TilemapSyncManager.Instance.UpdateTilemap(tileChunk.name, new Vector3Int(i, j, 0), tilesChunk[0].name);
                    }
                }
                else if (tileChunk.HasTile(new Vector3Int(i, j, 0)) && j > height && !tileChunk.HasTile(new Vector3Int(i, j + 1, 0)) && tileChunk.GetTile(new Vector3Int(i, j, 0)) == tilesChunk[0])
                {
                    int randomizer = UnityEngine.Random.Range(1, 10);
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.01f, 0.2f));
                    if (randomizer > 5)
                    {
                        Debug.Log("plants");
                        TilemapSyncManager.Instance.UpdateTilemap(tileChunk.name, new Vector3Int(i, j + 1, 0), tilesChunk[2].name);
                    }   
                }
            }
        }  
    }

    public void TreeGrowth()
    {
        for (int i = 0; i < width; i++)
        {
            int randomizer = UnityEngine.Random.Range(1, 100);
            if (randomizer > 95)
            {
                for (int j = height; j < height * 2; j++)
                {
                    // yield return new WaitForSeconds(0.02f);
                    if (tileChunk.HasTile(new Vector3Int(i, j, 0)) && j > height && !tileChunk.HasTile(new Vector3Int(i, j + 1, 0)))
                    {
                        int randomValue = UnityEngine.Random.Range(1, 6);
                        
                        for (int k = 1; k < randomValue; k++)
                        {
                            if (!tileChunk.HasTile(new Vector3Int(i, j + 1, 0)))
                            {
                                Debug.Log("trees");
                                TilemapSyncManager.Instance.UpdateTilemap(tileChunk.name, new Vector3Int(i, j + k, 0), tilesChunk[1].name);
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