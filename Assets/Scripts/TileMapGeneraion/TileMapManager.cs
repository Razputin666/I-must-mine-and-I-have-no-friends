using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class TileMapManager : NetworkBehaviour
{
    [SerializeField] private List<Tilemap> chunks; //Only serialized for testing

    [SerializeField] private List<TileData> tileDatas;

    [SerializeField] private LevelGeneratorLayered mapgGen;

    private Dictionary<TileBase, TileData> dataFromTiles;

    public override void OnStartServer()
    {
        StartCoroutine(TileChecker());
    }

    private IEnumerator TileChecker()
    {
        yield return null;
        chunks = mapgGen.chunks;
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var tileData in tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
        }
    }

    private void Update()
    {

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    Vector3Int gridPos = chunks[0].WorldToCell(mousePos);

        //    TileBase clickedTile = chunks[0].GetTile(gridPos);

        //    string blockType = dataFromTiles[clickedTile].blockType;

          
        //}
    }

    public float BlockStrengthGet(Vector3Int target, Tilemap chunk)
    {
        target = new Vector3Int(target.x, target.y, 0);
        TileBase targetedBlock = chunk.GetTile(target);
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

    public string BlockNameGet(Vector3Int target, Tilemap chunk)
    {
        target = new Vector3Int(target.x, target.y, 0);
        TileBase targetedBlock = chunk.GetTile(target);
        
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

    public string BlockTypeGet(Vector3Int target, Tilemap chunk)
    {
        target = new Vector3Int(target.x, target.y, 0);
        TileBase targetedBlock = chunk.GetTile(target);

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
}
