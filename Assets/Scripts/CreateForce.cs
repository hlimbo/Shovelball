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

            // Make the hits more predictable to 8 angles
            if (useCollisionAngle)
            {
                forceVector = other.transform.position - hitSource.position;
                float angleUp = Vector2.Angle(forceVector, Vector2.up);
                float angleRight = Vector2.Angle(forceVector, Vector2.right);

                // Snap the force vector to 8 angles around the unit circle
                /* 
                    * U  -> up < 15, right < 105
                    * UR -> up < 60, right < 75
                    * R  -> up < 120, right < 30
                    * DR -> up < 165, right < 75
                    * D  -> up < 180, right < 105
                    * DL -> up < 165, right < 150
                    * L  -> up < 120, right < 180
                    * UL -> up < 60, right < 150
                    */
                if (angleUp < 15 && angleRight < 105)
                {
                    forceVector = Vector2.up;
                }
                else if (angleUp < 60)
                {
                    if (angleRight < 75)
                        forceVector = (Vector2.up + Vector2.right).normalized;
                    else
                        forceVector = (Vector2.up + Vector2.left).normalized;
                }
                else if (angleUp < 120)
                {
                    if (angleRight < 30)
                        forceVector = Vector2.right;
                    else
                        forceVector = Vector2.left;
                }
                else if (angleUp < 165)
                {
                    if (angleRight < 75)
                        forceVector = (Vector2.down + Vector2.right).normalized;
                    else
                        forceVector = (Vector2.down + Vector2.left).normalized;
                }
                else
                {
                    forceVector = Vector2.down;
                }

                //Debug.Log(angleUp.ToString() + ", " + angleRight.ToString() + ": " + forceVector.ToString());

                // Apply the force
                if (ball.isGrounded && Vector2.Angle(forceVector, ball.SurfaceNormal()) > 90)
                {
                    forceVector = Vector2.Reflect(forceVector, ball.SurfaceNormal());
                }
                forceVector = forceVector * power;
            }
            ball.SendFlying(new Vector2(forceVector.x, forceVector.y));
            movement.Lag((int)(forceVector.magnitude / 5f));
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

            if (player.IsGrounded() && Vector2.Angle(forceVector, player.SurfaceNormal()) > 90)
            {
                forceVector = Vector2.Reflect(forceVector, player.SurfaceNormal());
            }

            player.Knockback(new Vector2(forceVector.x, forceVector.y));
            movement.Knockback(new Vector2(recoil.x, recoil.y));
        }
    }
}
