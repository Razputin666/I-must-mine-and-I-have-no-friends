using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public class BaseBehaviour : NetworkBehaviour, HasCoolDownInterFace
{
    protected List<GameObject> minions;
    [SerializeField] private GameObject minion;
    protected List<Vector3> oreList;
    [SerializeField] private int oreSearchDistance;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private CoolDownSystem coolDownSystem;
    [SerializeField] private int id = 5;
    public int Id => id;

    public float CoolDownDuration => coolDownDuration;

    protected int blockAmount;
    protected int oreAmount;

    public override void OnStartServer()
    {
        InitBase();
    }
    protected void InitBase()
    {
        blockAmount = 0;
        oreAmount = 0;
        minions = new List<GameObject>();
        oreList = new List<Vector3>();
        SpawnMinion();

        StartCoroutine(UpdateOreList());
    }

    private void Update()
    {
        if (!isServer)
            return;

        if(!coolDownSystem.IsOnCoolDown(Id))
        {
            MinionSpawnCheck();
        }
    }

    private void LateUpdate()
    {
        if (!isServer)
            return;

        //PathfindingDots.Instance.CompleteJobs();
    }
    /// <summary>
    /// Check if the base has enough ore or blocks to spawn another minion
    /// </summary>
    private void MinionSpawnCheck()
    {
        //Check if we have reached our max minions;
        if (minions.Count >= 10)
            return;

        //Spawn a minion if we have enough ore or blocks
        if(oreAmount >= 50)
        {
            oreAmount -= 50;
            SpawnMinion();
        }
        else if(blockAmount >= 500)
        {
            blockAmount -= 500;
            SpawnMinion();
        }
    }
    /// <summary>
    /// Update our Ore list every 30 seconds as long as there is still ore around
    /// </summary>
    protected IEnumerator UpdateOreList()
    {
        yield return null;

        do
        { 
            SearchForOre();

            yield return new WaitForSeconds(30f);
        } 
        while (oreList.Count > 0);
    }
    /// <summary>
    /// Search for ore around the base and add their position to a list
    /// </summary>
    protected void SearchForOre()
    {
        oreList = new List<Vector3>();
        Vector2Int startSearch = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        Unity.Collections.NativeArray<int> worldArray = TileMapManager.Instance.worldArray;
        for (int x = startSearch.x - oreSearchDistance; x < startSearch.x + oreSearchDistance; x++)
        {
            for (int y = startSearch.y - oreSearchDistance; y < startSearch.y + oreSearchDistance; y++)
            {
                int index = x * 50 + y;
                if (
                    worldArray[index] == (int)BlockTypeConversion.IronBlock ||
                    worldArray[index] == (int)BlockTypeConversion.GoldBlock ||
                    worldArray[index] == (int)BlockTypeConversion.CopperBlock)
                {
                    oreList.Add(new Vector3(x, y, 0));
                }
            }
        }

        //oreList = oreList.OrderBy(x => Vector3.Distance(transform.position, x)).ToList();
    }

    /// <summary>
    /// Returns the closest ore location
    /// </summary>
    public Vector3 GetOreTarget(Vector3 unitPosition)
    {
        Utils.Timer.StartTimer("GetOreTarget");

        List<Vector3> tempOreList = oreList.OrderBy(x => Vector3.Distance(unitPosition, x)).ToList();

        Utils.Timer.StopTimer("GetOreTarget");
        Utils.Timer.PrintTimer("GetOreTarget");
        if(tempOreList.Count > 0)
            return tempOreList[0];

        return Vector3.zero;
    }
    /// <summary>
    /// Spawn a minion on the base location
    /// </summary>
    protected void SpawnMinion()
    {
        GameObject newMinion = Instantiate(minion, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
        NetworkServer.Spawn(newMinion);

        minions.Add(newMinion);
    }

    public void AddOre(int ores)
    {
        oreAmount += ores;
    }

    public void AddBlocks(int blocks)
    {
        blockAmount += blocks;
    }
}
