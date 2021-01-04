using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private float desiredYPosition;
    [SerializeField] private Transform desiredTarget;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Cast a ray
        RaycastHit2D hit = Physics2D.Raycast(new
        Vector2(desiredTarget.position.x, transform.position.y + 5),
        Vector2.down, 12f);

        // If we hit a collider, set the desiredYPosition to the hit Y point.        
        if (hit.collider != null)
        {
            desiredYPosition = hit.point.y;
        }
        else
        {
            desiredYPosition = transform.position.y;
        }

        desiredTarget.position = new Vector2(desiredTarget.position.x,
        desiredYPosition);
    }
}

