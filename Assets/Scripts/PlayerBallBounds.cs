using UnityEngine;
using System.Collections;

public class PlayerBallBounds : MonoBehaviour {


    public GameObject ballSpawner;
    private BallSpawner spawner;

    void Start()
    {
        spawner = ballSpawner.GetComponent<BallSpawner>();
    }

	void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == TagManager.ball)
        {
            Ball ball = other.gameObject.GetComponent<Ball>();
            spawner.DisableBall(ball);      
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
       

        if(other.gameObject.tag == TagManager.ball)
        {
            //ballSpawner.
            //StartCoroutine(,2.0f));
        }
    }
	

}
