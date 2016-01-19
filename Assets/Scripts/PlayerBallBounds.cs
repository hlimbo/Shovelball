using UnityEngine;
using System.Collections;

public class PlayerBallBounds : MonoBehaviour {


    public GameObject ballSpawner;
    public GameObject spawnPoint;
    public GameObject spawnPoint2;
    public GameObject player1;
    public GameObject player2;
    private BallSpawner spawner;

    int randomSpawnPoint;

    void Start()
    {
        spawner = ballSpawner.GetComponent<BallSpawner>();   
    }

	void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == TagManager.ball || other.gameObject.tag == TagManager.ball + "(Clone)")
        {
            Ball ball = other.gameObject.GetComponent<Ball>();
            spawner.DisableBall(ball);
        }

        //disable player
        if(other.gameObject.tag == TagManager.player || other.gameObject.tag == TagManager.player + "(Clone)")
        {
            if (other.gameObject == player1)
                other.transform.position = spawnPoint.transform.position;
            else
                other.transform.position = spawnPoint2.transform.position;
            //randomSpawnPoint = Random.Range(1, 100);
            //Debug.Log(randomSpawnPoint);
            //if (randomSpawnPoint > 50)
            //    other.transform.position = spawnPoint.transform.position;
            //else
            //    other.transform.position = spawnPoint2.transform.position;
            
            //other.transform.position = spawnPoint.transform.position;
        }
    }


	

}
