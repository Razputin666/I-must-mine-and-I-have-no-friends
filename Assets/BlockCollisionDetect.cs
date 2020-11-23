using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCollisionDetect : MonoBehaviour
{
    Vector3Int pointOfContact;

    private void OnTriggerStay2D(Collider2D collision)
    {

          pointOfContact = Vector3Int.FloorToInt(collision.ClosestPoint(FindObjectOfType<BoxCollider2D>().transform.position));

    }

    public Vector3Int PointOfContact
    {
        get { return pointOfContact; }

        set { pointOfContact = value; }
    }

}
