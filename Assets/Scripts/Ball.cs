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

    public bool isGrounded;
    public bool isFlying;
    public bool gettingHit = false;

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
        //Debug.DrawRay(other.contacts[0].point, other.contacts[0].normal, Color.red, 1);
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
                ballBody.velocity = Vector2.Reflect(ballBody.velocity * decay, surfaceNormal);
        }

        // Ball-ball collision, handle whole collision.
        if (other.gameObject.tag == TagManager.ball)
        {
            if (!gettingHit)
            {
                Ball otherBall = other.gameObject.GetComponent<Ball>();
                otherBall.gettingHit = true;

                // Set up collision vectors
                Vector2 myForceVector = other.contacts[0].normal;
                Vector2 otherForceVector = other.contacts[0].normal;

                if (otherBall.isGrounded)
                    myForceVector = GetRicochet(myForceVector, otherBall.surfaceNormal);
                if (isGrounded)
                    otherForceVector = GetRicochet(otherForceVector, surfaceNormal);

                // More powerful collision
                if (isFlying)
                    otherBall.SendFlying(myForceVector * -stashVelocity.magnitude * momentumTransferRatio);
                else
                    otherBall.ballBody.velocity = myForceVector * -ballBody.velocity.magnitude * otherBall.bounciness;
                if (otherBall.isFlying)
                    SendFlying(otherForceVector *= otherBall.stashVelocity.magnitude * otherBall.momentumTransferRatio);
                else
                    ballBody.velocity = otherForceVector * otherBall.ballBody.velocity.magnitude * bounciness;
            }
            else
            {
                gettingHit = false;
            }
        }
        
        // Ball-player collision
        if (other.gameObject.tag == TagManager.player){

            Movement player = other.gameObject.GetComponent<Movement>();
            Vector2 forceVector = ballBody.velocity;

            // If flying, hit harder
            if (isFlying)
                forceVector *= momentumTransferRatio;
            else
                forceVector *= playerMomentumTransferRatio;

            if (player.IsGrounded())
                forceVector = GetRicochet(forceVector, player.SurfaceNormal());

            player.Knockback(forceVector);
            Lag(Vector2.Reflect(ballBody.velocity * bounciness, surfaceNormal));
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
        Lag(velocity);
    }

    private void Lag(Vector2 velocity)
    {
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
