using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Unity.Collections;
using Unity.Jobs;

public class LoadingScreen : NetworkBehaviour
{
    [SerializeField] private TMP_Text loadingScreenProgressText = null;
    [SerializeField] private Image loadingScreenProgressBar = null;
    [SerializeField] private GameObject tilemapPrefab = null;

    private GameTiles gameTiles = null;
    private NetworkTransmitter networkTransmitter = null;
    
    private int messagesRecieved = 0;
    private int totalMessages = 0;

    private System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
    //public override void OnStartServer()
    //{
    //    Debug.Log("start");
    //     NetworkManager.singleton.StartGame();
    //}

    public void Start()
    {
        if (gameTiles == null)
            gameTiles = GetComponent<GameTiles>();

        if (networkTransmitter == null)
            networkTransmitter = GetComponent<NetworkTransmitter>();
    }
    public override void OnStartClient()
    {
        if (!isClientOnly)
        {
            return;
        }

        if (!isLocalPlayer)
            return;

        if (gameTiles == null)
            gameTiles = GetComponent<GameTiles>();

        if (networkTransmitter == null)
            networkTransmitter = GetComponent<NetworkTransmitter>();

        networkTransmitter.OnDataCompletelyReceived += MyCompletlyRecievedHandler;

        StartCoroutine(RequestTerrainData());
        
    }

    private IEnumerator RequestTerrainData()
    {
        yield return null;

        CmdRequestTerrainData();
    }
    #region Server
    [Command(ignoreAuthority = true)]
    private void CmdRequestTerrainData()
    {
        NetworkConnection conn = connectionToClient;
        GameObject grid = GameObject.Find("Grid");

        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();
        Debug.Log("tilemaps: " + tilemaps.Length);
        RpcSendMessageCount(tilemaps.Length);

        SendMapData(conn, tilemaps);
    }

    [Command]
    private void CmdMapSyncComplete()
    {
        //Destroy gameobject
        //Spawn player
    }

    [Server]
    private void SendMapData(NetworkConnection conn, Tilemap[] tilemaps)
    {
        Debug.Log(conn);
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
        Debug.Log("Transmission: " + transmissionID);

        GameObject grid = GameObject.Find("Grid");
        GameObject chunk = Instantiate(tilemapPrefab, grid.transform);
        chunk.name = "tilemap_" + transmissionID;
        chunk.GetComponent<TilemapCollider2D>().maximumTileChangeCount = 100;

        Tilemap tm = chunk.GetComponent<Tilemap>();

        List<WorldTile> saveData = DeserializeMap(data);
        //gameTiles.saveTiles = saveData;
        stopWatch.Restart();
        stopWatch.Start();
        gameTiles.SetWorldTiles(tm, "", saveData);
        stopWatch.Stop();
        // Get the elapsed time as a TimeSpan value.
        System.TimeSpan ts = stopWatch.Elapsed;

        // Format and display the TimeSpan value.
        string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Debug.Log("setTile time: " + elapsedTime);
        if (messagesRecieved == totalMessages)
            CmdMapSyncComplete();
    }
}