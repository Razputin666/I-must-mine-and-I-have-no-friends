using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelGeneratorLayered : MonoBehaviour
{
	[Tooltip("The Tilemap to draw onto")]
	[SerializeField]
	private Tilemap tilemap;
	[Tooltip("The Tile to draw (use a RuleTile for best results)")]
	[SerializeField]
	private TileBase[] tiles;

	[Tooltip("Width of our map")]
	[SerializeField]
	private int width;
	[Tooltip("Height of our map")]
	[SerializeField]
	private int height;

	[Tooltip("The settings of our map")]
	[SerializeField]
	public List<MapSettings> mapSettingsLeft = new List<MapSettings>();

	[Tooltip("The settings of our map")]
	[SerializeField]
	public List<MapSettings> mapSettingsRight = new List<MapSettings>();

	List<int[,]> mapList = new List<int[,]>();

	[SerializeField] private int numberOfLoopsNeumann;
	[SerializeField] private int numberOfLoopsMoore;
	[SerializeField] private int numberOfLoopsPerlin;
	[SerializeField] private int numberOfLoopsRandomWalkCave;
	[SerializeField] private float randomized;
	[SerializeField] private MapSettings mainMap;
	int neigbourCaveX;
	int neigbourCaveY;
	int neigbourCaveWidth;
	int neigbourCaveHeight;

	private void Start()
	{
		ClearMap();
		Vector2Int startPosition = new Vector2Int(-width / 2, (-height / 2) - 1);

		GenerateMainMap(mainMap, startPosition, width, height, true);
		//CreateFeatures(numberOfLoopsNeumann, mapSettingsLeft[0]);
		//CreateFeatures(numberOfLoopsMoore, mapSettingsLeft[1]);
		//CreateFeatures(numberOfLoopsPerlin, mapSettingsLeft[3]);
		CreateBigCaves(numberOfLoopsRandomWalkCave, mapSettingsLeft[3]);

		int numberOfCaves = width / 200;

		Debug.Log(numberOfCaves + " cave number");

  //      for (int i = 0; i < numberOfCaves; i++)
  //      {
		//	CreateCaves(numberOfCaves, mapSettingsLeft[2]);
		//}


	}

	void CreateFeatures(int numberOfLoops, MapSettings mapSettings)
    {
		for (int i = 0; i < numberOfLoops; i++)
		{
			int randomX = UnityEngine.Random.Range(-width / 2, width / 2);
			int randomY = UnityEngine.Random.Range(-height / 2, height / 2);
			int randomWidth = UnityEngine.Random.Range(width / 100, width / 25);
			int randomHeight = UnityEngine.Random.Range(height / 100, height / 15);

			if (randomX > (width / 2 - 20))
			{
				randomX -= width / 100;
				randomWidth = width / 100 - 1;
			}

			if (randomY > (height / 2 - 20))
			{
				randomY -= (height / 100);
				randomHeight = height / 100 - 1;
			}

			if (randomX + randomWidth >= width / 2)
			{
				int checker = (randomX + randomWidth - width / 2) - 1;
				randomWidth += checker;

			}

			if (randomY + randomHeight >= height / 2)
			{
				int checker = (randomY + randomHeight - height / 2) - 1;
				randomHeight += checker;

			}


			GenerateMainMap(mapSettings, new Vector2Int(randomX, randomY), randomWidth, randomHeight, false);

			//int randomValue = i > 0 ? UnityEngine.Random.Range(0, 12) : 0;
			//Vector2Int newStartPosition = new Vector2Int(randomValue, -randomValue);

			//GenerateMap(mapSettingsLeft, startPosition - newStartPosition,newWidth + newStartPosition.x,newHeight - newStartPosition.y);
			//startPosition.x += newWidth - 1;
			////GenerateMap(mapSettingsRight, startPosition, newWidth, newHeight);
			////startPosition.x += newWidth - 1;
			//randomized = UnityEngine.Random.Range(0f, 15f);
		}
	}

	void CreateCaves(int numberOfLoops, MapSettings mapSettings)
    {
		for (int i = 0; i < numberOfLoops; i++)
		{
			int randomX = UnityEngine.Random.Range(-width / 2, width / 2);
		 	int randomWidth = width / 10;
			int randomY = 0;
			int randomHeight = height / 2;

			if (randomX > (width / 2 - 20))
			{
				randomX -= width / 100;
				randomWidth = width / 100 - 1;
			}

			if (randomY > (height / 2 - 20))
			{
				randomY -= (height / 100);
				randomHeight = height / 100 - 1;
			}

			if (randomX + randomWidth >= width / 2)
			{
				int checker = (randomX + randomWidth - width / 2) - 1;
				randomWidth += checker;

			}

			if (randomY + randomHeight >= height / 2)
			{
				int checker = (randomY + randomHeight - height / 2) - 1;
				randomHeight += checker;

			}



            GenerateMainMap(mapSettings, new Vector2Int(randomX, randomY), randomWidth, randomHeight, false);

		}
	}

	void CreateBigCaves(int numberOfLoops, MapSettings mapSettings)
	{
		for (int i = 0; i < numberOfLoops; i++)
		{
			int randomX = UnityEngine.Random.Range(-width / 2, width / 2);
			int randomY = UnityEngine.Random.Range(-height / 2, height / 2);
			int randomWidth = UnityEngine.Random.Range(width / 100, width / 2);
			int randomHeight = UnityEngine.Random.Range(height / 100, height / 2);


			if (randomX > (width / 2 - 20))
			{
				randomX -= width / 100;
				randomWidth = width / 100 - 1;
			}

			if (randomY > (height / 2 - 20))
			{
				randomY -= (height / 100);
				randomHeight = height / 100 - 1;
			}

			if (randomX + randomWidth >= width / 2)
			{
				int checker = (randomX + randomWidth - width / 2) - 1;
				randomWidth += checker;

			}

			if (randomY + randomHeight >= height / 2)
			{
				int checker = (randomY + randomHeight - height / 2) - 1;
				randomHeight += checker;

			}
			neigbourCaveX = randomX;
			neigbourCaveY = randomY;
			neigbourCaveWidth = randomWidth;
			neigbourCaveHeight = randomHeight;


			GenerateMainMap(mapSettings, new Vector2Int(randomX, randomY), randomWidth, randomHeight, false);
			MapSettings newMapSettings = new MapSettings();
			newMapSettings = mapSettings;
			newMapSettings.clearAmount = mapSettings.clearAmount / 2;
			GenerateMainMap(newMapSettings, new Vector2Int(randomX -= (randomWidth / 2) - 1, randomY), randomWidth, randomHeight, false);
			GenerateMainMap(newMapSettings, new Vector2Int(randomX += (randomWidth / 2) - 1, randomY), randomWidth, randomHeight, false);
			GenerateMainMap(newMapSettings, new Vector2Int(randomX, randomY -= randomHeight - 1), randomWidth, randomHeight, false);
			GenerateMainMap(newMapSettings, new Vector2Int(randomX, randomY += randomHeight), randomWidth, randomHeight, false);

			//int randomValue = i > 0 ? UnityEngine.Random.Range(0, 12) : 0;
			//Vector2Int newStartPosition = new Vector2Int(randomValue, -randomValue);

			//GenerateMap(mapSettingsLeft, startPosition - newStartPosition,newWidth + newStartPosition.x,newHeight - newStartPosition.y);
			//startPosition.x += newWidth - 1;
			////GenerateMap(mapSettingsRight, startPosition, newWidth, newHeight);
			////startPosition.x += newWidth - 1;
			//randomized = UnityEngine.Random.Range(0f, 15f);
		}
	}


	private void Update()
	{
		//if (Input.GetKeyDown(KeyCode.N))
		//{
		//	ClearMap();
		//	GenerateMap();
		//}
	}

	[ExecuteInEditMode]
	public void GenerateMap(List<MapSettings> mapSettings, Vector2Int startPosition, int width, int height)
	{
		//ClearMap();
		Debug.Log(startPosition);
		mapList = new List<int[,]>();
		for(int i = 0; i < mapSettings.Count; i++)
        {
			int[,] map = new int[width, height];
			float seed;
			if (mapSettings[i].randomSeed)
			{
				seed = UnityEngine.Random.Range(0f,1f);
			}
			else
			{
				seed = mapSettings[i].seed;
			}

			//Generate the map depending on mapSen the algorithm selected
			switch (mapSettings[i].algorithm)
			{
				case Algorithm.Perlin:
					//First generate our array
					map = MapFunctions.GenerateArray(width, height, true);
					//Next generate the perlin noise onto the array
					map = MapFunctions.PerlinNoise(map, seed);
					break;
				case Algorithm.PerlinSmoothed:
					//First generate our array
					map = MapFunctions.GenerateArray(width, height, true);
					//Next generate the perlin noise onto the array
					map = MapFunctions.PerlinNoiseSmooth(map, seed, mapSettings[i].interval -(int)randomized);
					break;
				case Algorithm.PerlinCave:
					//First generate our array
					map = MapFunctions.GenerateArray(width, height, true);
					//Next generate the perlin noise onto the array
					map = MapFunctions.PerlinNoiseCave(map, mapSettings[i].modifier - randomized, mapSettings[i].edgesAreWalls);
					break;
				case Algorithm.RandomWalkTop:
					//First generate our array
					map = MapFunctions.GenerateArray(width, height, true);
					//Next generater the random top
					map = MapFunctions.RandomWalkTop(map, seed);
					break;
				case Algorithm.RandomWalkTopSmoothed:
					//First generate our array
					map = MapFunctions.GenerateArray(width, height, true);
					//Next generate the smoothed random top
					map = MapFunctions.RandomWalkTopSmoothed(map, seed, mapSettings[i].interval - (int)randomized, mapSettings[i].randomHeightStart);
					break;
				case Algorithm.RandomWalkCave:
					//First generate our array
					map = MapFunctions.GenerateArray(width, height, false);
					//Next generate the random walk cave
					map = MapFunctions.RandomWalkCave(map, seed, mapSettings[i].clearAmount);
					break;
				case Algorithm.RandomWalkCaveCustom:
					//First generate our array
					map = MapFunctions.GenerateArray(width, height, false);
					//Next generate the custom random walk cave
					map = MapFunctions.RandomWalkCaveCustom(map, seed, mapSettings[i].clearAmount);
					break;
				case Algorithm.CellularAutomataVonNeuman:
					//First generate the cellular automata array
					map = MapFunctions.GenerateCellularAutomata(width, height, seed, mapSettings[i].fillAmount, mapSettings[i].edgesAreWalls);
					//Next smooth out the array using the von neumann rules
					map = MapFunctions.SmoothVNCellularAutomata(map, mapSettings[i].edgesAreWalls, mapSettings[i].smoothAmount);
					break;
				case Algorithm.CellularAutomataMoore:
					//First generate the cellular automata array
					map = MapFunctions.GenerateCellularAutomata(width, height, seed, mapSettings[i].fillAmount, mapSettings[i].edgesAreWalls);
					//Next smooth out the array using the Moore rules
					map = MapFunctions.SmoothMooreCellularAutomata(map, mapSettings[i].edgesAreWalls, mapSettings[i].smoothAmount);
					break;
				case Algorithm.DirectionalTunnel:
					//First generate our array
					map = MapFunctions.GenerateArray(width, height, false);
					//Next generate the tunnel through the array
					map = MapFunctions.DirectionalTunnel(map, mapSettings[i].minPathWidth, mapSettings[i].maxPathWidth, mapSettings[i].maxPathChange, mapSettings[i].roughness, mapSettings[i].windyness);
					break;
			}
            //Add the map to the list
            mapList.Add(map);
		}
		//Allows for all of the maps to be on the same tilemap without overlaying
		Vector2Int offset = startPosition;

		//Work through the list to generate all maps
		bool topLayer = true;

		foreach (int[,] map in mapList)
		{
			if(topLayer)
            {
				MapFunctions.RenderMapWithOffset(map, tilemap, tiles[0], offset, true);
				offset.y += -height + 1;
			}

			else
            {
				MapFunctions.RenderMapWithOffset(map, tilemap, tiles[1], offset, true);
				offset.y += -height + 1;
			}

		}
	}

	[ExecuteInEditMode]
	public void GenerateMainMap(MapSettings mapSettings, Vector2Int startPosition, int width, int height, bool addTiles) 
	{
		//ClearMap();
		Debug.Log(startPosition + "Main");
		
		int[,] map = new int[width, height];
		float seed;
		if (mapSettings.randomSeed)
		{
			seed = UnityEngine.Random.Range(0f, 1f);
		}
		else
		{
			seed = mapSettings.seed;
		}

		//Generate the map depending on mapSen the algorithm selected
		switch (mapSettings.algorithm)
		{
			case Algorithm.Perlin:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, true);
				//Next generate the perlin noise onto the array
				map = MapFunctions.PerlinNoise(map, seed);
				break;
			case Algorithm.PerlinSmoothed:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, true);
				//Next generate the perlin noise onto the array
				map = MapFunctions.PerlinNoiseSmooth(map, seed, mapSettings.interval - (int)randomized);
				break;
			case Algorithm.PerlinCave:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, true);
				//Next generate the perlin noise onto the array
				map = MapFunctions.PerlinNoiseCave(map, mapSettings.modifier - randomized, mapSettings.edgesAreWalls);
				break;
			case Algorithm.RandomWalkTop:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, true);
				//Next generater the random top
				map = MapFunctions.RandomWalkTop(map, seed);
				break;
			case Algorithm.RandomWalkTopSmoothed:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, true);
				//Next generate the smoothed random top
				map = MapFunctions.RandomWalkTopSmoothed(map, seed, mapSettings.interval, mapSettings.randomHeightStart);
				break;
			case Algorithm.RandomWalkCave:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, false);
				//Next generate the random walk cave
				map = MapFunctions.RandomWalkCave(map, seed, mapSettings.clearAmount);
				break;
			case Algorithm.RandomWalkCaveCustom:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, false);
				//Next generate the custom random walk cave
				map = MapFunctions.RandomWalkCaveCustom(map, seed, mapSettings.clearAmount);
				break;
			case Algorithm.CellularAutomataVonNeuman:
				//First generate the cellular automata array
				map = MapFunctions.GenerateCellularAutomata(width, height, seed, mapSettings.fillAmount, mapSettings.edgesAreWalls);
				//Next smooth out the array using the von neumann rules
				map = MapFunctions.SmoothVNCellularAutomata(map, mapSettings.edgesAreWalls, mapSettings.smoothAmount);
				break;
			case Algorithm.CellularAutomataMoore:
				//First generate the cellular automata array
				map = MapFunctions.GenerateCellularAutomata(width, height, seed, mapSettings.fillAmount, mapSettings.edgesAreWalls);
				//Next smooth out the array using the Moore rules
				map = MapFunctions.SmoothMooreCellularAutomata(map, mapSettings.edgesAreWalls, mapSettings.smoothAmount);
				break;
			case Algorithm.DirectionalTunnel:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, false);
				//Next generate the tunnel through the array
				map = MapFunctions.DirectionalTunnel(map, mapSettings.minPathWidth, mapSettings.maxPathWidth, mapSettings.maxPathChange, mapSettings.roughness, mapSettings.windyness);
				break;
		}

		MapFunctions.RenderMapWithOffset(map, tilemap, tiles[0], startPosition, addTiles);
	//	StartCoroutine(MapFunctions.RenderMapWithDelay(map, tilemap, tiles[0], startPosition, addTiles));
	}


	public void ClearMap()
	{
		tilemap.ClearAllTiles();
	}
}

