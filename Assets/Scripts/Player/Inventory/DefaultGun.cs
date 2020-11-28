using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultGun : MonoBehaviour, HasCoolDownInterFace
{
    [SerializeField] PlayerController player;
    [SerializeField] private int id = 3;
    [SerializeField] private float coolDownDuration;
    [SerializeField] private CoolDownSystem coolDownSystem;
    [SerializeField] private FaceMouse armPos;
    [SerializeField] private Transform bullet;


    public int Id => id;
    public float CoolDownDuration => coolDownDuration;
    Transform endOfGun;

    // Start is called before the first frame update
    void Start()
    {
        endOfGun = transform.Find("EndOfGun");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Input.GetMouseButton(0))
        {
            return;
        }

        if (coolDownSystem.IsOnCoolDown(id))
        {
            return;
        }

        Transform bulletTransform = Instantiate(bullet,endOfGun.position , Quaternion.identity);
        Vector3 directionOfShot = (player.worldPosition - endOfGun.position).normalized;

        bulletTransform.GetComponent<Bullet>().Setup(directionOfShot);
        coolDownSystem.PutOnCoolDown(this);
    }
}
