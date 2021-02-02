using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingTesting : MonoBehaviour
{
    private void Start()
    {
        int width = 4000;
        int height = 4000;

        PathfindingDots pathfindingDots = new PathfindingDots(width, height, Vector3.zero);
        //Pathfinding pathfinding = new Pathfinding(width, height, Vector3.zero);
        Pathfind(true);
    }

    private void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        PathfindingDots.Instance.OnExit();
    }

    private void Pathfind(bool debug)
    {
        Vector3Int startPos = Vector3Int.zero;
        Vector3Int endPos = new Vector3Int(500, 0, 0);

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        stopwatch.Start();
        List<Vector3> path1 = PathfindingDots.Instance.FindPath(startPos, endPos);
        stopwatch.Stop();

        System.TimeSpan timeTaken = stopwatch.Elapsed;
        Debug.Log("Time taken dots: " + timeTaken.ToString(@"m\:ss\.fff"));

        //stopwatch.Start();
        //List<Vector3> path2 = Pathfinding.Instance.FindPath(startPos, endPos);
        //stopwatch.Stop();

        //timeTaken = stopwatch.Elapsed;
        //Debug.Log("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
        if(debug)
        {
            //Debug.Log(path1.Count + ", " + path2.Count);
            for (int i = 0; i < path1.Count; i++)
            {
                Debug.Log(i + " path1: " + path1[i]);// + " path2: " + path2[i]);
            }
        }
        
    }
}