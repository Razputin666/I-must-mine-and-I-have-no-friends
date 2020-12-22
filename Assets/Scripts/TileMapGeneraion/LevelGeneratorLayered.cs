using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using Mirror;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TypeOfPlanet
{
	Earthlike, meme
}

public class LevelGeneratorLayered : NetworkBehaviour
{
	[Tooltip("The Tilemap to draw onto")]
	//[SerializeField]
	//private Tilemap tilemap;
	[SerializeField]
	private GameObject tileChunk;
	[Tooltip("The Tile to draw (use a RuleTile for best results)")]
	[SerializeField]
	public TileBase[] tiles;

	[Tooltip("Width of our map")]
	[SerializeField]
	public int width;
	[Tooltip("Height of our map")]
	[SerializeField]
	public int height;

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
	[SerializeField] private int customCaveLoops;
	[SerializeField] private int numberOfChunks;
	[SerializeField] private GameObject worldWrappingTeleport;
	[SerializeField] private GrassPlanetOverworldChunk grassOverWorldChunk;
	[SerializeField] private GrassPlanetUnderworldChunk grassUnderWorldChunk;
	[SerializeField] private GameObject playerCharacter;
	[SerializeField] private GameObject drillLaser;
	[SerializeField] private GameObject evilBeavis;
	[SerializeField] private GameObject evilBeavisBase;
	public List<Tilemap> chunks = new List<Tilemap>();

	public float grassGrowthTimeToReach;
	public Vector2Int startPosition;
	private TypeOfPlanet typeOfPlanet { get; set; }
	private List<int> chunkHeightOffset;

	private GameTiles gameTiles;

	private NetworkTransmitter networkTransmitter;

	[SyncVar]
	private int mapSeed = 0;

    public override void OnStartServer()
	{
		//if (gameTiles == null)
		//	gameTiles = new GameTiles();

		//if (networkTransmitter == null)
		//	networkTransmitter = GetComponent<NetworkTransmitter>();

		mapSeed = UnityEngine.Random.Range(1, 100000);

		UnityEngine.Random.InitState(mapSeed);
		typeOfPlanet = TypeOfPlanet.Earthlike;
		WorldGeneration();
		//LoadMap();
	}

	//public override void OnStartClient()
	//{
	//	if (isServer)
	//		return;

	//	if (gameTiles == null)
	//		gameTiles = new GameTiles();

	//	if (networkTransmitter == null)
	//		networkTransmitter = GetComponent<NetworkTransmitter>();
	//	Debug.Log(mapSeed);
	//	UnityEngine.Random.InitState(mapSeed);

	//	typeOfPlanet = TypeOfPlanet.Earthlike;
	//	WorldGeneration();
	//	//LoadMap();
	//}

    //[Client]
    //private void MyCompletlyRecievedHandler(int transmissionID, byte[] data)
    //{
    //    Debug.Log("Transmission: " + transmissionID);

    //    GameObject grid = GameObject.Find("Grid");
    //    GameObject chunk = Instantiate(GameObject.Find("tilechunk"), grid.transform);

    //    chunk.GetComponent<TilemapCollider2D>().maximumTileChangeCount = 100;

    //    Tilemap tm = chunk.GetComponent<Tilemap>();

    //    List<WorldTile> saveData = DeserializeMap(data);
    //    gameTiles.saveTiles = saveData;
    //    gameTiles.SetWorldTiles(tm, "");

    //}

    //[Server]
    //private void SendMapData(NetworkConnection conn, List<Tilemap> tilemaps)
    //{
    //    for (int i = 0; i < tilemaps.Count; i++)
    //    {
    //        gameTiles.GetWorldTiles(tilemaps[i], false);
    //        byte[] tmByte = SerializeTileMap(gameTiles.saveTiles);

    //        networkTransmitter.SendBytesToClient(conn, i, tmByte);
    //    }
    //}

    //[Server]
    //private byte[] SerializeTileMap(List<WorldTile> saveTiles)
    //{
    //    BinaryFormatter bf = new BinaryFormatter();
    //    MemoryStream mf = new MemoryStream();

    //    bf.Serialize(mf, saveTiles);
    //    mf.Close();

    //    return mf.ToArray();
    //}

    //[Client]
    //private List<WorldTile> DeserializeMap(byte[] data)
    //{
    //    BinaryFormatter bf = new BinaryFormatter();
    //    MemoryStream mf = new MemoryStream(data);

    //    List<WorldTile> saveTiles = (List<WorldTile>)bf.Deserialize(mf);
    //    mf.Close();

