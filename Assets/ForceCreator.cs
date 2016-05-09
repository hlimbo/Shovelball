using UnityEngine;
using System.Collections;

public class ForceCreator : MonoBehaviour {

    public float ballSpeed;
    public float playerSpeed;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            collision.rigidbody.velocity = new Vector2(0, ballSpeed);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            collision.rigidbody.velocity = new Vector2(0, playerSpeed);
        }
    }
}
