
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

    private void Start() 
    {
        //grid = new Grid<HeatMapGridObject>(20, 10, 8f, Vector3.zero, (Grid<HeatMapGridObject> g, int x, int y) => new HeatMapGridObject(g, x, y));
        //boolGrid = new Grid<bool>(20, 10, 8f, Vector3.zero, (Grid<bool> g, int x, int y) => false);
        //stringGrid = new Grid<StringGridObject>(20, 10, 8f, Vector3.zero, (Grid<StringGridObject> g, int x, int y) => new StringGridObject(g, x, y));

        //heatMapVisual.SetGrid(grid);
        //heatMapBoolVisual.SetGrid(boolGrid);
        //heatMapGenericVisual.SetGrid(grid);

        //blockGrid = new Grid<GridBlockObject>(20, 20, 10f, Vector3.zero, (Grid<GridBlockObject> gameObject, int x, int y) => new GridBlockObject(g, x, y));

        _tilemap = new Tilemap(20, 10, 5f, Vector3.zero);
        _tilemap.SetTilemapVisual(_tilemapVisual);
    }

    private void Update() 
    {
        //Vector3 position = camera.ScreenToWorldPoint(Input.mousePosition);

        if(Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
            _tilemap.SetTilemapSprite(mouseWorldPosition, _tilemapSprite);
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            _tilemapSprite = Tilemap.TilemapObject.TilemapSprite.Grass;
            Debug.Log(_tilemapSprite.ToString());
        }
        else if(Input.GetKeyDown(KeyCode.Y))
        {
            _tilemapSprite = Tilemap.TilemapObject.TilemapSprite.Dirt;
            Debug.Log(_tilemapSprite.ToString());
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            _tilemapSprite = Tilemap.TilemapObject.TilemapSprite.Stone;
            Debug.Log(_tilemapSprite.ToString());
        }


        //if (Input.GetMouseButtonDown(0)) 
        //{
        //    //boolGrid.SetGridObject(position, true);
        //    HeatMapGridObject heatMapGridObject = grid.GetGridObject(position);
        //    if (heatMapGridObject != null) 
        //    {
        //        heatMapGridObject.AddValue(5);
        //    }
        //}
        //if (Input.GetKeyDown(KeyCode.A)) 
        //{ 
        //    stringGrid.GetGridObject(position).AddLetter("A"); 
        //}
        //if (Input.GetKeyDown(KeyCode.B)) 
        //{ 
        //    stringGrid.GetGridObject(position).AddLetter("B"); 
        //}
        //if (Input.GetKeyDown(KeyCode.C)) 
        //{ 
        //    stringGrid.GetGridObject(position).AddLetter("C"); 
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha1)) 
        //{ 
        //    stringGrid.GetGridObject(position).AddNumber("1"); 
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2)) 
        //{ 
        //    stringGrid.GetGridObject(position).AddNumber("2"); 
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3)) 
        //{ 
        //    stringGrid.GetGridObject(position).AddNumber("3"); 
        //}

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
