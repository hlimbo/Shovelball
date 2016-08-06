using UnityEngine;
using System.Collections.Generic;

public class Ball : MonoBehaviour {


    public static int INACTIVE = -1;

    public float maxFallSpeed;
    public float fallAcceleration;
    public float flyingThreshold;
    //public float moveAcceleration;
    public float bounciness;
    public float flyingTransferRatio;
    public float collisionTransferRatio;
    public float wallHitDecay;
    public float airFriction;

    public bool isGrounded;
    public bool isFlying;

    private AudioSource bounceSound;

    private Dictionary<string, Acceleration> accelerations;
    private Rigidbody2D ballBody;
    private CircleCollider2D circleCollider;
    private Transform ballTransform;

    private int maxDelayFrames;
    private int delayFrames;
    private Vector2 stashVelocity;
    private float stashAngularVelocity;
    private Vector2 surfaceNormal;
    private bool collided;

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
        bounceSound = GetComponent<AudioSource>();
        
        SetIgnorePlayers(false);
        collided = false;
    }
    private Vector2 storeVel;
	// Update is called once per frame
	void FixedUpdate ()
    {
        Debug.DrawRay(transform.position, ballBody.velocity, Color.yellow);
        collided = false;
        // Pause physics if delayed
        if (delayFrames == maxDelayFrames)
        {
            // Check if on ground
            LayerMask floorLayerMask = LayerMask.GetMask(TagManager.floor, TagManager.platform);
            isGrounded = Physics2D.OverlapCircle(ballBody.position, (circleCollider.radius + 0.1f) * ballTransform.localScale.y, floorLayerMask);

            if (ballBody.velocity.magnitude < flyingThreshold)
            {
                isFlying = false;
                SetIgnorePlayers(true);
            }

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
            storeVel = ballBody.velocity;
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
        if (other.gameObject.tag == TagManager.wall && !collided)
        {
            collided = true;
            ballBody.velocity = GetRicochet(storeVel * wallHitDecay, other.contacts[0].normal);
            bounceSound.Play();
        }
        else if (other.gameObject.tag == TagManager.floor && !collided)
        {
            collided = true;
            ballBody.velocity = GetRicochet(storeVel * bounciness, other.contacts[0].normal);
            bounceSound.Play();
        }
        else if (other.gameObject.tag == TagManager.platform && !collided)
        {
            collided = true;
            if (other.contacts[0].point.y < ballBody.position.y)
            {
                Debug.DrawRay(other.contacts[0].point, other.contacts[0].normal, Color.blue);
                ballBody.velocity = GetRicochet(storeVel * bounciness, other.contacts[0].normal);
                Debug.DrawRay(other.contacts[0].point, ballBody.velocity, Color.green);
                bounceSound.Play();
            }
            if (ballBody.velocity.magnitude < 2f)
            {
                ballBody.velocity = Vector2.zero;
            }
        }
        else if (other.gameObject.tag == TagManager.ball && !collided)
        {
            // if flying, do special stuff
            Ball otherBall = other.gameObject.GetComponent<Ball>();
            otherBall.collided = true;
            collided = true;
            if (isFlying)
            {
                isFlying = false;
                Vector2 forceVector = storeVel.normalized * stashVelocity.magnitude * flyingTransferRatio;
                if (otherBall.isGrounded)
                {
                    forceVector = GetRicochet(forceVector, otherBall.surfaceNormal);
                }
                otherBall.SendFlying(forceVector);
                ballBody.velocity = Vector2.zero;
            }
            else if (otherBall.isFlying)
            {
                otherBall.isFlying = false;
                Vector2 forceVector = otherBall.storeVel.normalized * otherBall.stashVelocity.magnitude * otherBall.flyingTransferRatio;
                if (isGrounded)
                {
                    forceVector = GetRicochet(forceVector, surfaceNormal);
                }
                SendFlying(forceVector);
                other.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
            // otherwise do normal bounce
            else
            {
                ballBody.velocity = other.contacts[0].normal * other.relativeVelocity.magnitude * collisionTransferRatio;
                otherBall.GetComponent<Rigidbody2D>().velocity = -ballBody.velocity;
            }
        }
        else if (other.gameObject.tag == TagManager.player)
        {
            Movement player = other.gameObject.GetComponent<Movement>();
            if (isFlying)
            {
                Vector2 forceVector = storeVel.normalized * stashVelocity.magnitude * flyingTransferRatio;
                Vector2 reactVector = Vector2.zero;

                if (player.IsGrounded())
                    forceVector = GetRicochet(forceVector, player.SurfaceNormal());
                if (isGrounded)
                    reactVector = GetRicochet(reactVector, surfaceNormal);

                player.Knockback(forceVector);
                ballBody.velocity = reactVector;
            }
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
        SetIgnorePlayers(false);
    }

    public Vector2 SurfaceNormal()
    {
        return surfaceNormal;
    }

    private void SetIgnorePlayers(bool ignore)
    {
        foreach (Movement move in FindObjectsOfType<Movement>())
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), move.GetComponent<Collider2D>(), ignore);
        }
    }
}