    //    return saveTiles;
    //}

    //[Command(ignoreAuthority = true)]
    //private void CmdRequestCurrentMap(NetworkIdentity target)
    //{
    //    Debug.Log(target.connectionToClient);
    //    SendMapData(target.connectionToClient, chunks);
    //}
    //[Client]
    //private void MyFragmentReceivedHandler(int transmissionID, byte[] data)
    //   {
    //	Debug.Log("Partly recieved data");
    //   }

    private void WorldGeneration()
	{
		//List<int[,]> mapList = new List<int[,]>();
		//mapList = LoadMap();
		switch (typeOfPlanet)
		{
			case TypeOfPlanet.Earthlike:
				ClearMap();
				GameObject grid = GameObject.Find("Grid");
				startPosition = new Vector2Int(0, (-height / 2));

				for (int i = 0; i < numberOfChunks; i++)
				{
					GameObject chunk = Instantiate(tileChunk, grid.transform);
					
					chunks.Add(chunk.GetComponent<Tilemap>());
					GenerateMainMap(mainMap, startPosition, width, height, true, chunks[i]);

					grassOverWorldChunk.GenerateGrassPlanetOverworldChunk(chunks[i]);
					chunks[i].transform.position = new Vector2(chunks[i].transform.position.x + startPosition.x, chunks[i].transform.position.y);
					startPosition.x += width - 1;
					chunks[i].GetComponent<TilemapCollider2D>().maximumTileChangeCount = 100;
					chunk.name = "tilemap_" + i;
				}
				chunkHeightOffset = grassOverWorldChunk.GetChunkHeightOffset;
				startPosition = new Vector2Int(0, 0);
				startPosition.y = -height + 1;
				int overWorldChunks = chunks.Count;
				int offset;
				for (int i = overWorldChunks; i < numberOfChunks + overWorldChunks; i++)
				{
					if (i - overWorldChunks != 0)
					{
						offset = i - overWorldChunks - 1;
						startPosition.y = startPosition.y + chunkHeightOffset[offset];
					}
					GameObject chunk = Instantiate(tileChunk, grid.transform);
					
					chunks.Add(chunk.GetComponent<Tilemap>());
					GenerateMainMap(mainMap, startPosition, width, height, true, chunks[i]);

					grassUnderWorldChunk.GenerateGrassPlanetUnderWorldChunk(chunks[i]);
					chunks[i].transform.position = new Vector2(chunks[i].transform.position.x + startPosition.x, chunks[i].transform.position.y + startPosition.y);
					startPosition.x += width - 1;
					chunks[i].GetComponent<TilemapCollider2D>().maximumTileChangeCount = 100;
					chunk.name = "tilemap_" + i;
				}
				Instantiate(worldWrappingTeleport, new Vector3(startPosition.x, height), quaternion.identity);
				Instantiate(worldWrappingTeleport, new Vector3(-1, height), quaternion.identity);
				//StartCoroutine(GenerateTrees());

				//SetPlayerSpawnLocation();

				StartCoroutine(SpawnPlayer());
				//StartCoroutine(SpawnEnemy());
				//SaveMap();
				break;
			case TypeOfPlanet.meme:
				break;
			default:
				break;
		}
	}

	private void SaveMap()
	{
		//gameTiles.index = 0;
		//for(int i = 0; i < chunks.Count; i++)
  //      {
		//	gameTiles.GetWorldTiles(chunks[i], true);
		//	gameTiles.index++;
  //      }
	
	}

	private void LoadMap()
    {
		GameObject grid = GameObject.Find("Grid");
        for (int i = 0; i < numberOfChunks * 2; i++)
        {
			GameObject chunk = Instantiate(tileChunk, grid.transform);
			Tilemap tm = chunk.GetComponent<Tilemap>();
			chunks.Add(tm);

			gameTiles.LoadWorldTiles(tm, i, tiles);

			chunk.GetComponent<TilemapCollider2D>().maximumTileChangeCount = 100;
		}
	}

