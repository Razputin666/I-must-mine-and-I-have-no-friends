using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    
    private void Start()
    {

    }
}