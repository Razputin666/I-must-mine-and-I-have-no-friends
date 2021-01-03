//using Mirror;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class PlayerSpawnSystem : NetworkBehaviour
//{
//    [SerializeField] private GameObject playerPrefab = null;

//    private static List<Transform> spawnPoints = new List<Transform>();

//    public static void AddSpawnPoint(Transform transform)
//    {
//        spawnPoints.Add(transform);

//        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
//    }

//    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

//    [Server]
//    public void SpawnPlayer(NetworkConnection conn)
//    {
//        Transform spawnPoint = spawnPoints.ElementAtOrDefault(0);

//        if(spawnPoint == null)
//        {
//            Debug.LogError($"Missing spawn point for player");
//            return;
//        }

//        GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[0].position, Quaternion.identity);
//        NetworkServer.Spawn(playerInstance, conn);
//    }
//}