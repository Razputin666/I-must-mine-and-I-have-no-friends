using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Tilemaps;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
public class TilemapSyncer : NetworkBehaviour
{
    private Tilemap tilemap;
    private TileBase[] tileAssets;

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

    public bool UpdateTilemap(Vector3Int tilePositionCell, string tilebaseName)
    {
        if (tilebaseName == string.Empty)
        {
            if (isServer)
                RpcUpdateTilemap(tilePositionCell, tilebaseName);

            SetTile(tilePositionCell, null);

            return true;
        }
        else
        {
            foreach (TileBase tileAsset in tileAssets)
            {
                if (tileAsset.name == tilebaseName)
                {
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

    private void SetTile(Vector3Int tilePositionCell, TileBase tilebase)
    {
        //if(isServer)
        //{
        //    bool mineable = tilebase == null ? false : true;
        //    Debug.Log(mineable);
        //    Pathfinding.Instance.UpdateGridMineable(tilemap.CellToWorld(tilePositionCell), mineable);
        //}
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