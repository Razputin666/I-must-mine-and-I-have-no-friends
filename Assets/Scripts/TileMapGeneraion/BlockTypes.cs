using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockTypes : MonoBehaviour
{
    [SerializeField] private TileBase[] blocks;
    private Dictionary<string, TileBase> tilebaseLookup;
    private void Start()
    {
        blocks = Resources.LoadAll<TileBase>("Tilebase");

        foreach (var tilebase in blocks)
        {
            tilebaseLookup.Add(tilebase.name, tilebase);
        }
    }
    public TileBase GetBlockType(string block)
    {
        //Dictionary<block, blocks> = new Dictionary<string, TileBase>();
        //switch (block)
        //{

        //    case "Dirt":
        //        return blocks[0];
        //    default:
        //        return blocks[0];

        //}
        return blocks[0];

    }
}
