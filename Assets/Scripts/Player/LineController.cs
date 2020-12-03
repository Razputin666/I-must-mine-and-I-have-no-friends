using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{

    private LineRenderer lr;
    private Transform[] points;
    private MiningController miningLaser;
    private Vector3[] laserPoints;
    private PlayerController player;
    private float timer;

    // Start is called before the first frame update
    void Awake()
    {
        player = GetComponentInParent<FaceMouse>().GetComponentInParent<PlayerController>();
       miningLaser = gameObject.GetComponent<MiningController>();
        lr = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        lr.enabled = true;
    }

    private void OnDisable()
    {
        lr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 watever = new Vector3(player.worldPosition.x, player.worldPosition.y, 0);
        laserPoints = new Vector3[] { watever, miningLaser.endOfGun.position };


        // lr.SetPosition(0, miningLaser.endOfGun.position);
        lr.SetPositions(laserPoints);

        timer += Time.deltaTime;
        if(timer > 0.2f)
        {
            this.enabled = false;
            timer = 0f;
        }

    }
    public void SetUpLine(Transform[] points)
    {
        lr.positionCount = points.Length;
        this.points = points;
    }
}
