using UnityEngine;
using System.Collections;

public class CreateForce : MonoBehaviour
{
    public float power;
    public Vector2 direction;
    public Collider2D hitSource;
    public Movement movement;
    public bool useCollisionAngle;
    
    void OnTriggerEnter2D (Collider2D other)
    {
        if (other.tag == TagManager.ball)
        {
            Ball ball = other.GetComponent<Ball>();
            Vector2 forceVector = direction * power;

            if (useCollisionAngle)
            {
                forceVector = (other.transform.position - hitSource.transform.position).normalized * power;
            }

            ball.SendFlying(new Vector2(forceVector.x, forceVector.y));
        }
    }
}