    #region
    void CreateFeatures(MapSettings mapSettings, MapSettings otherSettings, Tilemap tilemap)
    {

			//int randomX = UnityEngine.Random.Range(-width / 2, width / 2);
		int randomX = 0;
		int randomY = 0;
		int randomWidth = width;
		int randomHeight = height / 10;


        //if (randomX > (widthPosition / 2 - 20))
        //{
        //	randomX -= width / 100;
        //	randomWidth = width / 100 - 1;
        //}

        //if (randomY > (height / 2 - 20))
        //{
        //    randomY -= (height / 100);
        //    randomHeight = height / 100 - 1;
        //}

        //if (randomX + randomWidth >= widthPosition / 2)
        //{
        //	int checker = (randomX + randomWidth - width / 2) - 1;
        //	randomWidth += checker;

        //}

        //if (randomY + randomHeight >= height / 2)
        //{
        //    int checker = (randomY + randomHeight - height / 2) - 1;
        //    randomHeight += checker;
        //}

        int numberOfLoops = 10; // Always 10 loops
		int amountToFill = mapSettings.fillAmount;

          GenerateMapFeatures(mapSettings, new Vector2Int(randomX, randomY), randomWidth, randomHeight, false, amountToFill, 0, tilemap);

        for (int i = 0; i < numberOfLoops - 1; i++)
        {
            amountToFill += 3;
            GenerateMapFeatures(mapSettings, new Vector2Int(randomX, randomY += randomHeight - 2), randomWidth, randomHeight, false, amountToFill, 0, tilemap);
            GenerateMapFeatures(otherSettings, new Vector2Int(randomX, randomY), randomWidth, randomHeight, false, amountToFill, 0, tilemap);

        }
        amountToFill = mapSettings.fillAmount + mapSettings.fillAmount / 2;
        GenerateMapFeatures(mapSettings, new Vector2Int(randomX, 0), randomWidth, height, false, amountToFill, 0, tilemap);

    }

	void CreateTopTerrain(MapSettings mapSettings, MapSettings otherSettings, Tilemap tilemap)
	{
		
		//int randomX = UnityEngine.Random.Range(-width / 2, width / 2);
		int randomX = 0;
		int randomY = height - 1;
		int randomWidth = width;
		int randomHeight = height / 10;

		//if (randomX > (widthPosition / 2 - 20))
		//{
		//	randomX -= width / 100;
		//	randomWidth = width / 100 - 1;
		//}
		//if (randomX + randomWidth >= widthPosition / 2)
		//{
		//	int checker = (randomX + randomWidth - width / 2) - 1;
		//	randomWidth += checker;
		//}

		int numberOfLoops = 10; // Always 10 loops
		int minWalk = UnityEngine.Random.Range(2, 7);
		//int chanceForHill;
		int hillGenerate = randomX;
		float hillModifier = 0.01f;
		GenerateMapFeatures(mapSettings, new Vector2Int(randomX, randomY), randomWidth, randomHeight, true, minWalk, hillModifier, tilemap);

        for (int i = 0; i < numberOfLoops - 1; i++)
        {
            //chanceForHill = UnityEngine.Random.Range(0, 100);
            //hillGenerate += randomWidth / 10;
            //if (chanceForHill < 90)
            //{
            //	//UnityEngine.Random.Range(-width / 4, width / 4)
            //	GenerateMapFeatures(otherSettings, new Vector2Int(hillGenerate, (height / 2) - (height / 100) - 1), width / UnityEngine.Random.Range(16,24), UnityEngine.Random.Range(16, 24), true, 50, UnityEngine.Random.Range(0.02f, 0.04f));
            //}

            minWalk = UnityEngine.Random.Range(2, 7);
            GenerateMapFeatures(mapSettings, new Vector2Int(randomX, randomY += UnityEngine.Random.Range(0, 2)), randomWidth + 1, randomHeight, true, minWalk, hillModifier, tilemap);

        }

        //      for (int i = 0; i < numberOfLoops; i++)
        //      {
        //	chanceForHill = UnityEngine.Random.Range(0, 1);
        //	hillGenerate += randomWidth / 10;
        //	if (chanceForHill > 0)
        //	{
        //		//UnityEngine.Random.Range(-width / 4, width / 4)
        //		GenerateMapFeatures(otherSettings, new Vector2Int(hillGenerate, (height / 2) - 1), width / UnityEngine.Random.Range(16, 24), UnityEngine.Random.Range(16, 24), true, 50, UnityEngine.Random.Range(0.02f, 0.04f));
        //	}

        //}

    }


