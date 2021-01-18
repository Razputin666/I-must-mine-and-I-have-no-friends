using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbMovement : MonoBehaviour
{
    [SerializeField] private EnemyBehaviour body;
    [SerializeField] private Transform[] arms;
    //[SerializeField] private Transform[] armDesiredTarget;
    // private Transform[] armCurrentTarget;
    [SerializeField] private Transform swingTarget;
    [SerializeField] public Transform[] legs;
    [SerializeField] public Transform[] legCurrentTargets;

    [SerializeField] public Transform[] legDesiredTargets;

    [SerializeField] private Transform[] legDefaultTargets;
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private Transform target;
    
   // private List<Vector3> defaultTargets = new List<Vector3>();
    private float desiredYPosition;
    private bool isGrounded;
    private const float swingNumber = 1.2f;
    private Vector3 armLerp;
    public float speed;
    float timer;
    float swingTimer;
    [HideInInspector] public bool isSwinging;
    public AnimationCurve yCurve;



    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < legs.Length; i++)
        {
           legCurrentTargets[i] = Instantiate(target, legs[i].position, Quaternion.identity);
         //  defaultTargets.Add(legs[i].position);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        MoveArmDirection();
        // Cast a ray
        for (int i = 0; i < legs.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(legDesiredTargets[i].position.x, transform.position.y),
                -Vector2.up, transform.localScale.y * 4, LayerMask.GetMask("Blocks"));

            if (hit.collider != null)
            {
                if (i == 0)
                {
                    
                    if (legs[i + 1].position.y - hit.point.y <= 0f)
                    {
                       
                    }
                }
                desiredYPosition = hit.point.y;


                //if (legs[i].position.y - hit.point.y == 0f)
                //{

                //    IsGrounded = true;
                //}
                //else
                //{
                //    IsGrounded = false;
                //}
            }
            else
            {
                desiredYPosition = transform.position.y;
                IsGrounded = false;
            }
            legDesiredTargets[i].position = new Vector2(legDesiredTargets[i].position.x,
            desiredYPosition);


            float dist;

            dist = Vector2.Distance(legCurrentTargets[i].position, legDesiredTargets[i].position);

            if (dist > 3f)
            {
                legCurrentTargets[i].position = legDesiredTargets[i].position;
            }

            float dist2 = Vector2.Distance(legs[i].position, legCurrentTargets[i].position);

            // Move the foot                 
            if (dist2 > 0.1f)
            {
                timer = 0;
                // Increase curve timer
                timer += Time.deltaTime;
                // Move towards desired target position
                legs[i].position = Vector2.MoveTowards(legs[i].position, new Vector2(legCurrentTargets[i].position.x, legCurrentTargets[i].position.y + yCurve.Evaluate(timer)), speed * Time.deltaTime);

            }
            // Clamp the foot        
            else
            {
                legs[i].position = legCurrentTargets[i].position;
            }

           // if (Mathf.Abs(rb2d.velocity.x) <= 0f)
           // {
               // transform.position = Vector2.MoveTowards(transform.position, new Vector2(legs[i].position.x, transform.position.y), speed * Time.deltaTime);
               // currentTargets[i].position = Vector2.MoveTowards(currentTargets[i].position, defaultTargets[i].position, speed * Time.deltaTime); // ruins everything?

          //  }


            transform.position = Vector2.MoveTowards(transform.position, new Vector2(legs[i].position.x, transform.position.y), speed * Time.deltaTime);

        }


        // If we hit a collider, set the desiredYPosition to the hit Y point.        


    }

    private void MoveArmDirection()
    {
        foreach (Transform arm in arms)
        {

            if (Vector2.Distance(body.target, transform.position) < body.GetStats.range && !isSwinging)
            {
                StartCoroutine(Swing(arm));
            }
            else if (isSwinging)
            {
                return;
            }
            else
            {
                arm.up = Vector2.Lerp(arm.up, Vector2.down, body.GetStats.strength * Time.deltaTime);
            }

        }
    }

    private IEnumerator Swing(Transform arm)
    {
        isSwinging = true;
        Vector3 direction = new Vector2(swingTarget.position.x - arm.position.x, swingTarget.position.y - arm.position.y);
        swingTimer = 0f;
        while (swingTimer < body.GetStats.strength / 2)
        {

            arm.up = Vector2.Lerp(arm.up, direction, body.GetStats.strength * Time.deltaTime);
            swingTimer += body.GetStats.strength * Time.deltaTime;

            yield return null;
        }

        while (Vector2.Distance(body.target, transform.position) < body.GetStats.range)
        {
                swingTimer = 0f;

                direction = new Vector2(body.target.x - arm.position.x, body.target.y - arm.position.y);
            while (swingTimer < body.GetStats.strength / 2)
            {
                arm.up = Vector2.Lerp(arm.up, direction, body.GetStats.strength * Time.deltaTime);
                swingTimer += body.GetStats.strength * Time.deltaTime;

                yield return null;
            }
            swingTimer = 0f;
            direction = new Vector2(swingTarget.position.x - arm.position.x, swingTarget.position.y - arm.position.y);
            while (swingTimer < body.GetStats.strength / 2)
            {


                arm.up = Vector2.Lerp(arm.up, direction, body.GetStats.strength * Time.deltaTime);
                swingTimer += body.GetStats.strength * Time.deltaTime;

                yield return null;
            }
        }
        isSwinging = false;

    }

    private IEnumerator SwingAt(Transform arm, Vector3 position)
    {
        isSwinging = true;
        Vector3 direction = new Vector2(swingTarget.position.x - arm.position.x, swingTarget.position.y - arm.position.y);
        swingTimer = 0f;
        while (swingTimer < body.GetStats.strength / 2)
        {

            arm.up = Vector2.Lerp(arm.up, direction, body.GetStats.strength * Time.deltaTime);
            swingTimer += body.GetStats.strength * Time.deltaTime;

            yield return null;
        }

        while (Vector2.Distance(body.target, transform.position) < body.GetStats.range)
        {
            swingTimer = 0f;

            direction = new Vector2(body.target.x - arm.position.x, body.target.y - arm.position.y);
            while (swingTimer < body.GetStats.strength / 2)
            {
                arm.up = Vector2.Lerp(arm.up, direction, body.GetStats.strength * Time.deltaTime);
                swingTimer += body.GetStats.strength * Time.deltaTime;

                yield return null;
            }
            swingTimer = 0f;
            direction = new Vector2(swingTarget.position.x - arm.position.x, swingTarget.position.y - arm.position.y);
            while (swingTimer < body.GetStats.strength / 2)
            {


                arm.up = Vector2.Lerp(arm.up, direction, body.GetStats.strength * Time.deltaTime);
                swingTimer += body.GetStats.strength * Time.deltaTime;

                yield return null;
            }
        }
        isSwinging = false;

    }

    public void MoveFootTarget(Vector3 direction)
    {
        for (int i = 0; i < legs.Length; i++)
        {
           legCurrentTargets[i].position = Vector2.MoveTowards(legCurrentTargets[i].position, legDesiredTargets[i].position, speed * Time.deltaTime);
           // currentTargets[i].position += direction * Time.deltaTime; 
           transform.position = Vector2.MoveTowards(transform.position, new Vector2(legs[i].position.x, transform.position.y), speed * Time.deltaTime);
        }

    }

    public bool IsGrounded
    {
        get { return isGrounded; }

        set { isGrounded = value; }
    }
}

