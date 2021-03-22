using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Tilemaps;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using Unity.Collections;
public class TilemapSyncer : NetworkBehaviour
{
    private Tilemap tilemap;
    private TileBase[] tileAssets;

    private GameTiles gametiles;
    private NetworkTransmitter networkTransmitter;

    private List<TileUpdateData> tileUpdateData;

    public static event EventHandler<Vector3> OnTileMapUpdated;

    [SyncVar(hook = nameof(OnNameChanged))]
    private string tilemapName = "";
    private struct TileUpdateData
    {
        public TileUpdateData(Vector3Int blockPos, string newTilebaseName)
        {
            blockCellPos = blockPos;
            tilebaseName = newTilebaseName;
        }
        public Vector3Int blockCellPos;
        public string tilebaseName;
    }

    public bool hasTilemap;

    private void Awake()
    {
        if (!gametiles)
            gametiles = GetComponent<GameTiles>();

        if (!networkTransmitter)
            networkTransmitter = GetComponent<NetworkTransmitter>();

        if (!tilemap)
            tilemap = GetComponent<Tilemap>();

        tileAssets = Resources.LoadAll<TileBase>("Tilebase");
    }

    public override void OnStartClient()
    {

        if (isServer)
            return;

        hasTilemap = false;

        tileUpdateData = new List<TileUpdateData>();

        TileMapManager.Instance.AddTileChunk(GetComponent<Tilemap>());
        
    }

    private void OnNameChanged(string _Old, string _New)
    {
        gameObject.name = _New;
    }

    [Server]
    public void SetName(string name)
    {
        tilemapName = name;
    }

    private void FrequentTilemapUpdate()
    {
        //int index = Worldgeneration.Instance.GetHeight * Worldgeneration.Instance.GetWidth;
        //int[] chunkArray = new int[index];
        //int[] test = TileMapManager.Instance.worldArray.ToArray();
        //Array.Copy(test, int.Parse(gameObject.name.Substring(6)) * index, chunkArray, 0 , index);

        //int xThreshold = index * Worldgeneration.Instance.GetVerticalChunks;
        //int x = int.Parse(gameObject.name.Substring(6)) * index / xThreshold;

       // Buffer.BlockCopy(chunkArray, 0, chunkArray, 0, index);


    }

    //public IEnumerator GrassGrowth()
    //{
    //    for (int i = 0; i < Worldgeneration.Instance.GetWidth; i++)
    //    {
    //        yield return new WaitForSeconds(0.01f);
    //        for (int j = Worldgeneration.Instance.GetHeight; j < Worldgeneration.Instance.GetHeight * 2; j++)
    //        {
    //            // yield return new WaitForSeconds(0.02f);
    //            if (tilemap.HasTile(new Vector3Int(i, j, 0)) && j > Worldgeneration.Instance.GetHeight && !tilemap.HasTile(new Vector3Int(i, j + 1, 0)) && tilemap.GetTile(new Vector3Int(i, j, 0)) == tileAssets[(int)BlockTypeConversion.GrassBlock])
    //            {
    //                int randomizer = UnityEngine.Random.Range(1, 10);
    //                yield return new WaitForSeconds(UnityEngine.Random.Range(0.01f, 0.2f));
    //                if (randomizer > 5)
    //                {
    //                    UpdateTilemap(new Vector3Int(i, j, 0), BlockTypeConversion.Plant.ToString());
    //                }
    //            }
    //            else if (tilemap.HasTile(new Vector3Int(i, j, 0)) && j > Worldgeneration.Instance.GetHeight && !tilemap.HasTile(new Vector3Int(i, j + 1, 0)) && tilemap.GetTile(new Vector3Int(i, j, 0)) == tileAssets[(int)BlockTypeConversion.Plant])
    //            {
    //                int randomizer = UnityEngine.Random.Range(1, 10);
    //                yield return new WaitForSeconds(UnityEngine.Random.Range(0.01f, 0.2f));
    //                if (randomizer > 5)
    //                {
    //                    UpdateTilemap(new Vector3Int(i, j + 1, 0), BlockTypeConversion.GrassBlock.ToString());
    //                }
    //            }
    //        }
    //    }
    //}