    void CreateCaves(MapSettings mapSettings, Tilemap tilemap)
	{

		int	numberOfLoops = UnityEngine.Random.Range((width + height) / 40, (width + height) / 20); // This becomes 66 caves in a 500x500 chunk
		

		for (int i = 0; i < numberOfLoops; i++)
		{
		
			int randomY = UnityEngine.Random.Range(0, height / 2);
			int randomHeight = height / 2;
			int randomX = UnityEngine.Random.Range(0, width / 2);
			int randomWidth = width / 2;


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




			GenerateMainMap(mapSettings, new Vector2Int(randomX, randomY), randomWidth, randomHeight, false, tilemap);
            //MapSettings newMapSettings = new MapSettings();
            //newMapSettings = mapSettings;
            //newMapSettings.clearAmount = mapSettings.clearAmount / 2;
            //GenerateMainMap(newMapSettings, new Vector2Int(randomX - (randomWidth / 2) - 1, randomY), randomWidth, randomHeight, false);
            //GenerateMainMap(newMapSettings, new Vector2Int(randomX + (randomWidth / 2) + 1, randomY), randomWidth, randomHeight, false);
            //GenerateMainMap(newMapSettings, new Vector2Int(randomX, randomY - (randomHeight / 2) - 1), randomWidth, randomHeight, false);
            //GenerateMainMap(newMapSettings, new Vector2Int(randomX, randomY + (randomHeight / 2) + 1), randomWidth, randomHeight, false);

            //int randomValue = i > 0 ? UnityEngine.Random.Range(0, 12) : 0;
            //Vector2Int newStartPosition = new Vector2Int(randomValue, -randomValue);

            //GenerateMap(mapSettingsLeft, startPosition - newStartPosition,newWidth + newStartPosition.x,newHeight - newStartPosition.y);
            //startPosition.x += newWidth - 1;
            ////GenerateMap(mapSettingsRight, startPosition, newWidth, newHeight);
            ////startPosition.x += newWidth - 1;
            //randomized = UnityEngine.Random.Range(0f, 15f);
        }
	}

    #endregion

	//[Server]
	private void SetPlayerSpawnLocation()
    {
		RaycastHit2D playerSpawn = Physics2D.Raycast(new Vector2(UnityEngine.Random.Range(0, startPosition.x), height * 2), Vector2.down);

		if (playerSpawn.collider.gameObject.CompareTag("TileMap"))
		{
			if (NetworkManager.startPositions.Count > 0)
				NetworkManager.startPositions[0].position = playerSpawn.point;
			else
            {
				GameObject start = GameObject.Find("StartPosition");
				start.transform.position = playerSpawn.point;
				NetworkManager.RegisterStartPosition(start.transform);
			}
			GameObject drill = Instantiate(drillLaser, new Vector3(playerSpawn.point.x, playerSpawn.point.y + 10), quaternion.identity);

			NetworkServer.Spawn(drill);
		}
	}
	private IEnumerator SpawnPlayer()
    {
		bool playerSpawned = false;

		while(!playerSpawned)
        {
			yield return new WaitForSeconds(0.5f);

			RaycastHit2D playerSpawn = Physics2D.Raycast(new Vector2(UnityEngine.Random.Range(0, startPosition.x), height * 2), Vector2.down);

			if(playerSpawn.collider.gameObject.CompareTag("TileMap"))
            {
				if (NetworkManager.startPositions.Count > 0)
					NetworkManager.startPositions[0].position = playerSpawn.point + Vector2.up;
				else
				{
					GameObject start = GameObject.Find("StartPosition");
					start.transform.position = playerSpawn.point + Vector2.up;
					NetworkManager.RegisterStartPosition(start.transform);
				}
				GameObject drill = Instantiate(drillLaser, playerSpawn.point + Vector2.up * 3, quaternion.identity);
				drill.GetComponent<Rigidbody2D>().simulated = true;
				NetworkServer.Spawn(drill);

				playerSpawned = true;
			}
		}
	}

	private IEnumerator SpawnEnemy()
	{
		bool enemySpawned = false;

		yield return new WaitForSeconds(0.8f);

		if (!enemySpawned)
		{
			RaycastHit2D enemySpawn = Physics2D.Raycast(new Vector2(UnityEngine.Random.Range(0, startPosition.x), height * 2), Vector2.down);

			if (enemySpawn.collider.gameObject.CompareTag("TileMap"))
			{
				Instantiate(evilBeavisBase, new Vector3(enemySpawn.point.x, enemySpawn.point.y), quaternion.identity);
				Instantiate(evilBeavis, new Vector3(enemySpawn.point.x, enemySpawn.point.y + 10), quaternion.identity);
				enemySpawned = true;
			}
		}

	}

