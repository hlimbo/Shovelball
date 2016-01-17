using UnityEngine;
using System.Collections;
using System.Linq;

public class GameState : MonoBehaviour {
    public int NUM_OF_PLAYERS = 2;
    public int gameTime = 90;

    private int timeLeft;
    public int[] scores;
    public int scoreof0viewer;


	// Use this for initialization
	void Start () {
        scores = Enumerable.Repeat(0, NUM_OF_PLAYERS).ToArray();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        scoreof0viewer = scores[0];
	}

    public void increaseScore(int player)
    {
        scores[player] += 1;
        //whatever flashy thing you want to add in a score
    }

    public void removeBall(Collider2D ball)
    {
        Destroy(ball.gameObject);
    }
}
