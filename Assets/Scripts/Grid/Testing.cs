
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour 
{
    //private Grid<HeatMapGridObject> grid;
    //private Grid<bool> boolGrid;
    //private Grid<StringGridObject> stringGrid;
    //private Grid<GridBlockObject> blockGrid;
    [SerializeField]
    private Camera camera;

    private Tilemap _tilemap;

    [SerializeField]
    private TilemapVisual _tilemapVisual;

    private Tilemap.TilemapObject.TilemapSprite _tilemapSprite;

    Tilemap.TilemapObject.TilemapSprite[,] tilemapSpritearray;

    private void Start() 
    {
        //grid = new Grid<HeatMapGridObject>(20, 10, 8f, Vector3.zero, (Grid<HeatMapGridObject> g, int x, int y) => new HeatMapGridObject(g, x, y));
        //boolGrid = new Grid<bool>(20, 10, 8f, Vector3.zero, (Grid<bool> g, int x, int y) => false);
        //stringGrid = new Grid<StringGridObject>(20, 10, 8f, Vector3.zero, (Grid<StringGridObject> g, int x, int y) => new StringGridObject(g, x, y));

        //heatMapVisual.SetGrid(grid);
        //heatMapBoolVisual.SetGrid(boolGrid);
        //heatMapGenericVisual.SetGrid(grid);

        //blockGrid = new Grid<GridBlockObject>(20, 20, 10f, Vector3.zero, (Grid<GridBlockObject> gameObject, int x, int y) => new GridBlockObject(g, x, y));

        GenTerrain();

        _tilemap = new Tilemap(100, 50, 5f, Vector3.zero, tilemapSpritearray);
        _tilemap.SetTilemapVisual(_tilemapVisual);
    }

    int NoiseInt(int x, int y, float scale, float mag, float exp)
    {
        return (int)Mathf.Pow((Mathf.PerlinNoise(x / scale, y / scale) * mag), (exp));
    }

    void GenTerrain()
    {
        tilemapSpritearray = new Tilemap.TilemapObject.TilemapSprite[100, 50];

        for (int px = 0; px < tilemapSpritearray.GetLength(0); px++)
        {
            int grass = NoiseInt(px, 0, 80, 5, 1);

            // Layer one has a scale of 80 making it quite smooth with large rolling hills, 
            // the magnitude is 15 so the hills are at most 15 high (but in practice they're usually around 12 at the most) 
            // and at the least 0 and the exponent is 1 so no change is applied exponentially.
            int stone = NoiseInt(px, 0, 80, 15, 1);
            //The next layer has a smaller scale so it's more choppy (but still quite tame) 
            //and has a larger magnitude so a higher max height. This ends up being the most prominent layer making the hills.
            stone += NoiseInt(px, 0, 50, 30, 1);
            //The third layer has an even smaller scale so it's even noisier but it's magnitude is 10 so its max height is lower, 
            //it's mostly for adding some small noise to the stone to make it look more natural. Lastly we add 75 to the stone to raise it up.
            stone += NoiseInt(px, 0, 10, 10, 1);
            stone += 75;

            //The dirt layer has to be mostly higher than the stone so the magnitudes here are higher 
            //but the scales are 100 and 50 which gives us rolling hills with little noise. Again we add 75 to raise it up.
            int dirt = NoiseInt(px, 0, 100f, 35, 1);
            dirt += NoiseInt(px, 100, 50, 30, 1);
            dirt += 75;

            for (int py = 0; py < tilemapSpritearray.GetLength(1); py++)
            {
                if (py < stone)
                {
                    tilemapSpritearray[px, py] = Tilemap.TilemapObject.TilemapSprite.Stone;
                    //The next three lines make dirt spots in random places
                    if (NoiseInt(px, py, 12, 16, 1) > 10)
                    {  //dirt spots
                        tilemapSpritearray[px, py] = Tilemap.TilemapObject.TilemapSprite.Dirt;

                    }
                    //The next three lines remove dirt and rock to make caves in certain places
                    if (NoiseInt(px, py * 2, 16, 14, 1) > 10)
                    { //Caves
                        tilemapSpritearray[px, py] = Tilemap.TilemapObject.TilemapSprite.None;

                    }
                }
                else if (py < dirt)
                {
                    tilemapSpritearray[px, py] = Tilemap.TilemapObject.TilemapSprite.Grass;
                }
            }
        }
    }

    private void Update() 
    {
        //Vector3 position = camera.ScreenToWorldPoint(Input.mousePosition);

        //if(Input.GetMouseButton(0))
        //{
        //    Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
        //    _tilemap.SetTilemapSprite(mouseWorldPosition, _tilemapSprite);
        //}
        //if(Input.GetKeyDown(KeyCode.T))
        //{
        //    _tilemapSprite = Tilemap.TilemapObject.TilemapSprite.Grass;
        //    Debug.Log(_tilemapSprite.ToString());
        //}
        //else if(Input.GetKeyDown(KeyCode.Y))
        //{
        //    _tilemapSprite = Tilemap.TilemapObject.TilemapSprite.Dirt;
        //    Debug.Log(_tilemapSprite.ToString());
        //}
        //else if (Input.GetKeyDown(KeyCode.U))
        //{
        //    _tilemapSprite = Tilemap.TilemapObject.TilemapSprite.Stone;
        //    Debug.Log(_tilemapSprite.ToString());
        //}

        if (Input.GetKeyDown(KeyCode.P))
        {
            _tilemap.Save();
            Debug.Log("Saved!");
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            _tilemap.Load();
            Debug.Log("Loaded!");
        }

        /*
        if (Input.GetMouseButtonDown(0))
        {
            //boolGrid.SetGridObject(position, true);
            HeatMapGridObject heatMapGridObject = grid.GetGridObject(position);
            if (heatMapGridObject != null)
            {
                heatMapGridObject.AddValue(5);
            }
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            stringGrid.GetGridObject(position).AddLetter("A");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            stringGrid.GetGridObject(position).AddLetter("B");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            stringGrid.GetGridObject(position).AddLetter("C");
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            stringGrid.GetGridObject(position).AddNumber("1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            stringGrid.GetGridObject(position).AddNumber("2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            stringGrid.GetGridObject(position).AddNumber("3");
        }
        */
    }
}

