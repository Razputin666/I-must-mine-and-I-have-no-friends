using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using UnityEngine.Events;

public class GameTiles : MonoBehaviour
{
    //public List<WorldTile> saveTiles;

    //public Dictionary<Vector3, WorldTile> tiles;

    //public Vector3 tilemapWorldPos;

    public event UnityAction<string> OnWorldTilesSet;

    public List<WorldTile> GetWorldTiles(Tilemap tilemap, bool save)
    {
        //tilemapWorldPos = tilemap.transform.position;
        List<WorldTile>  saveTiles = new List<WorldTile>();
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            //tilemap.transform.position
            Vector3Int lPos = new Vector3Int(pos.x, pos.y, pos.z);

            if (!tilemap.HasTile(lPos))
                continue;

            Vector3 temp = tilemap.CellToWorld(lPos);
            WorldTile _tile = new WorldTile()
            {
                localPlace = new Vec3Int(lPos.x, lPos.y, lPos.z),
                gridLocation = new Vec3(temp.x, temp.y, temp.z),
                tileBase = tilemap.GetTile(lPos).name,
                isVisible = false,
                isExplored = false,
                transFormPos = new Vec3(
                    tilemap.transform.position.x, 
                    tilemap.transform.position.y, 
                    tilemap.transform.position.z),
            };

            saveTiles.Add(_tile);
        }

        if (save)
        {
            TilemapDataSystem.Save(tilemap.name, "Map", saveTiles);
        }

        return saveTiles;
    }

    public void LoadWorldTiles(Tilemap tilemap, int index, TileBase[] tileAssets)
    {
        List<WorldTile> saveTiles = TilemapDataSystem.Load(tilemap.name + index, "Map");
        SetWorldTiles(tilemap, "", saveTiles);
    }

    public void SetWorldTiles(Tilemap tilemap, string folderName, List<WorldTile> saveTiles)
    {
        StartCoroutine(SetWorldTilesCoRoutine(tilemap, folderName, saveTiles));
    }

    private IEnumerator SetWorldTilesCoRoutine(Tilemap tilemap, string folderName, List<WorldTile> saveTiles)
    {
        int count = 0;

        string path = Path.Combine("Tilebase", folderName);
        bool startPosSet = false;

        Tile[] tileAsset = Resources.LoadAll<Tile>(path);

        Vector3Int[] posArray = new Vector3Int[saveTiles.Count];
        TileBase[] tileArray = new TileBase[saveTiles.Count];
        int index = 0;
        foreach (WorldTile tile in saveTiles)
        {
            for (int i = 0; i < tileAsset.Length; i++)
            {
                if (tileAsset[i].name == tile.tileBase)
                {
                    posArray[index] = tile.localPlace.Vector3Int();
                    tileArray[index] = tileAsset[i];
                    index++;
                    //tilemap.SetTile(tile.localPlace.Vector3Int(), tileAsset[i]);
                    break;
                }
            }

            if (!startPosSet)
            {
                tilemap.transform.position = tile.transFormPos.Vector3();
                startPosSet = true;
            }

            count++;

            if(count >= 1000)
            {
                count = 0;
                yield return null;
            }    
            //tiles.Add(_tile.gridLocation.Vector3(), _tile);
        }

        OnWorldTilesSet.Invoke(tilemap.name);

        tilemap.SetTiles(posArray, tileArray);
        Resources.UnloadUnusedAssets();
    }
}