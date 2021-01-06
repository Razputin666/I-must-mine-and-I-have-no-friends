using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

public class PathfindingScript : MonoBehaviour
{
   [SerializeField] private Vector3 target;
    public float nextWayPointDistance;
    private EnemyController enemy;

    private Path path;
    private int currentWayPoint = 0;
    private bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb2d;

    private void Start()
    {
        enemy = GetComponent<EnemyController>();
        seeker = GetComponent<Seeker>();
        rb2d = GetComponent<Rigidbody2D>();

        InvokeRepeating("PathSeeking", 1f, 1f);
    }

    private void FixedUpdate()
    {
        if (enemy.target != null)
        {
            PathfindingTarget = new Vector3(Mathf.Clamp(enemy.target.x, transform.position.x - 20f, transform.position.x + 20f), Mathf.Clamp(enemy.target.y, transform.position.y - 20f, transform.position.y + 20f));

           // Debug.Log(target + " target");
        }
        if (path == null)
        {
            return;
        }
        if (currentWayPoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

    }

    private void PathSeeking()
    {
        seeker.StartPath(rb2d.position, target, OnPathComplete);
    }

    private void OnPathComplete(Path path)
    {
        if (!path.error)
        {
            this.path = path;
            currentWayPoint = 0;
        }
    }

    public Vector3 PathfindingTarget
    {
        get { return target; }

        set { target = value; }
    }


}
