using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        Vector2 direction = new Vector2(
        player.transform.position.x- transform.position.x,
        player.transform.position.y - transform.position.y);

       // Debug.Log(transform.up);
        transform.up = direction;
    }
}
