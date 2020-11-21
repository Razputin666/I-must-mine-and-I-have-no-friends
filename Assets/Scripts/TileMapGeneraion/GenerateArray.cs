//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class GenerateArray : MonoBehaviour
//{
//    [SerializeField]
//    private int mapWidth;
//    [SerializeField]
//    private int mapHeight;

//    private int[,] mapToBeGenerated;

//    [SerializeField]
//    private Tilemap generatedTileMap;

//    [SerializeField]
//    private TileBase[] testTiles;


//    private void Awake()
//    {
//        generatedTileMap = gameObject.GetComponent<Tilemap>();

//        TerrainGenerator terrainGenerator = new TerrainGenerator();

//        terrainGenerator.GenerateTiles(mapWidth, mapHeight);
//        mapToBeGenerated = terrainGenerator.GetGeneratedTiles();

//        RenderMap(mapToBeGenerated, generatedTileMap, testTiles);
//    }

//    public void RenderMap(int[,] map, UnityEngine.Tilemaps.Tilemap tilemap, TileBase[] tiles)
//    {
//        //Clear the map (ensures we dont overlap)
//        tilemap.ClearAllTiles();

//        //Loop through the width of the map
//        for (int x = 0; x < map.GetUpperBound(0); x++)
//        {
//            bool isGround = true;
//            //Loop through the height of the map
//            for (int y = map.GetUpperBound(1) - 1; y >= 0; y--)
//            {
//                // 1 = tile, 0 = no tile
//                if (map[x, y] == 1)
//                {
//                    //if we are at ground level set the tile to Grass else set it to Dirt
//                    if (isGround)
//                    {
//                        isGround = false;
//                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[0]);
//                    }
//                    else
//                    {
//                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[1]);
//                    }
//                }
//            }
//        }
//    }

//    public static void UpdateMap(int[,] map, UnityEngine.Tilemaps.Tilemap tilemap)
//    {
//        for (int x = 0; x < map.GetUpperBound(0); x++)
//        {
//            for (int y = 0; y < map.GetUpperBound(0); y++)
//            {
//                //We are only going to update the map, rather than rendering again
//                //This is because it uses less resources to update tiles to null
//                //As opposed to re-drawing every single tile (and collision data)
//                if (map[x, y] == 0)
//                {
//                    tilemap.SetTile(new Vector3Int(x, y, 0), null);
//                }
//            }
//        }
//    }
// }