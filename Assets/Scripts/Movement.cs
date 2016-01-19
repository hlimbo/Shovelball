using UnityEngine;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    public int playerIndex;
    public float maxMoveSpeed;
    public float maxAirSpeed;
    public float moveAcceleration;
    public float momentumScale;
    public float bounceSpeed;
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

    public Transform leftGroundCheck;
    public Transform rightGroundCheck;
    public Transform leftWallCheck;
    public Transform rightWallCheck;
    public float checkRadius;
    public LayerMask wallLayerMask;
    public LayerMask ballLayerMask;

    public bool facingRight = true;
    public bool canJump = true;
    public bool canAttack = true;

    private Dictionary<string, Acceleration> accelerations;
    private float previousDirection = -1;
    private int jumpTimer;
    private int jumpMaxTimer;

    private Vector2 surfaceNormal;

    private int maxDelayFrames;
    private int delayFrames;
    private int dropFrames;
    private int maxDropFrames = 0;

    private Vector2 stashVelocity;

    private Animator anim;
    private Rigidbody2D rbody;
    private Transform trans;

    private PlayerInput input;

    private bool flippedFromMove = false;

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

        maxDelayFrames = 0;
        delayFrames = maxDelayFrames;
        dropFrames = maxDropFrames;
    }

    // FixedUpdate is called once per physics step
    void FixedUpdate()
    {
        if (dropFrames != maxDropFrames)
        {
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            GetComponent<BoxCollider2D>().enabled = true;
        }
        if (delayFrames == maxDelayFrames)
        {
            input = LazyInputManager.GetInput(playerIndex);

            // Check if we are grounded.
            bool wasGrounded = anim.GetBool(TagManager.isOnGround);
            bool wasOnWall = anim.GetBool(TagManager.isOnWall);

            // Set the animator's state values
            LayerMask floorLayerMask = LayerMask.GetMask(TagManager.floor, TagManager.platform);
            LayerMask platformLayerMask = LayerMask.GetMask(TagManager.platform);
            anim.SetBool(TagManager.isOnGround, Physics2D.OverlapCircle(leftGroundCheck.position, checkRadius, floorLayerMask) || Physics2D.OverlapCircle(rightGroundCheck.position, checkRadius, floorLayerMask));
            anim.SetBool(TagManager.isOnWall, Physics2D.OverlapCircle(leftWallCheck.position, checkRadius, wallLayerMask) || Physics2D.OverlapCircle(rightWallCheck.position, checkRadius, wallLayerMask));
            anim.SetBool(TagManager.isOnBall, Physics2D.OverlapCircle(leftGroundCheck.position, checkRadius, ballLayerMask) || Physics2D.OverlapCircle(rightGroundCheck.position, checkRadius, ballLayerMask));

            // Platform dropping
            bool isOnPlatform = Physics2D.OverlapCircle(leftGroundCheck.position, checkRadius, platformLayerMask) || Physics2D.OverlapCircle(rightGroundCheck.position, checkRadius, platformLayerMask);
            if (input.joyStickY < -0.9f && isOnPlatform)
            {
                maxDropFrames = 10 - (int) (Mathf.Abs(rbody.velocity.y) / 10f);
                if (maxDropFrames < 0)
                    maxDropFrames = 3;
                dropFrames = 0;
                anim.SetBool(TagManager.isOnGround, false);
            }
            if (dropFrames < maxDropFrames)
            {
                dropFrames++;
            }

            // On landing, apply horizontal boost for responsive controls. Also tell animation controller we hit the ground.
            if (anim.GetBool(TagManager.isOnGround) && (wasGrounded != anim.GetBool(TagManager.isOnGround)))
            {
                rbody.velocity = PhysicsUtility.SetVelocity(rbody.velocity, rbody.velocity.x * momentumScale, null);
            }

            // Disable gravity if needed
            if (anim.GetBool(TagManager.isOnGround))
                accelerations["Gravity"].maxVelY = null;
            else if (anim.GetBool(TagManager.isOnWall))
                accelerations["Gravity"].maxVelY = wallSlideSpeed;
            else
                accelerations["Gravity"].maxVelY = maxFallSpeed;

            // Apply movement.
            flippedFromMove = false;
            DoMove();

            // On hitting a wall, apply horizontal momentum to vertical momentum
            // Need to do it here in case the player flipped in DoMove()
            if (anim.GetBool(TagManager.isOnWall) && (wasOnWall != anim.GetBool(TagManager.isOnWall)))
            {
                float ysign = Mathf.Sign(rbody.velocity.y);
                float xsign = Mathf.Sign(rbody.velocity.x);
                if (ysign > 0.0f)
                    rbody.velocity = PhysicsUtility.SetVelocity(rbody.velocity, xsign * 5.0f, rbody.velocity.magnitude * ysign);
                else
                    rbody.velocity = PhysicsUtility.SetVelocity(rbody.velocity, xsign * 5.0f, 0.0f);

                // flip character
                if (!flippedFromMove)
                {
                    Flip();
                    if (!wasGrounded)
                    previousDirection = -previousDirection;
                }
            }

            // Apply jump.
            DoJump();

            // Apply attack.
            DoAttack();

            // Apply all accelerations.
            rbody.velocity = PhysicsUtility.ApplyAccelerations(rbody.velocity, accelerations.Values);

            // Prevent holding button down from issuing multiple commands
            canJump = !input.jump;
            canAttack = !input.attackForward;
        }
        else if (delayFrames == maxDelayFrames - 1)
        {
            rbody.velocity = stashVelocity;
            delayFrames++;
        }
        else
        {
            delayFrames++;
        }
    }

    private void DoMove()
    {
        // Can't move while wall-hugging
        if (!anim.GetBool(TagManager.isOnWall) || anim.GetBool(TagManager.isOnBall) || (anim.GetBool(TagManager.isOnWall) && anim.GetBool(TagManager.isOnGround)))
        {
            // Get the scaled movement direction depending on if grounded or in air.
            float direction = input.joyStickX;
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
                    flippedFromMove = true;
                }

                // Disable friction and apply movement
                accelerations["Friction"].maxVelX = null;
                accelerations["Movement"].maxVelX = direction;
                accelerations["Movement"].magnitude = currAcceleration;

                previousDirection = direction;
                anim.SetBool(TagManager.isWalking, true);
            }
            // Player not moving ... apply friction and remove movement influence
            // Also, need to keep track of flipping
            else
            {
                accelerations["Friction"].maxVelX = 0.0f;
                accelerations["Friction"].magnitude = currFriction;
                accelerations["Movement"].maxVelX = null;
                anim.SetBool(TagManager.isWalking, false);
            }
        }
        else
        {
            anim.SetBool(TagManager.isWalking, false);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == TagManager.player)
        {
            this.Knockback((rbody.position - other.rigidbody.position).normalized * bounceSpeed, false);
        }
        else if (other.gameObject.tag == TagManager.floor || other.gameObject.tag == TagManager.platform)
        {
            surfaceNormal = other.contacts[0].normal;
        }
    }

    void DoJump()
    {
        // Player has jumped?
        if (input.jump && canJump && jumpTimer == jumpMaxTimer && (anim.GetBool(TagManager.isOnGround) || anim.GetBool(TagManager.isOnWall) || anim.GetBool(TagManager.isOnBall)))
        {
            // Reset jump timer and max frames of jumping
            jumpTimer = 0;
            jumpMaxTimer = MIN_JUMP_FRAMES;

            // If jumping from wall, apply horizontal acceleration
            if (anim.GetBool(TagManager.isOnWall) && !anim.GetBool(TagManager.isOnGround))
            {
                float dir = (facingRight) ? 1 : -1;
                rbody.velocity = PhysicsUtility.SetVelocity(rbody.velocity, dir * maxMoveSpeed, null);
            }
        }

        // If a jump is in progress, apply acceleration
        if (jumpTimer < jumpMaxTimer)
        {
            // If just about to hit jump frame limit, increase limits accordingly
            if (input.jump && jumpTimer == jumpMaxTimer - 1)
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
            canJump = true;
            accelerations["Jump"].maxVelY = null;
            accelerations["Jump"].magnitude = 0.0f;
        }
    }

    private void DoAttack()
    {
        if (input.attackForward && canAttack)
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
        trans.localScale = new Vector3(Mathf.Sign(previousDirection), trans.localScale.y, trans.localScale.z);
        facingRight = Mathf.Sign(previousDirection) < 0;
    }

    public void Knockback(Vector2 velocity, bool delay = true)
    {
        maxDelayFrames = maxDelayFrames = (int)(velocity.magnitude / 5f);
        stashVelocity = velocity;
        rbody.velocity = Vector2.zero;
        delayFrames = 0;
    }

    public void Lag(int maxDelay)
    {
        maxDelayFrames = maxDelay;
        stashVelocity = rbody.velocity;
        rbody.velocity = Vector2.zero;
        delayFrames = 0;
    }

    public bool IsGrounded()
    {
        return anim.GetBool(TagManager.isOnGround);
    }

    public Vector2 SurfaceNormal()
    {
        return surfaceNormal;
    }
}