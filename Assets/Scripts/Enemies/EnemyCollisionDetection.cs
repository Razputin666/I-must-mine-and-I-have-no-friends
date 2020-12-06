using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollisionDetection : MonoBehaviour
{

    private EnemyController enemy;
    


    private void Start()
    {
        enemy = GetComponent<EnemyController>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {


        if (collision.collider.CompareTag("Bullet"))
        {
            enemy.enemyHP -= collision.collider.GetComponent<Bullet>().projectileStrength;
        }
        if (collision.collider.CompareTag("Enemy"))
        {
            EnemyController ally = collision.collider.gameObject.GetComponentInParent<EnemyController>();

            if (collision.otherCollider.transform.position.x - collision.collider.transform.position.x > 0f)
            {
                enemy.rb2d.AddForceAtPosition(ally.enemyKnockBack, transform.position);
            }

            else if (collision.otherCollider.transform.position.x - collision.collider.transform.position.x < 0f)
            {
                enemy.rb2d.AddForceAtPosition(-ally.enemyKnockBack, transform.position);
            }

        }

        else if (collision.collider.CompareTag("Player") && enemy.enemyStates != EnemyController.EnemyStates.FrogAggressive)
        {
            enemy.SetAggressiveMode();
        }

    }
   

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.GetComponent<BoxCollider2D>() && enemy.enemyStates != EnemyController.EnemyStates.FrogAggressive)
    //    {
    //        enemy.SetAggressiveMode();
    //    }
    //}


}
