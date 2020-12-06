using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapManager : MonoBehaviour
{
    [SerializeField] private List<Tilemap> chunks; //Only serialized for testing

    [SerializeField] private List<TileData> tileDatas;

    [SerializeField] private LevelGeneratorLayered mapgGen;

    private Dictionary<TileBase, TileData> dataFromTiles;

    private void Awake()
    {
        StartCoroutine(TileChecker());
    }

    private IEnumerator TileChecker()
    {
        yield return new WaitForSeconds(0.1f);
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
