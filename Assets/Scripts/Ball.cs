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
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        Vector2 oldVelocity = ballBody.velocity;
        bool wasGrounded = isGrounded;
        bool wasOnWall = isOnWall;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, floorLayerMask);
        isOnWall = Physics2D.OverlapCircle(leftWallCheck.position, checkRadius, wallLayerMask)
            || Physics2D.OverlapCircle(rightWallCheck.position, checkRadius, wallLayerMask);

        if (isGrounded)
        {
            //accelerations["Gravity"].maxVelY = null;
            ballBody.velocity = PhysicsUtility.SetVelocity(ballBody.velocity,null, maxMoveSpeed);
            
            oldVelocity = ballBody.velocity;
        }
        else
        {
            //apply gravity here
            //accelerations["Gravity"].maxVelY = maxFallSpeed;
            ballBody.velocity = PhysicsUtility.SetVelocity(ballBody.velocity, null, maxFallSpeed);
         
           // oldVelocity = ballBody.velocity;

        }

        ballBody.velocity = PhysicsUtility.ApplyAccelerations(oldVelocity, accelerations.Values);


	}

    private void Bounce()
    {
    }

    private void Move()
    {

    }

   

}
