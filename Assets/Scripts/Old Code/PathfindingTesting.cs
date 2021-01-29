using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingTesting : MonoBehaviour
{
    private void Start()
    {
        int width = 100;
        int height = 100;
        PathfindingDots pathfindingDots = new PathfindingDots(width, height, Vector3.zero);

        Vector3Int startPos = Vector3Int.zero;
        Vector3Int endPos = new Vector3Int(50, 50, 0);
        Debug.Log("init");
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        stopwatch.Start();
        List<Vector3> path1 = PathfindingDots.Instance.FindPath(startPos, endPos);
        stopwatch.Stop();

        //System.TimeSpan timeTaken = stopwatch.Elapsed;
        //Debug.Log("Time taken dots: " + timeTaken.ToString(@"m\:ss\.fff"));

        //Pathfinding pathfinding = new Pathfinding(width, height, Vector3.zero);
        //stopwatch.Start();
        //List<Vector3> path2 = Pathfinding.Instance.FindPath(startPos, endPos);
        //stopwatch.Stop();

        //timeTaken = stopwatch.Elapsed;
        //Debug.Log("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));

        //for (int i = 0; i < path1.Count; i++)
        //{
        //    Debug.Log(i + " path1: " + path1[i] + " path2: " + path2[i]);
        //}
    }

    private void Update()
    {
        //Vector3[] startPosList = new Vector3[1000];
        //for (int i = 0; i < 1000; i++)
        //{
        //    int x = Random.Range(0, 1000);
        //    int y = Random.Range(0, 1000);

        //    startPosList[i] = new Vector3(x, y);
        //}

        //Vector3[] endPosList = new Vector3[1000];
        //for (int i = 0; i < 1000; i++)
        //{
        //    int x = Random.Range(0, 1000);
        //    int y = Random.Range(0, 1000);

        //    endPosList[i] = new Vector3(x, y);
        //}

        
    }
}