//[CustomEditor(typeof(LevelGeneratorLayered))]
//public class LevelGeneratorLayeredEditor : Editor
//{
//	public override void OnInspectorGUI()
//	{
//		base.OnInspectorGUI();
//		//Create a reference to our script
//		LevelGeneratorLayered levelGen = (LevelGeneratorLayered)target;

//		//List of editors to only show if we have elements in the map settings list
//		List<Editor> mapEditors = new List<Editor>();

//		for (int i = 0; i < levelGen.mapSettingsLeft.Count; i++)
//		{
//			if (levelGen.mapSettingsLeft[i] != null)
//			{
//				Editor mapLayerEditor = CreateEditor(levelGen.mapSettingsLeft[i]);
//				mapEditors.Add(mapLayerEditor);
//			}
//		}
//		//If we have more than one editor in our editor list, draw them out. Also draw the buttons
//		if (mapEditors.Count > 0)
//		{
//			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
//			for (int i = 0; i < mapEditors.Count; i++)
//			{
//				mapEditors[i].OnInspectorGUI();
//			}

//			if (GUILayout.Button("Generate"))
//			{
//				levelGen.GenerateMap();
//			}


//			if (GUILayout.Button("Clear"))
//			{
//				levelGen.ClearMap();
//			}
//		}
//	}
//}
