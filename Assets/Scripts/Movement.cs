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
    public string jumpButton;
    public string horizontalAxis;

    public Transform leftGroundCheck;
    public Transform rightGroundCheck;
    public Transform leftWallCheck;
    public Transform rightWallCheck;
    public float checkRadius;
    public LayerMask floorLayerMask;
    public LayerMask wallLayerMask;

    public bool isGrounded;
    public bool isOnWall;
    public bool isJumping;
    public bool facingRight = true;

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
        // Combine platform and floor masks for grounded mask
        isGrounded = Physics2D.OverlapCircle(leftGroundCheck.position, checkRadius, floorLayerMask) || Physics2D.OverlapCircle(rightGroundCheck.position, checkRadius, floorLayerMask);
        isOnWall = Physics2D.OverlapCircle(leftWallCheck.position, checkRadius, wallLayerMask) || Physics2D.OverlapCircle(rightWallCheck.position, checkRadius, wallLayerMask);

        // On landing, apply horizontal boost for responsive controls
        if (isGrounded && (wasGrounded != isGrounded))
        {
            GetComponent<Rigidbody2D>().velocity = PhysicsUtility.SetVelocity(GetComponent<Rigidbody2D>().velocity, GetComponent<Rigidbody2D>().velocity.x * momentumScale, null);
        }
        
        // On hitting a wall, apply horizontal momentum to vertical momentum
        if (isOnWall && (wasOnWall != isOnWall))
        {
            float ysign = Mathf.Sign(GetComponent<Rigidbody2D>().velocity.y);
            if (ysign > 0.0f)
                GetComponent<Rigidbody2D>().velocity = PhysicsUtility.SetVelocity(GetComponent<Rigidbody2D>().velocity, 0.0f, GetComponent<Rigidbody2D>().velocity.magnitude * ysign);
            else
                GetComponent<Rigidbody2D>().velocity = PhysicsUtility.SetVelocity(GetComponent<Rigidbody2D>().velocity, 0.0f, 0.0f);

            // flip character
            Flip();
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
        GetComponent<Rigidbody2D>().velocity = PhysicsUtility.ApplyAccelerations(GetComponent<Rigidbody2D>().velocity, accelerations.Values);
    }

    private void DoMove()
    {
        // Can't move while wall-hugging
        if (!isOnWall || (isOnWall && isGrounded))
        {
            // Get the scaled movement direction depending on if grounded or in air.
            float direction = Input.GetAxisRaw(horizontalAxis);
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
                    GetComponent<Rigidbody2D>().velocity = PhysicsUtility.SetVelocity(GetComponent<Rigidbody2D>().velocity, GetComponent<Rigidbody2D>().velocity.x * currPivotSpeedRetention, null);
                    Flip();
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
        if (Input.GetButtonDown(jumpButton) && (isGrounded || isOnWall) && jumpTimer == jumpMaxTimer)
        {
            // Reset jump timer and max frames of jumping
            jumpTimer = 0;
            jumpMaxTimer = MIN_JUMP_FRAMES;

            // If jumping from wall, apply horizontal acceleration
            if (isOnWall && !isGrounded)
                GetComponent<Rigidbody2D>().velocity = PhysicsUtility.SetVelocity(GetComponent<Rigidbody2D>().velocity, GetComponent<Transform>().localScale.x * maxMoveSpeed, null);
        }

        // If a jump is in progress, apply acceleration
        if (jumpTimer < jumpMaxTimer)
        {
            isJumping = true;

            // If just about to hit jump frame limit, increase limits accordingly
            if (Input.GetButton(jumpButton) && jumpTimer == jumpMaxTimer - 1)
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
        else if (isJumping)
        {
            isJumping = false;
            accelerations["Jump"].maxVelY = null;
            accelerations["Jump"].magnitude = 0.0f;
        }
    }

    private void Flip()
    {
        GetComponent<Transform>().localScale = new Vector3(-Mathf.Sign(previousDirection), GetComponent<Transform>().localScale.y, GetComponent<Transform>().localScale.z);
        facingRight = !facingRight;
    }
}