using UnityEngine;
using System.Collections;

public class CreateForce : MonoBehaviour
{
    public float power;
    public float playerHitPowerRatio;
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

            if (useCollisionAngle)
            {
                forceVector = (other.transform.position - hitSource.position).normalized * power;
            }

            ball.SendFlying(new Vector2(forceVector.x, forceVector.y));
            movement.Lag((int) (forceVector.magnitude / 5f));
        }
        else if (other.tag == TagManager.player)
        {
            Movement player = other.GetComponent<Movement>();
            Vector2 forceVector = direction.normalized * power * playerHitPowerRatio;
            Vector2 recoil = direction.normalized * -1f * power * playerHitPowerRatio;

            if (useCollisionAngle)
            {
                forceVector = (other.transform.position - hitSource.position).normalized * power * playerHitPowerRatio;
                recoil = (hitSource.position - other.transform.position).normalized * power * playerHitPowerRatio;
            }

            player.Knockback(new Vector2(forceVector.x, forceVector.y));
            movement.Knockback(new Vector2(recoil.x, recoil.y));
        }
    }
}