//public class HeatMapGridObject 
//{

//    private const int MIN = 0;
//    private const int MAX = 100;

//    private Grid<HeatMapGridObject> grid;
//    private int x;
//    private int y;
//    private int value;

//    public HeatMapGridObject(Grid<HeatMapGridObject> grid, int x, int y) 
//    {
//        this.grid = grid;
//        this.x = x;
//        this.y = y;
//    }

//    public void AddValue(int addValue) {
//        value += addValue;
//        value = Mathf.Clamp(value, MIN, MAX);
//        grid.TriggerGridObjectChanged(x, y);
//    }

//    public float GetValueNormalized() 
//    {
//        return (float)value / MAX;
//    }

//    public override string ToString() 
//    {
//        return value.ToString();
//    }

//}

//public class StringGridObject 
//{
    
//    private Grid<StringGridObject> grid;
//    private int x;
//    private int y;

//    private string letters;
//    private string numbers;
    
//    public StringGridObject(Grid<StringGridObject> grid, int x, int y) 
//    {
//        this.grid = grid;
//        this.x = x;
//        this.y = y;
//        letters = "";
//        numbers = "";
//    }

//    public void AddLetter(string letter) 
//    {
//        letters += letter;
//        grid.TriggerGridObjectChanged(x, y);
//    }

//    public void AddNumber(string number) 
//    {
//        numbers += number;
//        grid.TriggerGridObjectChanged(x, y);
//    }

//    public override string ToString() 
//    {
//        return letters + "\n" + numbers;
//    }

//}
