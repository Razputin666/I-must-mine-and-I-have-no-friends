using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GamemanagerJoffy : MonoBehaviour
{

    public static GamemanagerJoffy main;

    public GameObject notSure;

    public float xBound = 3f;
    public float yBound = 3f;
    public float ballSpeed = 3f;
    public float bruh = 2f;
    public int[] somethingCool;

    public Text mainText;
    public Text[] playerTexts;
    // Start is called before the first frame update
    private void Awake()
    {
        if (main != null && main != this)
        { 
            Destroy(gameObject);
        return;
        }
        main = this;



    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
