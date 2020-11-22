using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceMouse : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        Vector2 direction = new Vector2(
        player.worldPosition.x - transform.position.x,
        player.worldPosition.y - transform.position.y);

        transform.up = direction;
    }

}