	private IEnumerator GenerateTrees()
	{
		int numberOfTrees = UnityEngine.Random.Range(0,100);

		yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < numberOfTrees; i++)
        {
			RaycastHit2D treeSpawn = Physics2D.Raycast(new Vector2(UnityEngine.Random.Range(0, (int)startPosition.x), height * 2), Vector2.down);
			Debug.Log("TreeSpawned");
			if (treeSpawn.collider.gameObject.CompareTag("TileMap"))
			{

				treeSpawn.collider.gameObject.GetComponent<Tilemap>().SetTile(treeSpawn.collider.gameObject.GetComponent<Tilemap>().WorldToCell(treeSpawn.point), tiles[3]);

			}
		}	

	}

	[ExecuteInEditMode]
	public void GenerateMap(List<MapSettings> mapSettings, Vector2Int startPosition, int width, int height, Tilemap tilemap)
	{
		//ClearMap();
		//Debug.Log(startPosition);
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
				case Algorithm.Default:
					//First generate our array
					map = MapFunctions.GenerateArray(width, height, true);
					break;
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
					map = MapFunctions.RandomWalkCaveCustom(map, seed, mapSettings[i].clearAmount, 100);
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
	public int[,] GenerateMainMap(MapSettings mapSettings, Vector2Int startPosition, int width, int height, bool addTiles, Tilemap tilemap) 
	{
		//ClearMap();
		
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
			case Algorithm.Default:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, false);
				break;
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
				map = MapFunctions.RandomWalkCaveCustom(map, seed, mapSettings.clearAmount, customCaveLoops);
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

        if (!addTiles)
        {
            MapFunctions.RenderMapWithOffset(map, tilemap, tiles[0], startPosition, addTiles);
        }
        else
        {
            MapFunctions.RenderMap(map, tilemap, tiles);
        }
		return map;
		//	StartCoroutine(MapFunctions.RenderMapWithDelay(map, tilemap, tiles[0], startPosition, addTiles));
	}

	[ExecuteInEditMode]
	public void GenerateMapFeatures(MapSettings mapSettings, Vector2Int startPosition, int width, int height, bool addTiles, int fillAmount, float modifier, Tilemap tilemap)
	{
		//ClearMap();

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
			case Algorithm.Default:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, false);
				break;
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
				map = MapFunctions.PerlinNoiseCave(map, modifier, mapSettings.edgesAreWalls);
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
				map = MapFunctions.RandomWalkTopSmoothed(map, seed, 3, 0);
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
				map = MapFunctions.RandomWalkCaveCustom(map, seed, fillAmount, customCaveLoops);
				break;
			case Algorithm.CellularAutomataVonNeuman:
				//First generate the cellular automata array
				map = MapFunctions.GenerateCellularAutomata(width, height, seed, fillAmount, mapSettings.edgesAreWalls);
				//Next smooth out the array using the von neumann rules
				map = MapFunctions.SmoothVNCellularAutomata(map, mapSettings.edgesAreWalls, mapSettings.smoothAmount);
				break;
			case Algorithm.CellularAutomataMoore:
				//First generate the cellular automata array
				map = MapFunctions.GenerateCellularAutomata(width, height, seed, fillAmount, mapSettings.edgesAreWalls);
				//Next smooth out the array using the Moore rules
				map = MapFunctions.SmoothMooreCellularAutomata(map, mapSettings.edgesAreWalls, mapSettings.smoothAmount);
				break;
			case Algorithm.DirectionalTunnel:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, false);
				//Next generate the tunnel through the array
				map = MapFunctions.DirectionalTunnel(map, fillAmount / 5, fillAmount / 2, fillAmount / 10, mapSettings.roughness, mapSettings.windyness);
				break;
			case Algorithm.BlockWalkGeneration:
				//First generate our array
				map = MapFunctions.GenerateArray(width, height, false);
				//Next generate the custom random walk cave
				map = MapFunctions.BlockWalkGeneration(map, seed, mapSettings.clearAmount, customCaveLoops);
				break;

		}

		if (!addTiles)
        {
			MapFunctions.RenderFeaturesWithOffset(map, tilemap, tiles, startPosition, addTiles);
		}
		else if (modifier == 50)
        {
			MapFunctions.RenderFeaturesWithOffsetNoNull(map, tilemap, tiles, startPosition, !addTiles);
		}
		else
        {
			MapFunctions.RenderMapWithOffset(map, tilemap, tiles[2], startPosition, addTiles);
		}
		
		//	StartCoroutine(MapFunctions.RenderMapWithDelay(map, tilemap, tiles[0], startPosition, addTiles));
	}

	public void ClearMap()
	{
		//tilemap.ClearAllTiles();
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
