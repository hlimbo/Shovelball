using UnityEngine;
using System.Collections.Generic;

public class Ball : MonoBehaviour {


    public static int INACTIVE = -1;

    public float maxFallSpeed;
    public float fallAcceleration;
    public float flyingThreshold;
    //public float moveAcceleration;
    public float bounciness;
    public float wallHitDecay;
    public float airFriction;

    public LayerMask floorLayerMask;

    public bool isGrounded;
    public bool isFlying;

    private Dictionary<string, Acceleration> accelerations;
    private Rigidbody2D ballBody;
    private CircleCollider2D collider;

    //useful for ball Pooling
    public int TagNumber { get; set; }

    // Use this for initialization
    void Start ()
    {
        accelerations = new Dictionary<string, Acceleration>();
        accelerations.Add("Gravity", new Acceleration(null, maxFallSpeed, fallAcceleration));
        accelerations.Add("Friction", new Acceleration(null,null, airFriction));

        ballBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();

        TagNumber = INACTIVE;
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        //detect what the ball hits using physics engine.
        isGrounded = Physics2D.OverlapCircle(ballBody.position, collider.radius, floorLayerMask);
        //isOnWall = Physics2D.OverlapCircle(leftWallCheck.position, checkRadius, wallLayerMask)
        //    || Physics2D.OverlapCircle(rightWallCheck.position, checkRadius, wallLayerMask);

        if (ballBody.velocity.magnitude < flyingThreshold)
        {
            isFlying = false;
        }

        // gravity
        if (isGrounded || isFlying)
        {
            accelerations["Gravity"].maxVelY = null;
        }
        else
        {
            accelerations["Gravity"].maxVelY = maxFallSpeed;
        }


        // airdrag
        if (isFlying)
        {
            accelerations["Friction"].maxVelX = 0.0f;
            accelerations["Friction"].maxVelY = 0.0f;
        }
        else
        {
            accelerations["Friction"].maxVelX = null;
            accelerations["Friction"].maxVelY = null;
        }

        ballBody.velocity = PhysicsUtility.ApplyAccelerations(ballBody.velocity, accelerations.Values);
	}

    void OnCollisionEnter2D(Collision2D other)
    {
        //reflections
        if (other.gameObject.tag == TagManager.wall
            || other.gameObject.tag == TagManager.platform
            || other.gameObject.tag == TagManager.floor) 
        {
            Vector2 CollNorm = other.contacts[0].normal;
            float decay = bounciness;
            if (other.gameObject.tag == TagManager.wall)
                decay = wallHitDecay;
            if (other.gameObject.tag != TagManager.platform || other.contacts[0].point.y < ballBody.position.y)
            {
                ballBody.velocity = Vector2.Reflect(ballBody.velocity.normalized * ballBody.velocity.magnitude * decay, CollNorm);
            }
        }

        // Ball-ball collision
        if (other.gameObject.tag == TagManager.ball)
        {
            // if flying, do special stuff
            Ball otherBall = other.gameObject.GetComponent<Ball>();
            if (isFlying)
            {
                if (otherBall.isFlying)
                {
                    other.rigidbody.velocity = ballBody.velocity;
                    ballBody.velocity = ballBody.velocity.normalized * ballBody.velocity.magnitude * -1f;
                }
                else
                {
                    isFlying = false;
                    otherBall.SendFlying(ballBody.velocity);
                    ballBody.velocity = new Vector2(0f, 0f);
                }
            }
            // otherwise do normal bounce
            else if (!otherBall.isFlying)
            {
                ballBody.velocity = Vector2.Reflect(ballBody.velocity.normalized * ballBody.velocity.magnitude * bounciness, other.contacts[0].normal);
            }
        }
        
        // Ball-player collision
        {

        }
    }

    public void SendFlying(Vector2 velocity)
    {
        isFlying = true;
        ballBody.velocity = velocity;
    }


}
