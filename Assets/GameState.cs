using UnityEngine;
using System.Collections;
using System.Linq;

public class GameState : MonoBehaviour {
    public int NUM_OF_PLAYERS = 2;
    public int timeLeft = 90; // in seconds

    private int[] scores;


	// Use this for initialization
	void Start () {
        scores = Enumerable.Repeat<int>(0, NUM_OF_PLAYERS).ToArray<int>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void increaseScore(int player)
    {
        scores[player] += 1;
        //whatever flashy thing you want to add in a score
    }

    public void removeBall(Collider2D ball)
    {
        Destroy(ball);
    }
}
