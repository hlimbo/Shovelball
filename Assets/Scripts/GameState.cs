using UnityEngine;
using System.Collections;
using System.Linq;

public class GameState : MonoBehaviour {
    public int NUM_OF_PLAYERS = 2;
    public int gameTime = 90;

    private bool gameActive;
    private bool gameOver;
    public float timeLeft;
    private int[] scores;

    private BallSpawner bspawner;
    private CanvasController ccont;

	// Use this for initialization
	void Start () {
        scores = Enumerable.Repeat(0, NUM_OF_PLAYERS).ToArray();
        timeLeft = gameTime;
        gameOver = false;

        bspawner = Object.FindObjectOfType<BallSpawner>();
        ccont = Object.FindObjectOfType<CanvasController>();

        //REMOVE KEBAB FROM SCORE
        ccont.updateScore(0, 0); ccont.updateScore(1, 0);
        startGame();
	}

    void Update()
    {
        ccont.updateTimer(timeLeft);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameActive)
        {
            timerCountdown();
            if(timeLeft <= 0)
            {
                endGame();
            }
        }
    }

    public void startGame()
    {
        gameActive = true;
    }

    public bool isGameOver()
    {
        return gameOver;
    }

    public bool isGameOngoing()
    {
        return gameActive;
    }

    public void increaseScore(int player)
    {
        scores[player] += 1;
        //whatever flashy thing you want to add in a score
        ccont.updateScore(player, scores[player]);
    }

    public void removeBall(Collider2D ball)
    {
        //Get direction to face
        bspawner.DisableBall(ball.GetComponent<Ball>());
        bspawner.activeBallCount--;
    }

    private void timerCountdown()
    {
        timeLeft -= Time.deltaTime;
    }

    private void endGame()
    {
        gameActive = false;
        gameOver = true;
        timeLeft = 0;
    }
}
