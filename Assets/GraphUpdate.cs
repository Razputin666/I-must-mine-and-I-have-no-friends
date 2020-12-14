using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GraphUpdate : MonoBehaviour
{
    float timer = 0;
    [SerializeField] private GraphUpdateScene graphUpdate;
    private AstarPath pathfinding;
    // Start is called before the first frame update
    void Start()
    {
        pathfinding = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AstarPath>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 5f)
        {
            Bounds bounds = GetComponent<Collider>().bounds;
            Bounds boonds = graphUpdate.GetBounds();
            GraphUpdateObject guo = new GraphUpdateObject(boonds);

            // Set some settings
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo);
            
            timer = 0f;
        }
    }
}
