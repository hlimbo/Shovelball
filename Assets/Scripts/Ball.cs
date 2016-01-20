using UnityEngine;
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
    public float playerBounciness;

    public bool isGrounded;
    public bool isFlying;

    private Dictionary<string, Acceleration> accelerations;
    private Rigidbody2D ballBody;
    private CircleCollider2D circleCollider;
    private Transform ballTransform;

    private int maxDelayFrames;
    private int delayFrames;
    private Vector2 stashVelocity;
    private float stashAngularVelocity;
    private Vector2 surfaceNormal;

    //useful for ball Pooling
    public int TagNumber { get; set; }

    // Use this for initialization
    void Start ()
    {
        accelerations = new Dictionary<string, Acceleration>();
        accelerations.Add("Gravity", new Acceleration(null, maxFallSpeed, fallAcceleration));
        accelerations.Add("Friction", new Acceleration(null, null, airFriction));

        ballBody = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        ballTransform = GetComponent<Transform>();

        maxDelayFrames = 0;
        delayFrames = maxDelayFrames;

        TagNumber = INACTIVE;
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        // Pause physics if delayed
        if (delayFrames == maxDelayFrames)
        {
            // Check if on ground
            LayerMask floorLayerMask = LayerMask.GetMask(TagManager.floor, TagManager.platform);
            isGrounded = Physics2D.OverlapCircle(ballBody.position, (circleCollider.radius + 0.1f) * ballTransform.localScale.y, floorLayerMask);

            if (ballBody.velocity.magnitude < flyingThreshold)
               isFlying = false;

            // Flying balls know no limits
            if (isFlying)
            {
                accelerations["Gravity"].maxVelY = null;
                accelerations["Friction"].maxVelY = 0.0f;
                accelerations["Friction"].maxVelX = 0.0f;
            }
            else
            {
                accelerations["Gravity"].maxVelY = maxFallSpeed;
                accelerations["Friction"].maxVelY = null;
                accelerations["Friction"].maxVelX = null;
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
            ballBody.angularVelocity = 0f;
            ballBody.velocity = Vector2.zero;
            delayFrames++;
        }
	}

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == TagManager.wall)
        {
            ballBody.velocity = Vector2.Reflect(ballBody.velocity * wallHitDecay, other.contacts[0].normal);
        }
        else if (other.gameObject.tag == TagManager.floor)
        {
            ballBody.velocity = Vector2.Reflect(ballBody.velocity * bounciness, other.contacts[0].normal);
        }
        else if (other.gameObject.tag == TagManager.platform)
        {
            if (other.contacts[0].point.y < ballBody.position.y)
            {
                ballBody.velocity = Vector2.Reflect(ballBody.velocity * bounciness, other.contacts[0].normal);
            }
            else if (ballBody.velocity.magnitude < 1f)
            {
                ballBody.velocity *= 2f;
            }
        }
        else if (other.gameObject.tag == TagManager.ball)
        {
            // if flying, do special stuff
            Ball otherBall = other.gameObject.GetComponent<Ball>();
            if (isFlying)
            {
                isFlying = false;
                Vector2 forceVector = ballBody.velocity.normalized * stashVelocity.magnitude * momentumTransferRatio;
                if (otherBall.isGrounded)
                {
                    forceVector = GetRicochet(forceVector, otherBall.surfaceNormal);
                }
                otherBall.SendFlying(forceVector);
                ballBody.velocity = Vector2.zero;
            }
            // otherwise do normal bounce
            else
            {
                ballBody.velocity = Vector2.Reflect(other.relativeVelocity * bounciness, other.contacts[0].normal) * playerBounciness;
            }
        }
        else if (other.gameObject.tag == TagManager.player)
        {
            Movement player = other.gameObject.GetComponent<Movement>();
            Vector2 forceVector = other.relativeVelocity * playerMomentumTransferRatio;
            Vector2 reactVector = Vector2.Reflect(other.relativeVelocity * bounciness, other.contacts[0].normal) * playerBounciness;

            if (isFlying)
            {
                forceVector = ballBody.velocity.normalized * stashVelocity.magnitude * momentumTransferRatio;
                reactVector = Vector2.zero;
            }

            if (player.IsGrounded())
                forceVector = GetRicochet(forceVector, player.SurfaceNormal());
            if (isGrounded)
                reactVector = GetRicochet(reactVector, surfaceNormal);

            player.Knockback(forceVector);
            ballBody.velocity = reactVector;
        }
    }

    private Vector2 GetRicochet(Vector2 vector, Vector2 normal)
    {
        if (Vector2.Angle(vector, normal) > 90)
            return Vector2.Reflect(vector, normal);
        return vector;
    }

    public void SendFlying(Vector2 velocity)
    {
        isFlying = true;
        maxDelayFrames = (int)(velocity.magnitude / 5f);
        stashVelocity = velocity;
        stashAngularVelocity = ballBody.angularVelocity;
        ballBody.velocity = Vector2.zero;
        ballBody.angularVelocity = 0f;
        delayFrames = 0;
    }

    public Vector2 SurfaceNormal()
    {
        return surfaceNormal;
    }
}
