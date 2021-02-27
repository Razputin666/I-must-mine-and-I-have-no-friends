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
    private Dictionary<string, TileBase> tilebaseLookup;
    private GameTiles gametiles;
    private NetworkTransmitter networkTransmitter;

    private List<TileUpdateData> tileUpdateData;

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

        tilebaseLookup = new Dictionary<string, TileBase>();
        foreach (var tilebase in tileAssets)
        {
            tilebaseLookup.Add(tilebase.name, tilebase);
        }
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
        int index = Worldgeneration.Instance.GetHeight * Worldgeneration.Instance.GetWidth;
        int[] chunkArray = new int[index];
        int[] test = TileMapManager.Instance.worldArray.ToArray();
        Array.Copy(test, int.Parse(gameObject.name.Substring(6)) * index, chunkArray, 0 , index);

        Buffer.BlockCopy(chunkArray, 0, chunkArray, 0, index);


    }

    public bool UpdateTilemap(Vector3Int tilePositionCell, string tilebaseName)
    {
        if (tilebaseName == string.Empty)
        {
            if (isServer)
                RpcUpdateTilemap(tilePositionCell, tilebaseName);

            Vector3 tileWorldPos = tilemap.CellToWorld(tilePositionCell);
            TileMapManager.Instance.worldArray[(int)tileWorldPos.x * Worldgeneration.Instance.GetWorldHeight + (int)tileWorldPos.y] = (int)BlockTypeConversion.Empty;
            SetTile(tilePositionCell, null);

            return true;
        }
        else
        {
            tilebaseLookup.TryGetValue(tilebaseName, out TileBase tileAsset);

            if (tileAsset != null)
            {
                BlockTypeConversion block = (BlockTypeConversion)Enum.Parse(typeof(BlockTypeConversion), tileAsset.name);
                TileMapManager.Instance.worldArray[tilePositionCell.x * Worldgeneration.Instance.GetWorldHeight + tilePositionCell.y] = (int)block;
                return UpdateTilemap(tilePositionCell, tileAsset);
            }
            else
                Debug.LogError("Couldn't find tilebase, Missing in Resource folder");
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
        if (isServer)
        {
            bool mineable = tilebase == null ? false : true;
            
            PathfindingDots.Instance.UpdateGridMineable(tilemap.CellToWorld(tilePositionCell), mineable);
        }
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
                tilebaseLookup.TryGetValue(data.tilebaseName, out TileBase tileAsset);

                if (tileAsset != null)
                    tilemap.SetTile(data.blockCellPos, tileAsset);
                else
                    Debug.LogError("Couldn't find tilebase, Missing in Resource folder");
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