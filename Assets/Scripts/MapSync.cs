//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//using Mirror;
//using UnityEngine.Tilemaps;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.IO;

//public class MapSync : NetworkBehaviour
//{
//    [SerializeField] private TMP_Text loadingScreenProgressText = null;
//    [SerializeField] private Image loadingScreenProgressBar = null;
//    [SerializeField] private GameObject tilemapPrefab = null;
//    private GameTiles gameTiles = null;
//    private NetworkTransmitter networkTransmitter = null;

//    private int messagesRecieved = 0;
//    private int totalMessages = 0;
//    private int tilemapsSynced = 0;
//    //private System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();

//    public void Start()
//    {
//        if (gameTiles == null)
//            gameTiles = GetComponent<GameTiles>();

//        if (networkTransmitter == null)
//            networkTransmitter = GetComponent<NetworkTransmitter>();
//    }
//    public override void OnStartLocalPlayer()
//    {
//        if (!isLocalPlayer)
//            return;

//        if (!isClientOnly)
//        {
//            StartCoroutine(SpawnPlayer(connectionToClient));
//            return;
//        }

//        if (gameTiles == null)
//            gameTiles = GetComponent<GameTiles>();

//        gameTiles.OnWorldTilesSet += OnTilemapSet;

//        if (networkTransmitter == null)
//            networkTransmitter = GetComponent<NetworkTransmitter>();

//        networkTransmitter.OnDataCompletelyReceived += MyCompletlyRecievedHandler;

//        StartCoroutine(RequestTerrainData());
//    }
//    [Server]
//    private IEnumerator SpawnPlayer(NetworkConnection conn)
//    {
//        yield return new WaitForSeconds(0.5f);
//        gameObject.transform.position = NetworkManager.startPositions[0].position;
        
//        GetComponent<PlayerController>().SetReady(true);
//    }

//    private IEnumerator RequestTerrainData()
//    {
//        yield return null;

//        CmdRequestTerrainData();
//    }
//    #region Server
//    [Command(ignoreAuthority = true)]
//    private void CmdRequestTerrainData()
//    {
//        NetworkConnection conn = connectionToClient;
//        GameObject grid = GameObject.Find("Grid");

//        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();
//        RpcSendMessageCount(tilemaps.Length);

//        SendMapData(conn, tilemaps);
//    }

//    [Command]
//    private void CmdMapSyncComplete()
//    {
//        GetComponent<PlayerController>().SetReady(true);
//        RpcSetReady();
//        gameObject.transform.position = NetworkManager.startPositions[0].position;
//    }

//    [TargetRpc]
//    private void RpcSetReady()
//    {
//        GetComponent<PlayerController>().SetReady(true);
//    }

//    [Server]
//    private void SendMapData(NetworkConnection conn, Tilemap[] tilemaps)
//    {
//        for (int i = 0; i < tilemaps.Length; i++)
//        {
//            List<WorldTile> saveTiles = gameTiles.GetWorldTiles(tilemaps[i], false);
//            byte[] tmByte = SerializeTileMap(saveTiles);
//            networkTransmitter.SendBytesToClient(conn, i, tmByte);
//        }
//    }

//    [Server]
//    private byte[] SerializeTileMap(List<WorldTile> saveTiles)
//    {
//        BinaryFormatter bf = new BinaryFormatter();
//        MemoryStream ms = new MemoryStream();

//        bf.Serialize(ms, saveTiles);
//        ms.Close();

//        return ms.ToArray();
//    }
//    #endregion

//    [TargetRpc]
//    private void RpcSendMessageCount(int messageCount)
//    {
//        totalMessages = messageCount;
//        Debug.Log("messages: " + messageCount);
//    }

//    [Client]
//    private List<WorldTile> DeserializeMap(byte[] data)
//    {
//        BinaryFormatter bf = new BinaryFormatter();
//        MemoryStream mf = new MemoryStream(data);

//        List<WorldTile> saveTiles = (List<WorldTile>)bf.Deserialize(mf);
//        mf.Close();

//        return saveTiles;
//    }

//    [Client]
//    private void MyCompletlyRecievedHandler(int transmissionID, byte[] data)
//    {
//        messagesRecieved++;

//        //RectTransform rt = loadingScreenProgressBar.GetComponent<RectTransform>();
//        //rt.sizeDelta = new Vector2(640f * messagesRecieved / totalMessages, rt.sizeDelta.y);

//        //float perc = (float)messagesRecieved / (float)totalMessages;

//        //loadingScreenProgressText.text = "Loading: " + perc * 100;

//        GameObject grid = GameObject.Find("Grid");
//        GameObject chunk = Instantiate(tilemapPrefab, grid.transform);
//        chunk.name = "tilemap_" + transmissionID;

//        Tilemap tm = chunk.GetComponent<Tilemap>();

//        List<WorldTile> saveData = DeserializeMap(data);

//        gameTiles.SetWorldTiles(tm, "", saveData);
//    }

//    [Client]
//    private void OnTilemapSet(string tilemap)
//    {
//        tilemapsSynced++;

//        if(tilemapsSynced == totalMessages)
//        {
//            Debug.Log("Sync Complete");
//            CmdMapSyncComplete();
//        }
//    }
//}