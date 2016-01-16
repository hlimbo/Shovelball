using UnityEngine;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    public float maxMoveSpeed;
    public float maxAirSpeed;
    public float moveAcceleration;
    public float momentumScale;
    public float airAcceleration;
    public float maxFallSpeed;
    public float wallSlideSpeed;
    public float fallAcceleration;
    public float groundFriction;
    public float airFriction;
    public float groundPivotSpeedRetention;
    public float airPivotSpeedRetention;
    public float jumpScaling;
    public float maxJumpSpeed;
    public float jumpAcceleration;
    public int MIN_JUMP_FRAMES;
    public int MID_JUMP_FRAMES;
    public int MAX_JUMP_FRAMES;
    public string playerInputIndex;

    public Transform groundCheck;
    public Transform leftWallCheck;
    public Transform rightWallCheck;
    public float checkRadius;
    public LayerMask floorLayerMask;
    public LayerMask wallLayerMask;

    public bool isGrounded;
    public bool isOnWall;
    public bool isJumping;

    private Dictionary<string, Acceleration> accelerations;
    private float previousDirection;
    private int jumpTimer;
    private int jumpMaxTimer;

    // Use this for initialization
    void Start()
    {
        accelerations = new Dictionary<string, Acceleration>();
        accelerations.Add("Gravity", new Acceleration(null, maxFallSpeed, fallAcceleration));
        accelerations.Add("Movement", new Acceleration(null, null, moveAcceleration));
        accelerations.Add("Friction", new Acceleration(null, null, 0.0f));
        accelerations.Add("Jump", new Acceleration(null, null, 0.0f));

        jumpTimer = MIN_JUMP_FRAMES;
        jumpMaxTimer = MIN_JUMP_FRAMES;
    }

    // FixedUpdate is called once per physics step
    void FixedUpdate()
    {
        // Check if we are grounded.
        bool wasGrounded = isGrounded;
        bool wasOnWall = isOnWall;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, floorLayerMask);
        bool isOnLeftWall = Physics2D.OverlapCircle(leftWallCheck.position, checkRadius, wallLayerMask);
        bool isOnRightWall = Physics2D.OverlapCircle(rightWallCheck.position, checkRadius, wallLayerMask);
        isOnWall = isOnLeftWall || isOnRightWall;

        // On landing, apply horizontal boost for responsive controls
        if (isGrounded && (wasGrounded != isGrounded))
        {
            SetVelocity(GetComponent<Rigidbody2D>().velocity.x * momentumScale, null);
        }
        
        // On hitting a wall, apply horizontal momentum to vertical momentum
        if (isOnWall && (wasOnWall != isOnWall))
        {
            float ysign = Mathf.Sign(GetComponent<Rigidbody2D>().velocity.y);
            if (ysign > 0.0f)
                SetVelocity(0.0f, GetComponent<Rigidbody2D>().velocity.magnitude * ysign);
            else
                SetVelocity(0.0f, 0.0f);

            // flip character
            GetComponent<Transform>().localScale = new Vector3((isOnLeftWall)? -1 : 1, GetComponent<Transform>().localScale.y, GetComponent<Transform>().localScale.z);
        }

        // Disable gravity if needed
        if (isGrounded || isJumping)
            accelerations["Gravity"].maxVelY = null;
        else if (isOnWall)
            accelerations["Gravity"].maxVelY = wallSlideSpeed;
        else
            accelerations["Gravity"].maxVelY = maxFallSpeed;

        // Apply movement.
        DoMove();

        // Apply jump.
        DoJump();

        // Apply all accelerations.
        ApplyAccelerations();
    }

    private void DoMove()
    {
        // Can't move while wall-hugging
        if (!isOnWall || (isOnWall && isGrounded))
        {
            // Get the scaled movement direction depending on if grounded or in air.
            float direction = Input.GetAxisRaw("Horizontal" + playerInputIndex);
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
    }

    void DoJump()
    {
        // Player has jumped?
        if (Input.GetButtonDown("Jump" + playerInputIndex) && (isGrounded || isOnWall) && jumpTimer == jumpMaxTimer)
        {
            // Reset jump timer and max frames of jumping
            jumpTimer = 0;
            jumpMaxTimer = MIN_JUMP_FRAMES;

            // If jumping from wall, apply horizontal acceleration
            if (isOnWall && !isGrounded)
                SetVelocity(GetComponent<Transform>().localScale.x * maxMoveSpeed, null);
        }

        // If a jump is in progress, apply acceleration
        if (jumpTimer < jumpMaxTimer)
        {
            isJumping = true;

            // If just about to hit jump frame limit, increase limits accordingly
            if (Input.GetButton("Jump" + playerInputIndex) && jumpTimer == jumpMaxTimer - 1)
            {
                if (jumpMaxTimer == MIN_JUMP_FRAMES)
                    jumpMaxTimer = MID_JUMP_FRAMES;
                else if (jumpMaxTimer == MID_JUMP_FRAMES)
                    jumpMaxTimer = MAX_JUMP_FRAMES;
            }

            // scaling jump acceleration over the maximum POSSIBLE jump
            float scale = 1.0f - ((float)jumpTimer / (float)MAX_JUMP_FRAMES);
            float scaledMore = Mathf.Pow(scale, jumpScaling);

            accelerations["Jump"].maxVelY = maxJumpSpeed;
            accelerations["Jump"].magnitude = jumpAcceleration * scaledMore;

            jumpTimer++;
        }
        // Otherwise, reset
        else
        {
            isJumping = false;
            accelerations["Jump"].maxVelY = null;
            accelerations["Jump"].magnitude = 0.0f;
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