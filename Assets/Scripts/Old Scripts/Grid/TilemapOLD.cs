using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapOLD
{
    public event EventHandler OnLoaded;

    private Grid<TilemapObject> grid;
    public TilemapOLD(int width, int height, float cellSize, Vector3 originPosition, TilemapOLD.TilemapObject.TilemapSprite[,] tilemapSpritearray)
    {
        grid = new Grid<TilemapObject>(width, height, cellSize, originPosition, (Grid<TilemapObject> g, int x, int y) => new TilemapObject(g, x, y, tilemapSpritearray[x,y]));
    }

    public void SetTilemapSprite(Vector3 worldPos, TilemapObject.TilemapSprite tilemapSprite)
    {
        TilemapObject tilemapObject = grid.GetGridObject(worldPos);

        if (tilemapObject != null)
            tilemapObject.SetTilemapSprite(tilemapSprite);
    }

    public void SetTilemapVisual(TilemapVisual tilemapVisual)
    {
        tilemapVisual.SetGrid(this, grid);
    }
    #region Save-Load
    public void Save()
    {
        //contains all save object for each single grid cell position.
        List<TilemapObject.SaveObject> tilemapObjectSaveObjectList = new List<TilemapObject.SaveObject>();
        for(int x = 0; x < grid.GetWidth(); x++)
        {
            for(int y = 0; y < grid.GetHeight(); y++)
            {
                TilemapObject tilemapObject = grid.GetGridObject(x, y);
                tilemapObjectSaveObjectList.Add(tilemapObject.Save());
            }
        }
        SaveObject saveObject = new SaveObject
        {
            tilemapObjectSaveObjectArray = tilemapObjectSaveObjectList.ToArray()
        };

        SaveSystem.SaveObject(saveObject);
    }

    public void Load()
    {
        SaveObject saveObject = SaveSystem.LoadMostRecentObject<SaveObject>();

        foreach(TilemapObject.SaveObject tilemapObjectSaveObject in saveObject.tilemapObjectSaveObjectArray)
        {
            TilemapObject tilemapObject = grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.y);
            tilemapObject.Load(tilemapObjectSaveObject);
        }

        OnLoaded?.Invoke(this, EventArgs.Empty);
    }

    public class SaveObject
    {
        public TilemapObject.SaveObject[] tilemapObjectSaveObjectArray;
        public int _width;
        public int _height;
    }

    #endregion
    /*
     * Represents a single Tilemap Object tha exists in each Grid Cell Position
    */
    public class TilemapObject
    {
        public enum TilemapSprite
        {
            None,
            Grass,
            Dirt,
            Stone
        }
        
        private Grid<TilemapObject> grid;
        private int x;
        private int y;

        private TilemapSprite tilemapSprite;

        private BoxCollider2D boxCollider2D;

        public TilemapObject(Grid<TilemapObject> grid, int x, int y, TilemapSprite tilemapSprite = TilemapSprite.None)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            this.tilemapSprite = tilemapSprite;
        }

        public BoxCollider2D GetBoxCollider2D()
        {
            if (boxCollider2D != null)
                return boxCollider2D;
            else
                return null;
        }

        public void SetBoxCollider2D(BoxCollider2D boxCollider2D)
        {
            this.boxCollider2D = boxCollider2D;

            this.boxCollider2D.size = new Vector2(grid.GetCellSize(), grid.GetCellSize());
            this.boxCollider2D.transform.position = grid.GetWorldPosition(this.x, this.y);

            //Debug.Log(this.boxCollider2D.transform.position + " : " + grid.GetWorldPosition(this.x, this.y));
            if (tilemapSprite != TilemapSprite.None)
            {
               this.boxCollider2D.enabled = true;
            }
        }

        public void SetTilemapSprite(TilemapSprite tilemapSprite)
        {
            this.tilemapSprite = tilemapSprite;
            if(this.boxCollider2D != null)
            {
                if (tilemapSprite != TilemapSprite.None)
                {
                    this.boxCollider2D.enabled = true;
                }
                else
                {
                    this.boxCollider2D.enabled = false;
                }
            }
            grid.TriggerGridObjectChanged(x, y);
        }

        public TilemapSprite GetTilemapSprite()
        {
            return tilemapSprite;
        }

        public override string ToString()
        {
            return tilemapSprite.ToString();
        }
        [System.Serializable]
        public class SaveObject
        {
            public TilemapSprite _tilemapSprite;
            public int x;
            public int y;
        }
        // Save - Load
        public SaveObject Save()
        {
            return new SaveObject
            {
                _tilemapSprite = tilemapSprite,
                x = x,
                y = y,
            };
        }

        public void Load(SaveObject saveObject)
        {
            tilemapSprite = saveObject._tilemapSprite;
        }
    }
}
