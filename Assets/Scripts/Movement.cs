using UnityEngine;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    public float maxMoveSpeed = 8.0f;
    public float moveAcceleration = 4.0f;
    public float maxFallSpeed = -8.0f;
    public float fallAcceleration = 0.5f;
    public float groundFriction = 1.0f;
    public string playerInputIndex;
    public bool isGrounded = false;

    private Dictionary<string, Acceleration> accelerations;

	// Use this for initialization
	void Start ()
    {
        accelerations = new Dictionary<string, Acceleration>();
        accelerations.Add("Gravity", new Acceleration(null, maxFallSpeed, fallAcceleration));
        accelerations.Add("Movement", new Acceleration(null, null, moveAcceleration));
        accelerations.Add("GroundFriction", new Acceleration(0.0f, null, groundFriction));
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        //Input.GetAxisRaw("Horizontal" + playerInputIndex);

        //controllers
        float direction = Input.GetAxis("Horizontal" + playerInputIndex) * maxMoveSpeed;
        if (direction != 0)
        {
            accelerations["GroundFriction"].magnitude = 0.0f;
            accelerations["Movement"].maxVelX = direction;
        }
        else
        {
            accelerations["GroundFriction"].magnitude = groundFriction;
        }


            ApplyAccelerations();
	}

    private void OnCollisionEnter2D (Collision2D collision)
    {
        if (collision.gameObject.tag == TagManager.floor || collision.gameObject.tag == TagManager.platform)
        {
            
            accelerations["Gravity"].maxVelY = 0.0f;
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0.0f);
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D (Collision2D collision)
    {
        if (collision.gameObject.tag == TagManager.floor || collision.gameObject.tag == TagManager.platform)
        {
            accelerations["Gravity"].maxVelY = maxFallSpeed;
            isGrounded = false;
        }
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
            GetComponent<Rigidbody2D>().velocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accel.magnitude);
        }
    }
}
