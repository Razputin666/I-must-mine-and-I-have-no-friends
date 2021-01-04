using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegMovement : MonoBehaviour
{
    [SerializeField] public Transform[] legs;
    [SerializeField] public Transform[] currentTargets;

    [SerializeField] public Transform[] desiredTargets;

    [SerializeField] private Transform[] defaultTargets;
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private Transform target;
    
   // private List<Vector3> defaultTargets = new List<Vector3>();
    private float desiredYPosition;
    private bool isGrounded;
    public float speed;
    float timer;
    public AnimationCurve yCurve;



    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < legs.Length; i++)
        {
           currentTargets[i] = Instantiate(target, legs[i].position, Quaternion.identity);
         //  defaultTargets.Add(legs[i].position);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // Cast a ray
        for (int i = 0; i < legs.Length; i++)
        {



            RaycastHit2D hit = Physics2D.Raycast(new
       Vector2(desiredTargets[i].position.x, transform.position.y),
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
            desiredTargets[i].position = new Vector2(desiredTargets[i].position.x,
            desiredYPosition);


            float dist;

            dist = Vector2.Distance(currentTargets[i].position, desiredTargets[i].position);

            if (dist > 3f)
            {
                currentTargets[i].position = desiredTargets[i].position;
            }

            float dist2 = Vector2.Distance(legs[i].position, currentTargets[i].position);

            // Move the foot                 
            if (dist2 > 0.1f)
            {
                timer = 0;
                // Increase curve timer
                timer += Time.deltaTime;
                // Move towards desired target position
                legs[i].position = Vector2.MoveTowards(legs[i].position, new Vector2(currentTargets[i].position.x, currentTargets[i].position.y + yCurve.Evaluate(timer)), speed * Time.deltaTime);

            }
            // Clamp the foot        
            else
            {
                legs[i].position = currentTargets[i].position;
            }

           // if (Mathf.Abs(rb2d.velocity.x) <= 0f)
           // {
               // transform.position = Vector2.MoveTowards(transform.position, new Vector2(legs[i].position.x, transform.position.y), speed * Time.deltaTime);
               // currentTargets[i].position = Vector2.MoveTowards(currentTargets[i].position, defaultTargets[i].position, speed * Time.deltaTime); // ruins everything?

          //  }


           // transform.position = Vector2.MoveTowards(transform.position, new Vector2(legs[i].position.x, transform.position.y), speed * Time.deltaTime);

        }


        // If we hit a collider, set the desiredYPosition to the hit Y point.        


    }

    public void MoveFootTarget(Vector3 direction)
    {
        for (int i = 0; i < legs.Length; i++)
        {
           currentTargets[i].position = Vector2.MoveTowards(currentTargets[i].position, desiredTargets[i].position, speed * Time.deltaTime);
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

