using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class TilemapSyncManager : NetworkBehaviour
{
    public static TilemapSyncManager Instance { get; private set; }

    public List<Tilemap> Tilemaps { get; private set; }

    private TileBase[] tileAssets;

    private struct TileUpdateData
    {
        public TileUpdateData(Vector3Int blockPos, string newTilemapName, string newTilebaseName)
        {
            blockCellPos = blockPos;
            tilemapName = newTilemapName;
            tilebaseName = newTilebaseName;
        }
        public Vector3Int blockCellPos;
        public string tilemapName;
        public string tilebaseName;
    }

    private List<TileUpdateData> tileUpdateData = new List<TileUpdateData>();

    public bool hasTilemaps;

    public bool HasTilemaps
    { 
        get { return hasTilemaps; } 
        set 
        { 
            hasTilemaps = value; 
            if(hasTilemaps)
                SetTileData(); 
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(this.gameObject);

        Tilemaps = new List<Tilemap>();
        
        tileAssets = Resources.LoadAll<TileBase>("Tilebase");
    }

    public void AddTileChunk(Tilemap tilemap)
    {
        Tilemaps.Add(tilemap);
    }

    public bool UpdateTilemap(string tilemapName, Vector3Int blockPositionCell, string tileBaseName)
    {
        foreach (Tilemap tilemapChunk in Tilemaps)
        {
            if(tilemapChunk.name == tilemapName)
            {
                if(tileBaseName == string.Empty)
                {
                    tilemapChunk.SetTile(blockPositionCell, null);

                    if (isServer)
                        RpcUpdateTilemap(tilemapName, blockPositionCell, tileBaseName);

                    return true;
                }
                else
                {
                    foreach (TileBase tileAsset in tileAssets)
                    {
                        if(tileAsset.name == tileBaseName)
                        {
                            tilemapChunk.SetTile(blockPositionCell, tileAsset);

                            if (isServer)
                                RpcUpdateTilemap(tilemapName, blockPositionCell, tileBaseName);

                            return true;
                        }
                    }
                }
                return false;
            }
        }
        return false;
    }

    [ClientRpc]
    private void RpcUpdateTilemap(string tilemapName, Vector3Int blockPositionCell, string tilebaseName)
    {
        if (isServer)
            return;

        if(!hasTilemaps)
        {
            Debug.Log("Adding tilemap to list");
            tileUpdateData.Add(new TileUpdateData(blockPositionCell, tilemapName, tilebaseName));
        }
        else
        {
            Debug.Log("Updating tilemap");
            UpdateTilemap(tilemapName, blockPositionCell, tilebaseName);
        }
    }

    private void SetTileData()
    {
        foreach (TileUpdateData data in tileUpdateData)
        {
            foreach (Tilemap tilemapChunk in Tilemaps)
            {
                if (tilemapChunk.name == data.tilemapName)
                {
                    if (data.tilebaseName == string.Empty)
                    {
                        tilemapChunk.SetTile(data.blockCellPos, null);
                    }
                    else
                    {
                        foreach (TileBase tileAsset in tileAssets)
                        {
                            if (tileAsset.name == data.tilebaseName)
                            {
                                tilemapChunk.SetTile(data.blockCellPos, tileAsset);
                            }
                        }
                    }
                }
            }
        }
        tileUpdateData.Clear();
    }
}