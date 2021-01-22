using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PathfindingTesting : MonoBehaviour
{
    private Pathfinder pathfinder;

    private void Start()
    {
        Pathfinding pathfinding = new Pathfinding(10, 10, Vector3.zero);
    }

    private void Update()
    {
        Vector3[] startPosList = new Vector3[1000];
        for (int i = 0; i < 1000; i++)
        {
            int x = Random.Range(0, 1000);
            int y = Random.Range(0, 1000);

            startPosList[i] = new Vector3(x, y);
        }

        Vector3[] endPosList = new Vector3[1000];
        for (int i = 0; i < 1000; i++)
        {
            int x = Random.Range(0, 1000);
            int y = Random.Range(0, 1000);

            endPosList[i] = new Vector3(x, y);
        }

        Stopwatch stopWatch = new Stopwatch();

        stopWatch.Start();
        for(int i = 0; i < 1000; i++)
        {
            pathfinder.CalculatePath(startPosList[i], endPosList[i]);
        }
        stopWatch.Stop();

        UnityEngine.Debug.Log("Time: " + stopWatch.ElapsedMilliseconds / 1000);
    }
}