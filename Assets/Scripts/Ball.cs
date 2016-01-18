﻿using UnityEngine;
using System.Collections.Generic;

public class Ball : MonoBehaviour {


    public static int INACTIVE = -1;

    public float maxFallSpeed;
    public float fallAcceleration;
    public float flyingThreshold;
    //public float moveAcceleration;
    public float bounciness;
    public float momentumTransferRatio;
    public float wallHitDecay;
    public float airFriction;
    public float playerMomentumTransferRatio;

    public LayerMask floorLayerMask;

    public bool isGrounded;
    public bool isFlying;

    private Dictionary<string, Acceleration> accelerations;
    private Rigidbody2D ballBody;
    private CircleCollider2D collider;

    private int maxDelayFrames;
    private int delayFrames;
    private Vector2 stashVelocity;
    private Vector2 surfaceNormal;

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

        maxDelayFrames = 0;
        delayFrames = maxDelayFrames;

        TagNumber = INACTIVE;
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        if (delayFrames == maxDelayFrames)
        {
            
            //detect what the ball hits using physics engine.
            isGrounded = Physics2D.OverlapCircle(ballBody.position, collider.radius, floorLayerMask);

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
        else if (delayFrames == maxDelayFrames - 1)
        {
            ballBody.velocity = stashVelocity;
            delayFrames++;
        }
        else
        {
            delayFrames++;
        }
	}

    void OnCollisionEnter2D(Collision2D other)
    {
        //reflections
        if (other.gameObject.tag == TagManager.wall
            || other.gameObject.tag == TagManager.platform
            || other.gameObject.tag == TagManager.floor) 
        {
            surfaceNormal = other.contacts[0].normal;
            float decay = bounciness;
            if (other.gameObject.tag == TagManager.wall)
                decay = wallHitDecay;
            if (other.gameObject.tag != TagManager.platform || other.contacts[0].point.y < ballBody.position.y)
            {
                ballBody.velocity = Vector2.Reflect(ballBody.velocity * decay, surfaceNormal);
            }
        }

        // Ball-ball collision, only handle getting hit.
        if (other.gameObject.tag == TagManager.ball)
        {
            // if flying, do special stuff
            Ball otherBall = other.gameObject.GetComponent<Ball>();
            if (isFlying)
            {
                isFlying = false;
                Vector2 forceVector = ballBody.velocity * momentumTransferRatio;
                if (otherBall.isGrounded && Vector2.Angle(forceVector, otherBall.SurfaceNormal()) > 90)
                {
                    forceVector = Vector2.Reflect(forceVector, otherBall.SurfaceNormal());
                }
                otherBall.SendFlying(forceVector);
                ballBody.velocity = Vector2.zero;
            }
            // otherwise do normal bounce
            else
            {
                ballBody.velocity = Vector2.Reflect(ballBody.velocity * momentumTransferRatio, other.contacts[0].normal);
            }
        }
        
        // Ball-player collision
        if (other.gameObject.tag == TagManager.player){
            // if flying, transfer momentum to player
            Movement player = other.gameObject.GetComponent<Movement>();
            if (isFlying)
            {
                isFlying = false;
                Vector2 forceVector = ballBody.velocity * momentumTransferRatio;
                if (player.IsGrounded() && Vector2.Angle(forceVector, player.SurfaceNormal()) > 90)
                {
                    forceVector = Vector2.Reflect(forceVector, player.SurfaceNormal());
                }
                player.Knockback(forceVector);
                ballBody.velocity = Vector2.zero;
            }
            // otherwise do normal bounce
            else
            {
                player.Knockback(ballBody.velocity * playerMomentumTransferRatio);
                ballBody.velocity = Vector2.Reflect(ballBody.velocity * momentumTransferRatio, other.contacts[0].normal);
            }
        }
    }

    public void SendFlying(Vector2 velocity)
    {
        maxDelayFrames = (int) (velocity.magnitude / 5f);
        isFlying = true;
        stashVelocity = velocity;
        ballBody.velocity = Vector2.zero;
        delayFrames = 0;
    }

    public Vector2 SurfaceNormal()
    {
        return surfaceNormal;
    }
}
