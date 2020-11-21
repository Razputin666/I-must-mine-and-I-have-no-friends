using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
  [SerializeField]private PlayerController player;

    public enum PlayerMode
    {
        Mining, Combat
    }

    public PlayerMode playerMode;
    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    public void SetMiningMode()
    {
       
    }

    public void SetCombatMode()
    {
        
    }
}
