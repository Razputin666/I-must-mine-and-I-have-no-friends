using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Jobs;
using Unity.Physics;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager GetInstance()
    {
        return instance;
    }

    [SerializeField]
    private UnityEngine.Material[] materials;

    private EntityManager entityManager;

    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private float cellSize = 5f;
    [SerializeField]
    private int chunkWidth = 10;
    [SerializeField]
    private int chunkHeight = 10;

    TerrainGenerator.TILE_TYPES[,] generatedTiles;

    private void Awake()
    {
        instance = this;
    }

    TerrainGenerator terrainGenerator;
    private void Start()
    {
        terrainGenerator = new TerrainGenerator();
        terrainGenerator.GenerateTiles(width, height);

        generatedTiles = terrainGenerator.GetGeneratedTiles();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        NativeArray<Entity> entityArray = new NativeArray<Entity>(width * height, Allocator.Temp);

        //EntityArchetype entityArchetype = entityManager.CreateArchetype(
        //    typeof(RenderMesh),
        //    typeof(LocalToWorld),
        //    typeof(Translation),
        //    typeof(RenderBounds)
        //    );

        Mesh mesh = CreateMesh(cellSize, cellSize);
        
        //entityManager.CreateEntity(entityArchetype, entityArray);
        Unity.Mathematics.float3 originPos = Unity.Mathematics.float3.zero;
        int index = 0;
        for (int x = 0; x < width; x++)
        {
            StartCoroutine(CreateEntities(mesh, originPos, x, height));

            index += height;
            //for (int y = 0; y < height; y++)
            //{
            //    Unity.Mathematics.float3 startPos = new Unity.Mathematics.float3(x * cellSize, y * cellSize, 0) + originPos;

            //    Entity entity = entityArray[index++];

            //    int matIndex = (int)generatedTiles[x, y];

            //    Material material = materials[matIndex];

            //    entityManager.SetSharedComponentData(entity, new RenderMesh
            //    {
            //        mesh = mesh,
            //        material = material,
            //    });

            //    entityManager.SetComponentData(entity, new Translation
            //    {
            //        Value = startPos
            //    });
            //}
        }
        Debug.Log(index);
        entityArray.Dispose();
    }

    IEnumerator CreateEntities(Mesh mesh, Unity.Mathematics.float3 originPosition, int x, int height)
    {
        NativeArray<Entity> entityArray = new NativeArray<Entity>(height, Allocator.Temp);

        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(RenderBounds)
            );

        int index = 0;
        entityManager.CreateEntity(entityArchetype, entityArray);

        for (int y = 0; y < height; y++)
        {
            Unity.Mathematics.float3 startPos = new Unity.Mathematics.float3(x * cellSize, y * cellSize, 0) + originPosition;

            Entity entity = entityArray[index++];

            int matIndex = (int)generatedTiles[x, y];

            UnityEngine.Material material = materials[matIndex];

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                material = material,
            });

            entityManager.SetComponentData(entity, new Translation
            {
                Value = startPos
            });
        }

        yield return null;
    }

    private Mesh CreateMesh(float width, float height)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        /* 0, 0
         * 0, 1
         * 1, 1
         * 1, 0
         */
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        vertices[0] = new Vector3(-halfWidth, -halfHeight);
        vertices[1] = new Vector3(-halfWidth, halfHeight);
        vertices[2] = new Vector3(halfWidth, halfHeight);
        vertices[3] = new Vector3(halfWidth, -halfHeight);

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }
}
