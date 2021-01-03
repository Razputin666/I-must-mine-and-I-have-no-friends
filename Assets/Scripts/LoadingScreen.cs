using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.Tilemaps;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LoadingScreen : NetworkBehaviour
{
    [SerializeField] private TMP_Text loadingScreenProgressText = null;
    [SerializeField] private Image loadingScreenProgressBar = null;
    [SerializeField] private GameObject playerPrefab = null;
    private GameTiles gameTiles = null;
    private NetworkTransmitter networkTransmitter = null;
    
    private int messagesRecieved = 0;
    private int totalMessages = 0;
    private int tilemapsSynced = 0;

    //private System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();

    public void Awake()
    {
        if (gameTiles == null)
            gameTiles = GetComponent<GameTiles>();

        if (networkTransmitter == null)
            networkTransmitter = GetComponent<NetworkTransmitter>();
    }
    public override void OnStartLocalPlayer()
    {
        if (!isClientOnly)
        {
            StartCoroutine(SpawnPlayer(connectionToClient));
            return;
        }

        gameTiles.OnWorldTilesSet += OnTilemapSet;

        networkTransmitter.OnDataCompletelyReceived += MyCompletlyRecievedHandler;

        StartCoroutine(RequestTerrainData());
    }
  
    private IEnumerator SpawnPlayer(NetworkConnection conn)
    {
        yield return null;

        yield return null;

        //Spawn player
        GameObject playerInstance = Instantiate(playerPrefab, NetworkManager.startPositions[0].position, Quaternion.identity);
        
        //Destroy gameobject
        GameObject go = conn.identity.gameObject;

        NetworkServer.ReplacePlayerForConnection(conn, playerInstance, true);
        NetworkServer.Spawn(playerInstance, conn);

        NetworkServer.Destroy(go);
    }

    [Command]
    private void CmdMapSyncComplete()
    {
        //Spawn player
        GameObject playerInstance = Instantiate(playerPrefab, NetworkManager.startPositions[0].position, Quaternion.identity);

        //Destroy gameobject
        GameObject go = connectionToClient.identity.gameObject;

        NetworkServer.ReplacePlayerForConnection(connectionToClient, playerInstance, true);
        NetworkServer.Spawn(playerInstance, connectionToClient);

        NetworkServer.Destroy(go);
    }

    private IEnumerator RequestTerrainData()
    {
        yield return null;

        TileMapManager.Instance.InitTilemaps();

        CmdRequestTerrainData();
    }

    #region Server
    [Command(ignoreAuthority = true)]
    private void CmdRequestTerrainData()
    {
        Tilemap[] tilemaps = TileMapManager.Instance.Tilemaps.ToArray();
        RpcSendMessageCount(tilemaps.Length);

        SendMapData(connectionToClient, tilemaps);
    }

    [Server]
    private void SendMapData(NetworkConnection conn, Tilemap[] tilemaps)
    {
        for (int i = 0; i < tilemaps.Length; i++)
        {
            List<WorldTile> saveTiles = gameTiles.GetWorldTiles(tilemaps[i], false);
            byte[] tmByte = SerializeTileMap(saveTiles);
            networkTransmitter.SendBytesToClient(conn, i, tmByte);
        }
    }

    [Server]
    private byte[] SerializeTileMap(List<WorldTile> saveTiles)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();

        bf.Serialize(ms, saveTiles);
        ms.Close();

        return ms.ToArray();
    }
    #endregion

    [TargetRpc]
    private void RpcSendMessageCount(int messageCount)
    {
        totalMessages = messageCount;
        Debug.Log("messages: " + messageCount);
    }

    [Client]
    private List<WorldTile> DeserializeMap(byte[] data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream mf = new MemoryStream(data);

        List<WorldTile> saveTiles = (List<WorldTile>)bf.Deserialize(mf);
        mf.Close();

        return saveTiles;
    }

    [Client]
    private void MyCompletlyRecievedHandler(int transmissionID, byte[] data)
    {
        messagesRecieved++;

        //RectTransform rt = loadingScreenProgressBar.GetComponent<RectTransform>();
        //rt.sizeDelta = new Vector2(640f * messagesRecieved / totalMessages, rt.sizeDelta.y);

        //float perc = (float)messagesRecieved / (float)totalMessages;

        //loadingScreenProgressText.text = "Loading: " + perc * 100;

        Tilemap tm = TileMapManager.Instance.GetTilemap("Tilemap_" + transmissionID);
        if (!tm)
        {
            Debug.LogError("Error finding the correct tilemap");
            return;
        }
            
        List<WorldTile> saveData = DeserializeMap(data);

        gameTiles.SetWorldTiles(tm, "", saveData);
    }

    [Client]
    private void OnTilemapSet(string tilemap)
    {
        tilemapsSynced++;

        if (tilemapsSynced == totalMessages)
        {
            Debug.Log("Sync Complete");
            TileMapManager.Instance.SyncComplete();
            CmdMapSyncComplete();
        }
    }
}