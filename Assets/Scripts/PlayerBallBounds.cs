using UnityEngine;
using System.Collections;

public class PlayerBallBounds : MonoBehaviour {


    public GameObject ballSpawner;
    //old spawner
    public GameObject[] players;
    private BallSpawner spawner;
    
    //new spawner with pooling.
    public GameObject playerSpawner;

    void Start()
    {
        spawner = ballSpawner.GetComponent<BallSpawner>();
    }

	void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == TagManager.ball || other.gameObject.tag == TagManager.ball + "(Clone)")
        {
            Debug.Log("sup");
            Ball ball = other.gameObject.GetComponent<Ball>();
            spawner.DisableBall(ball);
            Debug.Log("Ontriggerexit : " + spawner.activeBallCount);
        }

        //disable player
        if(other.gameObject.tag == TagManager.player || other.gameObject.tag == TagManager.player + "(Clone)")
        {
            playerSpawner.GetComponent<PlayerSpawner>().currentPlayerCount--;
            playerSpawner.GetComponent<PlayerSpawner>().DisablePlayer(other.GetComponent<Movement>());           
        }
    }


	

}
