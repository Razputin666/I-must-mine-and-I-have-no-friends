using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public class RaycastTest : MonoBehaviour
{
    [SerializeField] private int range;
    [SerializeField] private LayerMask layer;

    private void Update()
    {
        NativeArray<RaycastHit> meme = new NativeArray<RaycastHit>(1, Allocator.TempJob);
        NativeArray<RaycastCommand> prank = new NativeArray<RaycastCommand>(100, Allocator.TempJob);

        Vector3 origin = transform.position;

        
        for (int i = 0; i < prank.Length; i++)
        {
            Vector3 direction = new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100), 0);
            prank[i] = new RaycastCommand(origin, direction, range, layer);
            Debug.DrawRay(origin, direction, Color.red, 0.2f);
        }
        
        JobHandle handle = RaycastCommand.ScheduleBatch(prank, meme, 1, default(JobHandle));

        handle.Complete();

        RaycastHit bruh = meme[0];
        //Debug.Log(bruh.collider.gameObject.transform.position);
        //Debug.DrawLine(transform.position, transform.position - direction, Color.red, 0.3f);

        meme.Dispose();
        prank.Dispose();
    }
}

public struct RayCastTestJob : IJobParallelFor
{
    public void Execute(int index)
    {
        throw new System.NotImplementedException();
    }
}
