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

    // Use this for initialization
    void Start ()
    {
        accelerations = new Dictionary<string, Acceleration>();
        accelerations.Add("Gravity", new Acceleration(null,maxFallSpeed,fallAcceleration));
        accelerations.Add("Movement", new Acceleration(null,null, moveAcceleration));
        accelerations.Add("Jump", new Acceleration(null, null, 0.0f));
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        bool wasGrounded = isGrounded;
        bool wasOnWall = isOnWall;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, floorLayerMask);
        isOnWall = Physics2D.OverlapCircle(leftWallCheck.position, checkRadius, wallLayerMask)
            || Physics2D.OverlapCircle(rightWallCheck.position, checkRadius, wallLayerMask);

        if (isGrounded)
        {
            accelerations["Gravity"].maxVelY = null;
            SetVelocity(null, maxMoveSpeed);
        }
        else
        {
            //apply gravity here
            accelerations["Gravity"].maxVelY = maxFallSpeed;
        }



       // SetVelocity(0, 30);
        ApplyAccelerations();


	}

    private void Bounce()
    {
    }

    private void Move()
    {

    }

    private void ApplyAccelerations()
    {
        foreach (Acceleration accel in accelerations.Values)
        {
            // Get current and target velocities
            Vector2 currentVelocity = GetComponent<Rigidbody2D>().velocity;
            Vector2 targetVelocity = new Vector2(currentVelocity.x, currentVelocity.y);

            // If the target velocity has a non-null value, then use its value
            if (accel.maxVelX.HasValue)
                targetVelocity.x = accel.maxVelX.GetValueOrDefault();
            if (accel.maxVelY.HasValue)
                targetVelocity.y = accel.maxVelY.GetValueOrDefault();

            // Linearly interpolate towards the new velocity
            GetComponent<Rigidbody2D>().velocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accel.magnitude * Time.fixedDeltaTime);
        }
    }

    // NULL values are PASS-THROUGH. AKA they do NOT change the player velocity.
    private void SetVelocity(float? x, float? y)
    {
        Vector2 currentVelocity = GetComponent<Rigidbody2D>().velocity;
        Vector2 newVelocity = new Vector2(currentVelocity.x, currentVelocity.y);
        if (x.HasValue)
            newVelocity.x = x.GetValueOrDefault();
        if (y.HasValue)
            newVelocity.y = y.GetValueOrDefault();
        GetComponent<Rigidbody2D>().velocity = newVelocity;
    }

}
