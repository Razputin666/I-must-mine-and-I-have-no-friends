using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class WorldTile
{
    public Vec3Int localPlace;

    public Vec3 gridLocation;

    public string tileBase;

    public bool isExplored;

    public bool isVisible;
    public Vec3 transFormPos;
}

[System.Serializable]
public struct Vec3Int
{
    public Vec3Int(int _x, int _y, int _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
    public Vector3Int Vector3Int()
    {
        return new Vector3Int(x, y, z);
    }

    private int x;
    private int y;
    private int z;
}
[System.Serializable]
public struct Vec3
{
    public Vec3(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
    public Vector3 Vector3()
    {
        return new Vector3(x, y, z);
    }
    private float x;
    private float y;
    private float z;
}