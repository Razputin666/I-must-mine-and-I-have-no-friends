using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderManager : MonoBehaviour
{
    BoxCollider2D bruh;

    public void bwatevs(GameObject[,] boxCollider, int width, int height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                boxCollider[i, j].GetComponent<BoxCollider2D>().usedByComposite = true;
            }
        }
    }


}
