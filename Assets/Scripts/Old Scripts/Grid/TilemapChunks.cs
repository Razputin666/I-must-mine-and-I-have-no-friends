using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapChunks
{
    //Total width and height of the terrain.
    private int _width;
    private int _height;

    private int _numberOfchunksWidth;
    private int _numberOfchunksHeight;

    //Width and height for each chunk.
    //private static int MAX_CHUNK_WIDTH = 10;
    //private static int MAX_CHUNK_HEIGHT = 10;
    private int _chunkWidth;
    private int _chunkHeight;
    //Contains all the tilemap chunks
    private Tilemap[] _tilemapChunks;
    public TilemapChunks(int width, int height, int chunkWidth, int chunkHeight, float cellSize, Vector3 originPosition, Tilemap.TilemapObject.TilemapSprite[,] tilemapSpritearray, TilemapVisual tilemapVisual)
    {
        //Total width and height of the terrain.
        this._width = width;
        this._height = height;

        this._chunkWidth = chunkWidth;
        this._chunkHeight = chunkHeight;

        //Number of Tilemap chunks on the width and height
        this._numberOfchunksWidth = _width / _chunkWidth;
        this._numberOfchunksHeight = _height / _chunkWidth;

        //Create a new tilemap array to hold the chunks
        _tilemapChunks = new Tilemap[this._numberOfchunksWidth * this._numberOfchunksHeight];

        Vector3 chunkStartPosition;

        int index = 0;
        for (int x = 0; x < _numberOfchunksWidth; x++)
        {
            for (int y = 0; y < _numberOfchunksHeight; y++)
            {
                //Set the start position for the new chunk.
                chunkStartPosition = new Vector3(x * _chunkWidth * cellSize, y * _chunkHeight * cellSize) + originPosition;
                
                //Clone the tilemapvisual for the tilemap
                TilemapVisual tilemapVisualClone = TilemapVisual.Instantiate(tilemapVisual);
                //tilemapVisualClone.transform.parent = tilemapVisual.transform;
                Tilemap.TilemapObject.TilemapSprite[,] tilemapSpriteArrayCopy = GenerateSpriteCopy(x, y, tilemapSpritearray);
                //Create a new tilemap chunk
                Tilemap tilemap = new Tilemap(_chunkWidth, _chunkHeight, cellSize, chunkStartPosition, tilemapSpriteArrayCopy);
                tilemap.SetTilemapVisual(tilemapVisualClone);
                //Add the chunk to the array
                this._tilemapChunks[index++] = tilemap;
            }
        }
    }

    private Tilemap.TilemapObject.TilemapSprite[,] GenerateSpriteCopy(int x, int y, Tilemap.TilemapObject.TilemapSprite[,] tilemapSpriteArray)
    {
        Tilemap.TilemapObject.TilemapSprite[,] tilemapSpriteArrayCopy = new Tilemap.TilemapObject.TilemapSprite[_chunkWidth, _chunkHeight];

        for (int x2 = 0; x2 < _chunkWidth; x2++)
        {
            for (int y2 = 0; y2 < _chunkHeight; y2++)
            {
                tilemapSpriteArrayCopy[x2, y2] = tilemapSpriteArray[x * _chunkWidth + x2, y * _chunkHeight + y2];
            }
        }

        return tilemapSpriteArrayCopy;
    }
}
