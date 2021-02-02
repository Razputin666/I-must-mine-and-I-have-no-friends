using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneScript : NetworkBehaviour
{
    public Text canvasStatusText;
    public PlayerController playerController;

    [SyncVar(hook = nameof(OnStatusTextChanged))]
    public string statusText;

    void OnStatusTextChanged(string _Old, string _New)
    {
        //called from sync var hook, to update info on screen for all players
        canvasStatusText.text = statusText;
    }

    //public void ButtonSendMessage()
    //{
    //    if (playerController != null)
    //        playerController.CmdSendPlayerMessage();
    //}
}