    public bool UpdateTilemap(Vector3Int tilePositionCell, string tilebaseName)
    {
        Vector3Int worldTile = Vector3Int.FloorToInt(tilemap.CellToWorld(tilePositionCell));
        if (tilebaseName == string.Empty) 
        {

            if (isServer)
            {
                //Vector3 tileWorldPos = tilemap.CellToWorld(tilePositionCell);
                RpcUpdateTilemap(tilePositionCell, tilebaseName);
                TileMapManager.Instance.nativeWorldArray[worldTile.x * Worldgeneration.Instance.GetWorldHeight + worldTile.y] = (int)BlockTypeConversion.Empty;
                // TileMapManager.Instance.worldArray[(int)tileWorldPos.x * Worldgeneration.Instance.GetWorldHeight + (int)tileWorldPos.y] = (int)BlockTypeConversion.Empty;
                //TileMapManager.Instance.worldArray[worldTile.x, worldTile.y] = (int)BlockTypeConversion.Empty;
            }




            SetTile(tilePositionCell, null);

            return true;
        }
        else
        {
            
            foreach (TileBase tileAsset in tileAssets)
            {
                if (tileAsset.name == tilebaseName)
                {
                    if (isServer)
                    {
                        BlockTypeConversion block = (BlockTypeConversion)Enum.Parse(typeof(BlockTypeConversion), tileAsset.name);
                        TileMapManager.Instance.nativeWorldArray[worldTile.x * Worldgeneration.Instance.GetWorldHeight + worldTile.y] = (int)block;
                        // TileMapManager.Instance.worldArray[tilePositionCell.x * Worldgeneration.Instance.GetWorldHeight + tilePositionCell.y] = (int)block;
                        //TileMapManager.Instance.worldArray[worldTile.x, worldTile.y] = (int)block; T
                        
                        //Tror inte worldArray har nån funktion?
                    }

                    
                    return UpdateTilemap(tilePositionCell, tileAsset);
                }
            }
        }
        return false;
    }

    public bool UpdateTilemap(Vector3Int tilePositionCell, TileBase tilebase)
    {
        if (isServer)
        {
            RpcUpdateTilemap(tilePositionCell, tilebase.name);
        }

        SetTile(tilePositionCell, tilebase);

        return true;
    }

    private void SetTile(int tilePositionCellX, int tilePositionCellY , int tilebase)
    {
       // tilemap.SetTile(new Vector3Int(tilePositionCellX, tilePositionCellY, 0), tilebase);
    }

    private void SetTile(Vector3Int tilePositionCell, TileBase tilebase)
    {
        //if (isServer)
        //{
        //    bool mineable = tilebase == null ? false : true;

        //    Pathfinding.Instance.UpdateGridMineable(tilemap.CellToWorld(tilePositionCell), mineable);
        //}

        OnTileMapUpdated?.Invoke(this, tilemap.CellToWorld(tilePositionCell));

        tilemap.SetTile(tilePositionCell, tilebase);
    }
    #region Client
    [Client]
    public void SetTileData()
    {
        foreach (TileUpdateData data in tileUpdateData)
        {
            if (data.tilebaseName == string.Empty)
            {
                tilemap.SetTile(data.blockCellPos, null);
            }
            else
            {
                foreach (TileBase tileAsset in tileAssets)
                {
                    if (tileAsset.name == data.tilebaseName)
                    {
                        tilemap.SetTile(data.blockCellPos, tileAsset);
                    }
                }
            }
        }
        tileUpdateData.Clear();
    }

    [ClientRpc]
    private void RpcUpdateTilemap(Vector3Int tilePositionCell, string tilebaseName)
    {
        if (isServer)
            return;

        if (!hasTilemap)
        {
            Debug.Log("Adding tilemap to list");
            tileUpdateData.Add(new TileUpdateData(tilePositionCell, tilebaseName));
        }
        else
        {
            Debug.Log("Updating tilemap");
            UpdateTilemap(tilePositionCell, tilebaseName);
        }
    }
    #endregion
}