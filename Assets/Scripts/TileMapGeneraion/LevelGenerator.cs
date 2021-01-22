//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Tilemaps;
//using Mirror;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Collections;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//public class LevelGenerator : NetworkBehaviour
//{
//	[Tooltip("The Tilemap to draw onto")]
//	[SerializeField] private GameObject tilemapPrefab;
//	[Tooltip("The Tile to draw (use a RuleTile for best results)")]
//	[SerializeField] private TileBase[] tiles;

//	[Tooltip("Width of our map")]
//	[SerializeField] private int tilemapChunkWidth;
//	[Tooltip("Height of our map")]
//	[SerializeField] private int tilemapChunkHeight;

//	[Tooltip("Number of chunks on the x")]
//	[SerializeField] private int numberOfChunksX;
//	[Tooltip("Number of chunks on the y")]
//	[SerializeField] private int numberOfChunksY;
//	//[Tooltip("The settings of our map")]
//	//[SerializeField]
//	//public MapSettings mapSetting;

//	[SyncVar]
//	private int mapSeed = 0;

//	public List<Tilemap> chunks = new List<Tilemap>();

//	public override void OnStartServer()
//	{
//		mapSeed = UnityEngine.Random.Range(1, 100000);

//		UnityEngine.Random.InitState(mapSeed);

//		InitialWorldGeneration();
//	}

//	private void InitialWorldGeneration()
//	{
//		InitChunks();
		

//		//Vector2 startPosition = new Vector2(startX, startY);


		
//	}

//	private void InitChunks()
//    {
//		GameObject grid = GameObject.Find("Grid");

//		float startX = -tilemapChunkWidth * (numberOfChunksX / 2);
//		float startY = -tilemapChunkHeight;

//		int tilemapIndex = 0;
		
//		for (int y = 0; y < numberOfChunksY; y++)
//		{
//			for (int x = 0; x < numberOfChunksX; x++)
//			{
//				GameObject chunk = Instantiate(tilemapPrefab, grid.transform);

//				Tilemap tilemap = chunk.GetComponent<Tilemap>();
//				chunk.transform.position = new Vector2(
//					startX + (tilemapChunkWidth * x) - 1,
//					startY + (tilemapChunkHeight * y) - 1); ;

//				chunk.name = "tilemap_" + tilemapIndex;

//				InitMap(chunk.GetComponent<Tilemap>());
//				chunk.GetComponent<ChunkSettings>().Init();

//				TileMapManager.Instance.AddTileChunk(chunk.GetComponent<Tilemap>());
//				chunks.Add(chunk.GetComponent<Tilemap>());

//				tilemapIndex++;
//			}
//		}
//	}

//	private void InitMap(Tilemap tilemap)
//    {
//		int[,] map = new int[tilemapChunkWidth, tilemapChunkHeight];

//		map = MapFunctions.GenerateArray(tilemapChunkWidth, tilemapChunkHeight, false);

//		MapFunctions.RenderMap(map, tilemap, tiles[2]); //Render with Dirt
//	}

//	private void InitTopLayer()
//    {
//		int[,] map = new int[tilemapChunkWidth, tilemapChunkHeight];

//		for (int x = 0; x < numberOfChunksX; x++)
//		{
//			Tilemap tilemap = chunks[x];

//			float seed = UnityEngine.Random.Range(0f, 1f);
//			int heighRandomize = UnityEngine.Random.Range(2, 7);

//			//First generate our array
//			map = MapFunctions.GenerateArray(tilemapChunkWidth, tilemapChunkHeight, true);
//			//Next generate the smoothed random top
//			map = MapFunctions.RandomWalkTopSmoothed(map, seed, 3, heighRandomize);

//		}
//		NativeArray<int> mapCoords = new NativeArray<int>(tilemapChunkWidth * tilemapChunkHeight, Allocator.TempJob);

//		GenerateArrayJob generateArrayJob = new GenerateArrayJob
//		{
//			mapList = mapCoords,
//			empty = true,
//		};

//		JobHandle jobHandle = generateArrayJob.Schedule(mapCoords.Length, 100);
//		jobHandle.Complete();

//        for (int x = 0; x < tilemapChunkWidth; x++)
//        {
//            for (int y = 0; y < tilemapChunkHeight; y++)
//            {
//				map[x, y] = mapCoords[x + tilemapChunkHeight * y];
//            }
//        }
//	}

//	public struct GenerateArrayJob : IJobParallelFor
//    {
//		public NativeArray<int> mapList;
//		public bool empty;
//        public void Execute(int index)
//        {
//			if (empty)
//			{
//				mapList[index] = 0;
//			}
//			else
//			{
//				mapList[index] = 1;
//			}
//		}
//    }
//}
