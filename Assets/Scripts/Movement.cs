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
    public string attackButton;

    public Transform leftGroundCheck;
    public Transform rightGroundCheck;
    public Transform leftWallCheck;
    public Transform rightWallCheck;
    public float checkRadius;
    public LayerMask floorLayerMask;
    public LayerMask wallLayerMask;
    public LayerMask ballLayerMask;

    public bool facingRight = true;

    private Dictionary<string, Acceleration> accelerations;
    private float previousDirection;
    private int jumpTimer;
    private int jumpMaxTimer;

    private Animator anim;
    private Rigidbody2D rbody;
    private Transform trans;

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
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody2D>();
        trans = GetComponent<Transform>();
    }

    // FixedUpdate is called once per physics step
    void FixedUpdate()
    {
        // Check if we are grounded.
        bool wasGrounded = anim.GetBool(TagManager.isOnGround);
        bool wasOnWall = anim.GetBool(TagManager.isOnWall);

        // Set the animator's state values
        anim.SetBool(TagManager.isOnGround, Physics2D.OverlapCircle(leftGroundCheck.position, checkRadius, floorLayerMask) || Physics2D.OverlapCircle(rightGroundCheck.position, checkRadius, floorLayerMask));
        anim.SetBool(TagManager.isOnWall, Physics2D.OverlapCircle(leftWallCheck.position, checkRadius, wallLayerMask) || Physics2D.OverlapCircle(rightWallCheck.position, checkRadius, wallLayerMask));
        anim.SetBool(TagManager.isOnBall, Physics2D.OverlapCircle(leftGroundCheck.position, checkRadius, ballLayerMask) || Physics2D.OverlapCircle(rightGroundCheck.position, checkRadius, ballLayerMask));

        // On landing, apply horizontal boost for responsive controls. Also tell animation controller we hit the ground.
        if (anim.GetBool(TagManager.isOnGround) && (wasGrounded != anim.GetBool(TagManager.isOnGround)))
        {
            rbody.velocity = PhysicsUtility.SetVelocity(rbody.velocity, rbody.velocity.x * momentumScale, null);
        }
        
        // On hitting a wall, apply horizontal momentum to vertical momentum
        if (anim.GetBool(TagManager.isOnWall) && (wasOnWall != anim.GetBool(TagManager.isOnWall)))
        {
            float ysign = Mathf.Sign(rbody.velocity.y);
            if (ysign > 0.0f)
                rbody.velocity = PhysicsUtility.SetVelocity(rbody.velocity, 0.0f, rbody.velocity.magnitude * ysign);
            else
                rbody.velocity = PhysicsUtility.SetVelocity(rbody.velocity, 0.0f, 0.0f);

            // flip character
            Flip();
        }

        // Disable gravity if needed
        if (anim.GetBool(TagManager.isOnGround))
            accelerations["Gravity"].maxVelY = null;
        else if (anim.GetBool(TagManager.isOnWall))
            accelerations["Gravity"].maxVelY = wallSlideSpeed;
        else
            accelerations["Gravity"].maxVelY = maxFallSpeed;

        // Apply movement.
        DoMove();

        // Apply jump.
        DoJump();

        // Apply attack.
        DoAttack();

        // Apply all accelerations.
        rbody.velocity = PhysicsUtility.ApplyAccelerations(rbody.velocity, accelerations.Values);
    }

    private void DoMove()
    {
        // Can't move while wall-hugging
        if (!anim.GetBool(TagManager.isOnWall) || (anim.GetBool(TagManager.isOnWall) && anim.GetBool(TagManager.isOnGround)))
        {
            // Get the scaled movement direction depending on if grounded or in air.
            float direction = Input.GetAxisRaw(horizontalAxis);
            direction *= anim.GetBool(TagManager.isOnGround) ? maxMoveSpeed : maxAirSpeed;

            float currAcceleration = anim.GetBool(TagManager.isOnGround) ? moveAcceleration : airAcceleration;
            float currPivotSpeedRetention = anim.GetBool(TagManager.isOnGround) ? groundPivotSpeedRetention : airPivotSpeedRetention;
            float currFriction = anim.GetBool(TagManager.isOnGround) ? groundFriction : airFriction;

            // If the player is moving ...
            if (direction != 0)
            {
                // If the player's direction changed, set horizontal velocity to half. For responsiveness.
                if (Mathf.Sign(previousDirection) != Mathf.Sign(direction))
                {
                    rbody.velocity = PhysicsUtility.SetVelocity(rbody.velocity, rbody.velocity.x * currPivotSpeedRetention, null);
                    Flip();
                }

                // Disable friction and apply movement
                accelerations["Friction"].maxVelX = null;
                accelerations["Movement"].maxVelX = direction;
                accelerations["Movement"].magnitude = currAcceleration;

                previousDirection = direction;
                anim.SetBool(TagManager.isWalking, true);
            }
            // Player not moving ... apply friction and remove movement influence
            else
            {
                accelerations["Friction"].maxVelX = 0.0f;
                accelerations["Friction"].magnitude = currFriction;
                accelerations["Movement"].maxVelX = null;
                anim.SetBool(TagManager.isWalking, false);
            }
        }
    }

    void DoJump()
    {
        // Player has jumped?
        if (Input.GetButtonDown(jumpButton)
            && (anim.GetBool(TagManager.isOnGround) || anim.GetBool(TagManager.isOnWall) || anim.GetBool(TagManager.isOnBall))
            && jumpTimer == jumpMaxTimer)
        {
            // Reset jump timer and max frames of jumping
            jumpTimer = 0;
            jumpMaxTimer = MIN_JUMP_FRAMES;

            // If jumping from wall, apply horizontal acceleration
            if (anim.GetBool(TagManager.isOnWall) && !anim.GetBool(TagManager.isOnGround))
            {
                float facing = (facingRight) ? 1 : -1;
                rbody.velocity = PhysicsUtility.SetVelocity(rbody.velocity, facing * maxMoveSpeed, null);
                anim.SetBool(TagManager.isOnWall, false);
            }
        }

        // If a jump is in progress, apply acceleration
        if (jumpTimer < jumpMaxTimer)
        {
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
        else
        {
            accelerations["Jump"].maxVelY = null;
            accelerations["Jump"].magnitude = 0.0f;
        }
    }

    private void DoAttack()
    {
        if (Input.GetButtonDown(attackButton))
        {
            anim.SetBool(TagManager.isAttacking, true);
        }
    }

    public void ClearAttack()
    {
        anim.SetBool(TagManager.isAttacking, false);
    }

    private void Flip()
    {
        trans.localScale = new Vector3(-Mathf.Sign(previousDirection), trans.localScale.y, trans.localScale.z);
        facingRight = !facingRight;
    }
}