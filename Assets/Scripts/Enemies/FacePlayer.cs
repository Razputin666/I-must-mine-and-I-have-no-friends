using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    private Rigidbody2D rb2d;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 direction = new Vector2(
        (player.transform.position.x) - transform.position.x,
        player.transform.position.y - transform.position.y);

        // Vector2 direction = new Vector2(Random.Range(player.transform.position.x - transform.position.x, transform.position.x), Random.Range(player.transform.position.y - transform.position.y, transform.position.y));
        transform.up = direction;
        //rb2d.velocity = transform.up * Time.deltaTime;
        //transform.up = direction;
    }
}
