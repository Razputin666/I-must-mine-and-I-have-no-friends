using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using UnityEngine.Events;
using System.Linq;
using Unity.Collections;

public class TileMapManager : NetworkBehaviour
{
    [SerializeField] private List<TileData> tileDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;

    public static TileMapManager Instance { get; private set; }

    public List<Tilemap> Tilemaps 
    { 
        get { return tilemapss.Values.ToList(); }
    }
    private Dictionary<string, Tilemap> tilemapss;

    public NativeArray<int> worldArray;

    //private void Update()
    //{
    //    Debug.Log(Worldgeneration.Instance.GetWorldHeight);
    //}

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(this.gameObject);

        tilemapss = new Dictionary<string, Tilemap>();
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

    private IEnumerator GrassGrowth(NativeArray<int> worldArray)
    {
        
        for (int x = 0; x < Worldgeneration.Instance.GetWorldWidth; x++)
        {
            yield return new WaitForSeconds(0.01f);
            for (int y = Worldgeneration.Instance.GetWorldHeight - (Worldgeneration.Instance.GetHeight * 2); y < Worldgeneration.Instance.GetWorldHeight; y++)
            {
                yield return new WaitForSeconds(0.01f);
                if (worldArray[x * Worldgeneration.Instance.GetWorldHeight + y] == (int)BlockTypeConversion.GrassBlock && worldArray[x * Worldgeneration.Instance.GetWorldHeight + y + 1] == (int)BlockTypeConversion.Empty)
                {
                    int randomizer = UnityEngine.Random.Range(1, 10);
                    if (randomizer > 5)
                    {
                        worldArray[x * Worldgeneration.Instance.GetWorldHeight + y + 1] = (int)BlockTypeConversion.Plant;
                    }
                }
                else if (worldArray[x * Worldgeneration.Instance.GetWorldHeight + y] == (int)BlockTypeConversion.DirtBlock && worldArray[x * Worldgeneration.Instance.GetWorldHeight + y + 1] == (int)BlockTypeConversion.Empty)
                {
                    int randomizer = UnityEngine.Random.Range(1, 10);
                    if (randomizer > 5)
                    {
                        worldArray[x * Worldgeneration.Instance.GetWorldHeight + y + 1] = (int)BlockTypeConversion.GrassBlock;
                    }
                }
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

    [Client]
    public Tilemap GetTilemap(string tilemapName)
    {
        if (tilemapss.TryGetValue(tilemapName, out Tilemap tilemap))
            return tilemap;
        
        return null;

        //foreach (Tilemap tilemap in Tilemaps)
        //{
        //    if (tilemapName == tilemap.name)
        //        return tilemap;
        //}
        //return null;
    }

    public void AddTileChunk(Tilemap tilemap)
    {
        tilemapss.Add(tilemap.name, tilemap);
        //Tilemaps.Add(tilemap);
    }

    public Tilemap GetTileChunk(int index)
    {
        if(index < Tilemaps.Count)
        {
            return Tilemaps[index];
        }
        else
        {
            return null;
        }
    }

    public bool UpdateTilemap(string tilemapName, Vector3Int tilePositionCell, string tileBaseName)
    {
        Tilemap tilemap = GetTilemap(tilemapName);
        if(tilemap != null)
            return tilemap.GetComponent<TilemapSyncer>().UpdateTilemap(tilePositionCell, tileBaseName);
        
        return false;
    }

    [Server]
    public bool UpdateTilemap(Tilemap tilemap, Vector3Int tilePositionCell, TileBase tileBase)
    {
        return tilemap.GetComponent<TilemapSyncer>().UpdateTilemap(tilePositionCell, tileBase);
    }

    [Server]
    public TileBase GetTile(Vector3Int tilePositionWorld)
    {
        foreach (Tilemap tilemap in Tilemaps)
        {
            Vector2 worldPos = new Vector2(tilePositionWorld.x, tilePositionWorld.y);
            BoundsInt bounds = tilemap.cellBounds;
            Vector3 tilemapPos = tilemap.transform.position;
            if (IsInside(tilemapPos, bounds.size, worldPos))
            {
                Vector3Int tilePositionCell = tilemap.WorldToCell(tilePositionWorld);
                return tilemap.GetTile(tilePositionCell);
            } 
        }

        return null;
    }
    private bool IsInside(Vector3 pos, Vector3Int size, Vector2 point)
    {
        if (pos.x <= point.x &&
            point.x <= pos.x + size.x &&
            pos.y <= point.y &&
            point.y <= pos.y + size.y)
        {
            return true;
        }

        return false;
    }
    [Client]
    public void InitTilemaps()
    {
        GameObject grid = GameObject.Find("Grid");

        GameObject[] tilemapObjects = GameObject.FindGameObjectsWithTag("TileMap");

        for (int i = 0; i < tilemapObjects.Length; i++)
        {
            tilemapObjects[i].transform.SetParent(grid.transform, true);
        }
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