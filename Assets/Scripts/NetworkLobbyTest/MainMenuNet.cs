using System.Collections;
using UnityEngine;
using Mirror;

public class MainMenuNet : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;

    public void HostLobby()
    {
        networkManager.StartHost();

        //StartCoroutine(StartGame());
        landingPagePanel.SetActive(false);
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(1f);
        networkManager.StartGame();
    }
}