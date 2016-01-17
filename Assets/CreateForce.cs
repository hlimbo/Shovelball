using UnityEngine;
using System.Collections;

public class CreateForce : MonoBehaviour
{
    public float power;
    public Vector2 direction;
    public Transform hitSource;
    public Movement movement;
    public bool useCollisionAngle;
    
    void OnTriggerEnter2D (Collider2D other)
    {
        if (other.tag == TagManager.ball)
        {
            Ball ball = other.GetComponent<Ball>();
            Vector2 forceVector = direction * power;
            float whichDirection = -1f;

            if (useCollisionAngle)
            {
                forceVector = (other.transform.position - hitSource.position).normalized * power;
            }

            if (movement != null && movement.facingRight)
                whichDirection = 1f;

            ball.SendFlying(new Vector2(forceVector.x * whichDirection, forceVector.y));
        }
    }
}
