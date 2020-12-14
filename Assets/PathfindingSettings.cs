using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathfindingSettings : MonoBehaviour
{
    [SerializeField] private AstarPath pathfinder;
   // [SerializeField] private GridGraph gridGraph;
    [SerializeField] private LevelGeneratorLayered mapGen;
    [SerializeField] private ProceduralGridMover gridMover;
    //private AstarData data;
    float timer;
    public void prank()
    {


       // GridGraph gridGraph = pathfinder.data.AddGraph(typeof(GridGraph)) as GridGraph;
       GridGraph gridGraph = pathfinder.data.AddGraph(typeof(GridGraph)) as GridGraph;
        
        gridGraph.SetDimensions(30, 30, 1);
        gridMover.enabled = true;
        StartCoroutine(ScanPathFinding());
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;

        if(timer > 5f)
        {
            AstarData.active.Scan();
            timer = 0f;
        }
    }

    private IEnumerator ScanPathFinding()
    {
        yield return new WaitForSeconds(1f);
        
        gridMover.target = GameObject.FindGameObjectWithTag("Enemy").transform;
        AstarData.active.Scan();
    }

}
