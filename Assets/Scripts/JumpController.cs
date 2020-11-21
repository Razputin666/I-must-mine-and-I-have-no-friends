using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpController : MonoBehaviour, HasCoolDownInterFace
{
    [SerializeField] private int id = 1;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private PlayerController player;
    [SerializeField] private CoolDownSystem coolDownSystem = null;

    public int Id => id;
    public float CoolDownDuration => coolDownDuration;


    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Input.GetKey("space") || !player.IsGrounded())
        {
            return;
        }
        

            if (coolDownSystem.IsOnCoolDown(id))
            { 
                return; 
            }

        player.rb2d.AddForce(transform.up * player.jumpVelocity);
        coolDownSystem.PutOnCoolDown(this);
    }
}
