using UnityEngine;
using System.Collections;
using System.Linq;

public class GameState : MonoBehaviour {
    public int NUM_OF_PLAYERS = 2;
    public int gameTime = 90;
    public float countDown = 3;
    public int showFight = 3;

    private bool inCountdown;
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

        inCountdown = true;
        gameActive = false;

        bspawner = Object.FindObjectOfType<BallSpawner>();
        ccont = Object.FindObjectOfType<CanvasController>();

        //REMOVE KEBAB FROM SCORE
        ccont.updateScore(0, 0); ccont.updateScore(1, 0);
        ccont.showCountdown(countDown.ToString());
	}

    void Update()
    {
        ccont.updateTimer(timeLeft);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inCountdown)
        {
            if (countDown < 0)
            {
                ccont.showCountdown("ROUND START");
                inCountdown = false;
                startGame();
            }
            else
            {
                countDown -= Time.deltaTime;
                ccont.showCountdown(((int) Mathf.Ceil(countDown)).ToString());
            }
        }
        else if (gameActive)
        {
            if (timeLeft < (gameTime - showFight))
                ccont.showCountdown("");
            timeLeft -= Time.deltaTime;
            if(timeLeft <= 0)
            {
                endGame();
            }
        }
        if (gameOver && LazyInputManager.GetInput(0).attackForward && LazyInputManager.GetInput(1).attackForward
            && LazyInputManager.GetInput(0).jump && LazyInputManager.GetInput(1).jump)
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    public void startGame()
    {
        bspawner.isGameOver = false;
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
        if (!gameOver)
        {
            scores[player] += 1;
            //whatever flashy thing you want to add in a score
            ccont.updateScore(player, scores[player]);
        }
    }

    public void removeBall(Collider2D ball)
    {
        //Get direction to face
        bspawner.DisableBall(ball.GetComponent<Ball>());
        bspawner.activeBallCount--;
    }

    private void endGame()
    {
        gameActive = false;
        gameOver = true;
        timeLeft = 0;
        int winner = 0;
        if (scores[0] > scores[1])
            winner = 1;
        else if (scores[0] < scores[1])
            winner = 2;
        ccont.showGameOver(winner);
        bspawner.isGameOver = true;
    }
}
