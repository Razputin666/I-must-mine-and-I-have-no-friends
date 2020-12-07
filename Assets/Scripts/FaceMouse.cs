using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FaceMouse : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    public void RotateArm()
    {
        Vector2 direction = new Vector2(
        player.worldPosition.x - transform.position.x,
        player.worldPosition.y - transform.position.y);

        transform.up = direction;
    }
}