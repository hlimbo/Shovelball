using UnityEngine;
using System.Collections.Generic;

public class Ball : MonoBehaviour {


    public float maxFallSpeed;
    public float fallAcceleration;
    //public float maxMoveSpeed;
    //public float moveAcceleration;
    public float bounciness;

    //public Transform groundCheck;
    //public Transform leftWallCheck;
    //public Transform rightWallCheck;
    public LayerMask floorLayerMask;
   // public float checkRadius;

    public bool isGrounded;
    //public bool isOnWall;

    private Dictionary<string, Acceleration> accelerations;
    private Rigidbody2D ballBody;
    private CircleCollider2D collider;

    // Use this for initialization
    void Start ()
    {
        accelerations = new Dictionary<string, Acceleration>();
        accelerations.Add("Gravity", new Acceleration(null, maxFallSpeed, fallAcceleration));
        //accelerations.Add("Movement", new Acceleration(null,null, moveAcceleration));

        ballBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        //Vector2 oldVelocity = ballBody.velocity;


        //detect what the ball hits using physics engine.
        isGrounded = Physics2D.OverlapCircle(ballBody.position, collider.radius, floorLayerMask);
        //isOnWall = Physics2D.OverlapCircle(leftWallCheck.position, checkRadius, wallLayerMask)
        //    || Physics2D.OverlapCircle(rightWallCheck.position, checkRadius, wallLayerMask);

        //bounce
        if (isGrounded)
        {
            //apply upward acceleration here
            //ballBody.velocity = PhysicsUtility.SetVelocity(ballBody.velocity, null, ballBody.velocity.y * bounciness);
            accelerations["Gravity"].maxVelY = null;

            //oldVelocity = ballBody.velocity;
        }
        else
        {
            //apply gravity here
            //ballBody.velocity = PhysicsUtility.SetVelocity(ballBody.velocity, null, maxFallSpeed);
            accelerations["Gravity"].maxVelY = maxFallSpeed;
        }

      

        //ballBody.velocity = PhysicsUtility.ApplyAccelerations(oldVelocity, accelerations.Values);
        ballBody.velocity = PhysicsUtility.ApplyAccelerations(ballBody.velocity, accelerations.Values);

	}

    void OnCollisionEnter2D(Collision2D other)
    {
        //reflections
        if (other.gameObject.tag == TagManager.wall
            || other.gameObject.tag == TagManager.platform 
            || other.gameObject.tag == TagManager.player 
            || other.gameObject.tag == TagManager.ball
            || other.gameObject.tag == TagManager.floor) 
        {
            Vector2 CollNorm = other.contacts[0].normal;
            if (other.gameObject.tag != TagManager.platform || other.contacts[0].point.y < ballBody.position.y)
            {
                ballBody.velocity = Vector2.Reflect(ballBody.velocity.normalized * ballBody.velocity.magnitude * bounciness, CollNorm);
            }
        }
    }


   

}
