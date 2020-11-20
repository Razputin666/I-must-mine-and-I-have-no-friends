using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustoCollisionTest : MonoBehaviour
{

    private float dudePosX;
    private float dudePosY;
    private float boxPosX;
    private float boxPosY;

    private float dudePos;
    private float boxPos;
    

    [SerializeField]
    private PlayerController playerPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //dudePosX = Mathf.Abs(playerPos.transform.position.x);
        //dudePosY = Mathf.Abs(playerPos.transform.position.y);
        //boxPosX = Mathf.Abs(gameObject.transform.position.x);
        //boxPosY = Mathf.Abs(gameObject.transform.position.y);

        dudePos =  playerPos.transform.position.magnitude;
        boxPos = gameObject.transform.position.magnitude;

        //if (dudePosX == boxPosX && dudePosY == boxPosY)
        //    Debug.Log("bruh");

        //  Debug.Log(dudePos + " Position of Dude " + boxPos + " Position of Box");

        // Debug.Log(boxPos -= dudePos);


        if (Physics.CheckBox(gameObject.transform.position, gameObject.transform.position / 2))
            Debug.Log("Collision");

        if ((boxPos -= dudePos) > -10)
        {

           // gameObject.GetComponent<BoxCollider2D>().enabled = true;

        }

        else
        {
          //  gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }

    }
}
