using UnityEngine;
using System.Collections.Generic;

public class PolygonGenerator : MonoBehaviour
{
    // This first list contains every vertex of the mesh that we are going to render
    public List<Vector3> newVertices = new List<Vector3>();
    // The triangles tell Unity how to build each section of the mesh joining
    // the vertices
    public List<int> newTriangles = new List<int>();
    // The UV list is unimportant right now but it tells Unity how the texture is
    // aligned on each polygon
    public List<Vector2> newUV = new List<Vector2>();

    public List<Vector3> colVertices = new List<Vector3>();
    public List<int> colTriangles = new List<int>();
    private int colCount;

    // A mesh is made up of the vertices, triangles and UVs we are going to define,
    // after we make them up we'll save them as this mesh
    private Mesh mesh;
    private MeshCollider col;

    private float tUnit = 0.25f;
    private Vector2 tStone = new Vector2(1, 0);
    private Vector2 tGrass = new Vector2(0, 1);
    private Vector2 tDirt = new Vector2(1, 1);

    public byte[,] blocks;
    private int squareCount;
    public bool update = false;

    [SerializeField]
    private int mapWidth = 96;
    [SerializeField]
    private int mapHeight = 128;

    // Use this for initialization
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        col = GetComponent<MeshCollider>();

        GenTerrain();
        BuildMesh();
        UpdateMesh();
    }

    void Update()
    {
        if (update)
        {
            BuildMesh();
            UpdateMesh();
            update = false;
        }
    }


    int NoiseInt(int x, int y, float scale, float mag, float exp)
    {
        return (int)(Mathf.Pow((Mathf.PerlinNoise(x / scale, y / scale) * mag), (exp)));
    }

    void GenTerrain()
    {
        blocks = new byte[mapWidth, mapHeight];

        for (int px = 0; px < blocks.GetLength(0); px++)
        {
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

            int grass = NoiseInt(px, 0, 100f, 100, 1);
            grass += NoiseInt(px, 1, 50, 99, 1);
            grass += 75;

            for (int py = 0; py < blocks.GetLength(1); py++)
            {
                if (py < stone)
                {
                    blocks[px, py] = 1;
                    //The next three lines make dirt spots in random places
                    if (NoiseInt(px, py, 12, 16, 1) > 10)
                    {  //dirt spots
                        blocks[px, py] = 2;
                    }
                    //The next three lines remove dirt and rock to make caves in certain places
                    if (NoiseInt(px, py * 2, 16, 14, 1) > 10)
                    { //Caves
                        blocks[px, py] = 0;

                    }
                }
                else if (py < dirt)
                {
                    blocks[px, py] = 2;
                }
                else if (py < grass)
                {
                    blocks[px, py] = 3;
                }
            }
        }
    }

    void BuildMesh()
    {
        for (int px = 0; px < blocks.GetLength(0); px++)
        {
            for (int py = 0; py < blocks.GetLength(1); py++)
            {
                if (blocks[px, py] != 0)
                {
                    GenCollider(px, py);

                    if (blocks[px, py] == 1)
                    {
                        GenSquare(px, py, tStone);
                    }
                    else if (blocks[px, py] == 2)
                    {
                        GenSquare(px, py, tDirt);
                    }
                    else if(blocks[px, py] == 3)
                    {
                        GenSquare(px, py, tGrass);
                    }
                }
                //End air block check
            }
        }
    }

    byte Block(int x, int y)
    {

        if (x == -1 || x == blocks.GetLength(0) || y == -1 || y == blocks.GetLength(1))
        {
            return (byte)1;
        }

        return blocks[x, y];
    }

    void GenCollider(int x, int y)
    {
        //Top
        if (Block(x, y + 1) == 0)
        {
            colVertices.Add(new Vector3(x, y, 1));
            colVertices.Add(new Vector3(x + 1, y, 1));
            colVertices.Add(new Vector3(x + 1, y, 0));
            colVertices.Add(new Vector3(x, y, 0));

            ColliderTriangles();

            colCount++;
        }

        //bot
        if (Block(x, y - 1) == 0)
        {
            colVertices.Add(new Vector3(x, y - 1, 0));
            colVertices.Add(new Vector3(x + 1, y - 1, 0));
            colVertices.Add(new Vector3(x + 1, y - 1, 1));
            colVertices.Add(new Vector3(x, y - 1, 1));

            ColliderTriangles();
            colCount++;
        }

        //left
        if (Block(x - 1, y) == 0)
        {
            colVertices.Add(new Vector3(x, y - 1, 1));
            colVertices.Add(new Vector3(x, y, 1));
            colVertices.Add(new Vector3(x, y, 0));
            colVertices.Add(new Vector3(x, y - 1, 0));

            ColliderTriangles();

            colCount++;
        }

        //right
        if (Block(x + 1, y) == 0)
        {
            colVertices.Add(new Vector3(x + 1, y, 1));
            colVertices.Add(new Vector3(x + 1, y - 1, 1));
            colVertices.Add(new Vector3(x + 1, y - 1, 0));
            colVertices.Add(new Vector3(x + 1, y, 0));

            ColliderTriangles();

            colCount++;
        }

    }

    void ColliderTriangles()
    {
        colTriangles.Add(colCount * 4);
        colTriangles.Add((colCount * 4) + 1);
        colTriangles.Add((colCount * 4) + 3);
        colTriangles.Add((colCount * 4) + 1);
        colTriangles.Add((colCount * 4) + 2);
        colTriangles.Add((colCount * 4) + 3);
    }


    void GenSquare(int x, int y, Vector2 texture)
    {
        newVertices.Add(new Vector3(x, y, 0));
        newVertices.Add(new Vector3(x + 1, y, 0));
        newVertices.Add(new Vector3(x + 1, y - 1, 0));
        newVertices.Add(new Vector3(x, y - 1, 0));

        newTriangles.Add(squareCount * 4);
        newTriangles.Add((squareCount * 4) + 1);
        newTriangles.Add((squareCount * 4) + 3);
        newTriangles.Add((squareCount * 4) + 1);
        newTriangles.Add((squareCount * 4) + 2);
        newTriangles.Add((squareCount * 4) + 3);

        newUV.Add(new Vector2(tUnit * texture.x, tUnit * texture.y + tUnit));
        newUV.Add(new Vector2(tUnit * texture.x + tUnit, tUnit * texture.y + tUnit));
        newUV.Add(new Vector2(tUnit * texture.x + tUnit, tUnit * texture.y));
        newUV.Add(new Vector2(tUnit * texture.x, tUnit * texture.y));

        squareCount++;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();

        newVertices.Clear();
        newTriangles.Clear();
        newUV.Clear();
        squareCount = 0;

        Mesh newMesh = new Mesh();
        newMesh.vertices = colVertices.ToArray();
        newMesh.triangles = colTriangles.ToArray();
        col.sharedMesh = newMesh;

        colVertices.Clear();
        colTriangles.Clear();
        colCount = 0;
    }
}
