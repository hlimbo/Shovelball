using UnityEngine;
using System.Collections;

public class PlayerBallBounds : MonoBehaviour {


    public GameObject ballSpawner;
    public GameObject[] players;
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

        //disable player
        if(other.gameObject.tag == TagManager.player)
        {
            other.GetComponent<Movement>().enabled = false;
            other.gameObject.SetActive(false);            
        }
    }


	

}
