using UnityEngine;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    public float maxMoveSpeed;
    public float maxAirSpeed;
    public float moveAcceleration;
    public float airAcceleration;
    public float maxFallSpeed;
    public float fallAcceleration;
    public float groundFriction;
    public float airFriction;
    public float groundPivotSpeedRetention;
    public float airPivotSpeedRetention;
    public string playerInputIndex;

    public bool isGrounded;

    private Dictionary<string, Acceleration> accelerations;
    private bool checkGroundedFlag = false;
    private float previousDirection;
    private float checkPlatformY;

	// Use this for initialization
	void Start ()
    {
        accelerations = new Dictionary<string, Acceleration>();
        accelerations.Add("Gravity", new Acceleration(null, maxFallSpeed, fallAcceleration));
        accelerations.Add("Movement", new Acceleration(null, null, moveAcceleration));
        accelerations.Add("Friction", new Acceleration(null, null, 0.0f));
	}
	
	// FixedUpdate is called once per physics step
	void FixedUpdate ()
    {
        // Check if we are grounded.
        if (checkGroundedFlag && IsAbovePlatform(checkPlatformY))
        {
            isGrounded = true;
            checkGroundedFlag = false;
            // disable gravity or else we create drag
            accelerations["Gravity"].maxVelY = null;
        }

        // Apply movement.
        DoMove();

        // Apply all accelerations.
        ApplyAccelerations();
	}

    private void DoMove()
    {
        // Get the scaled movement direction depending on if grounded or in air.
        float direction = Input.GetAxis("Horizontal" + playerInputIndex);
        direction *= isGrounded ? maxMoveSpeed : maxAirSpeed;

        float currAcceleration = isGrounded ? moveAcceleration : airAcceleration;
        float currPivotSpeedRetention = isGrounded ? groundPivotSpeedRetention : airPivotSpeedRetention;
        float currFriction = isGrounded ? groundFriction : airFriction;

        // If the player is moving ...
        if (direction != 0)
        {
            // If the player's direction changed, set horizontal velocity to half. For responsiveness.
            if (Mathf.Sign(previousDirection) != Mathf.Sign(direction))
            {
                SetVelocity(GetComponent<Rigidbody2D>().velocity.x * currPivotSpeedRetention, null);
                GetComponent<Transform>().localScale = new Vector3(-Mathf.Sign(previousDirection), GetComponent<Transform>().localScale.y, GetComponent<Transform>().localScale.z);
            }

            // Disable friction and apply movement
            accelerations["Friction"].maxVelX = null;
            accelerations["Movement"].maxVelX = direction;
            accelerations["Movement"].magnitude = currAcceleration;

            previousDirection = direction;
        }
        // Player not moving ... apply friction and remove movement influence
        else
        {
            accelerations["Friction"].maxVelX = 0.0f;
            accelerations["Friction"].magnitude = currFriction;
            accelerations["Movement"].maxVelX = null;
        }
    }

    private void OnCollisionEnter2D (Collision2D collision)
    {
        // If we hit the floor or platform ...
        if (collision.gameObject.tag == TagManager.floor || collision.gameObject.tag == TagManager.platform)
        {
            // Schedule a check for whether we landed on TOP of a platform. Need to do it in the next FixedUpdate since at the
            // moment of collision, the colliders are intersecting and we can't accurately compare the colliders.
            checkGroundedFlag = true;
            checkPlatformY = collision.collider.transform.position.y + (((BoxCollider2D) collision.collider).size.y / 2);
        }
    }

    private void OnCollisionExit2D (Collision2D collision)
    {
        // If we leave the floor or platform ...
        if (collision.gameObject.tag == TagManager.floor || collision.gameObject.tag == TagManager.platform)
        {
            isGrounded = false;
            // re-enable gravity
            accelerations["Gravity"].maxVelY = maxFallSpeed;
        }
    }

    private bool IsAbovePlatform (float platformy)
    {
        float playery = GetComponent<Transform>().position.y - (GetComponent<BoxCollider2D>().size.y / 2);
        return platformy < playery;
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
}
