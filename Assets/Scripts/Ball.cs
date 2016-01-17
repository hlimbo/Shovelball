using UnityEngine;
using System.Collections.Generic;

public class Ball : MonoBehaviour {


    public float maxFallSpeed;
    public float fallAcceleration;
    public float maxMoveSpeed;
    public float moveAcceleration;

    public Transform groundCheck;
    public Transform leftWallCheck;
    public Transform rightWallCheck;
    public LayerMask floorLayerMask;
    public LayerMask wallLayerMask;
    public float checkRadius;

    public bool isGrounded;
    public bool isOnWall;

    private Dictionary<string, Acceleration> accelerations;
    private Rigidbody2D ballBody;

    // Use this for initialization
    void Start ()
    {
        accelerations = new Dictionary<string, Acceleration>();
        accelerations.Add("Gravity", new Acceleration(null,maxFallSpeed,fallAcceleration));
        accelerations.Add("Movement", new Acceleration(null,null, moveAcceleration));
        accelerations.Add("Jump", new Acceleration(null, null, 0.0f));

        ballBody = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
   
    }


	// Update is called once per frame
	void FixedUpdate ()
    {
        Vector2 oldVelocity = ballBody.velocity;


        //detect what the ball hits using physics engine.
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, floorLayerMask);
        isOnWall = Physics2D.OverlapCircle(leftWallCheck.position, checkRadius, wallLayerMask)
            || Physics2D.OverlapCircle(rightWallCheck.position, checkRadius, wallLayerMask);

        //bounce
        if (isGrounded)
        {
            //apply upward acceleration here
            ballBody.velocity = PhysicsUtility.SetVelocity(ballBody.velocity, null, maxMoveSpeed);

            oldVelocity = ballBody.velocity;
        }
        else
        {
            //apply gravity here
            ballBody.velocity = PhysicsUtility.SetVelocity(ballBody.velocity, null, maxFallSpeed);
        }

      

        ballBody.velocity = PhysicsUtility.ApplyAccelerations(oldVelocity, accelerations.Values);


	}

    private void Bounce(Vector2 oldVelocity)
    {
        
    }

    private void Move()
    {

    }

    void OnCollisionEnter2D(Collision2D other)
    {
        //reflections
        if (other.gameObject.tag == TagManager.wall
            || other.gameObject.tag == TagManager.platform 
            || other.gameObject.tag == TagManager.player 
            || other.gameObject.tag == TagManager.ball)
        {
            Vector2 CollNorm = other.contacts[0].normal;
            ballBody.velocity = Vector2.Reflect(ballBody.velocity, CollNorm);
        }


    }


   

}
