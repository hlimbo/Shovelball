using UnityEngine;
using System.Collections;

public class PlayerBallBounds : MonoBehaviour {


    public GameObject ballSpawner;
    public GameObject spawnPoint;
    public GameObject spawnPoint2;
    public GameObject[] players;
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
            //Debug.Log("sup");
            Ball ball = other.gameObject.GetComponent<Ball>();
            spawner.DisableBall(ball);
            //Debug.Log("Ontriggerexit : " + spawner.activeBallCount);
        }

        //disable player
        if(other.gameObject.tag == TagManager.player || other.gameObject.tag == TagManager.player + "(Clone)")
        {
            randomSpawnPoint = Random.Range(1, 100);
            Debug.Log(randomSpawnPoint);
            if (randomSpawnPoint > 50)
                other.transform.position = spawnPoint.transform.position;
            else
                other.transform.position = spawnPoint2.transform.position;
            
            //other.transform.position = spawnPoint.transform.position;
        }
    }


	

}
