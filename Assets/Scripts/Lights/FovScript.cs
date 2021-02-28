using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FovScript : NetworkBehaviour
{
    [SerializeField] private LayerMask layermask;
    private Mesh mesh;
    private Vector3 origin;
    private float startingAngle;
    private float fov;
    

    // Start is called before the first frame update
    //private void Start()
    //{
    //    mesh = new Mesh();
    //    GetComponent<MeshFilter>().mesh = mesh;
    //    fov = 90f;
    //    origin = Vector3.zero;

    //}
    //private void LateUpdate()
    //{
        
    
    
    //    int rayCount = 50;
    //    float angle = startingAngle + 90f;
    //    float angleIncrease = fov / rayCount;
    //    float viewDistance = 20f;


    //    Vector3[] vertices = new Vector3[rayCount + 1 + 1];
    //    Vector2[] uv = new Vector2[vertices.Length];
    //    int[] triangles = new int[rayCount * 3];

        

    //    vertices[0] = origin;

    //    int vertexIndex = 1;
    //    int triangleIndex = 0;

    //    for (int i = 0; i < rayCount; i++)
    //    {
    //        Vector3 vertex;
            
    //        RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, GetVectorFromAngle(angle), viewDistance, layermask);

    //        if (raycastHit2D.collider == null)
    //        {
    //            vertex = origin + GetVectorFromAngle(angle) * viewDistance;
    //        }
    //        else
    //        {
    //            vertex = raycastHit2D.point;
    //        }

    //        vertices[vertexIndex] = vertex;

    //        if (i > 0)
    //        {

            
    //        triangles[triangleIndex + 0] = 0;
    //        triangles[triangleIndex + 1] = vertexIndex - 1;
    //        triangles[triangleIndex + 2] = vertexIndex;

    //        triangleIndex += 3;
    //        }
    //        vertexIndex++;

    //        angle -= angleIncrease;
    //    }
    //    //vertices[1] = new Vector3(50, 0);
    //    //vertices[2] = new Vector3(0, -50);

    //    //triangles[0] = 0;
    //    //triangles[1] = 1;
    //    //triangles[2] = 2;

    //    mesh.vertices = vertices;
    //    mesh.uv = uv;
    //    mesh.triangles = triangles;
    //}

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin + new Vector3(0, 2);
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        startingAngle = GetAngleFromVectorFloat(aimDirection) - fov / 2f;
    }

    private Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    private float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0)
        {
            n += 360;
        }

        return n;
    }

}
