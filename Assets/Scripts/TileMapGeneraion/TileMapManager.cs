using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using UnityEngine.Events;

public class TileMapManager : NetworkBehaviour
{
    [SerializeField] private List<TileData> tileDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;

    public static TileMapManager Instance { get; private set; }

    public List<Tilemap> Tilemaps { get; private set; }

    public event UnityAction<int> OnTilemapsSynced;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(this.gameObject);

        Tilemaps = new List<Tilemap>();
    }

    public override void OnStartServer()
    {
        StartCoroutine(TileChecker());
    }

    private IEnumerator TileChecker()
    {
        yield return null;

        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var tileData in tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
        }
    }

    public float GetBlockStrength(Vector3Int target, Tilemap tilemap)
    {
        target = new Vector3Int(target.x, target.y, 0);
        TileBase targetedBlock = tilemap.GetTile(target);
        if(!targetedBlock)
        {
            return -1;
        }
        TileData check;
        if(dataFromTiles.TryGetValue(targetedBlock, out check))
        {
            return dataFromTiles[targetedBlock].blockStrength;
        }
        return -1;
    }

    public string GetBlockName(Vector3Int target, Tilemap tilemap)
    {
        target = new Vector3Int(target.x, target.y, 0);
        TileBase targetedBlock = tilemap.GetTile(target);
        
        if (!targetedBlock)
        {
            return "";
        }
        TileData check;
        if (dataFromTiles.TryGetValue(targetedBlock, out check))
        {
            return dataFromTiles[targetedBlock].blockName;
        }
        return "";
    }

    public string GetBlockType(Vector3Int target, Tilemap tilemap)
    {
        target = new Vector3Int(target.x, target.y, 0);
        TileBase targetedBlock = tilemap.GetTile(target);

        if (!targetedBlock)
        {
            return "";
        }
        TileData check;
        if (dataFromTiles.TryGetValue(targetedBlock, out check))
        {
            return dataFromTiles[targetedBlock].blockType;
        }
        return "";
    }

    public void AddTileChunk(Tilemap tilemap)
    {
        Tilemaps.Add(tilemap);
    }

    public bool UpdateTilemap(string tilemapName, Vector3Int tilePositionCell, string tileBaseName)
    {
        foreach (Tilemap tilemapChunk in Tilemaps)
        {
            if (tilemapChunk.name == tilemapName)
            {
                return tilemapChunk.GetComponent<TilemapSyncer>().UpdateTilemap(tilePositionCell, tileBaseName);
            }
        }
        return false;
    }
    [Server]
    public bool UpdateTilemap(Tilemap tilemap, Vector3Int tilePositionCell, TileBase tileBase)
    {
        return tilemap.GetComponent<TilemapSyncer>().UpdateTilemap(tilePositionCell, tileBase);
    }

    [Client]
    public void InitTilemaps()
    {
        GameObject grid = GameObject.Find("Grid");

        GameObject[] tilemapObjects = GameObject.FindGameObjectsWithTag("TileMap");

        for (int i = 0; i < tilemapObjects.Length; i++)
        {
            tilemapObjects[i].transform.parent = grid.transform;
            //Debug.Log(tilemapObjects[i]);
            //AddTileChunk(tilemapObjects[i].GetComponent<Tilemap>());
        }
    }

    [Client]
    public Tilemap GetTilemap(string tilemapName)
    {
        foreach (Tilemap tilemap in Tilemaps)
        {
            if (tilemapName == tilemap.name)
                return tilemap;
        }
        return null;
    }

    [Client]
    public void SyncComplete()
    {
        foreach (Tilemap tilemap in Tilemaps)
        {
            TilemapSyncer ts = tilemap.GetComponent<TilemapSyncer>();
            ts.hasTilemap = true;
            ts.SetTileData();
        }
    }
}