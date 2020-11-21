using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 directionOfShot;
    private Rigidbody2D rb2d;
    [SerializeField] float bulletSpeed;

    public float GetAngleFromVectorFloat(Vector3 direction)
    {
        direction = direction.normalized;
        float n = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (n < 0)
            n += 360;

        return n;
    }
    public void Setup(Vector3 direction)
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        directionOfShot = direction;
        transform.eulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(direction));
        Destroy(gameObject, 3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        
        // transform.position += (directionOfShot) * bulletSpeed * Time.deltaTime; //FOR FLAMETHROWER
        // rb2d.AddForce(directionOfShot * bulletSpeed * Time.deltaTime); //FOR ROCKET LAWN CHAIR
        rb2d.velocity = directionOfShot * bulletSpeed;
    }
}
