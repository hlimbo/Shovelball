using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class CanvasController : MonoBehaviour {

    private Text timerText;
    private Text playerOneScore;
    private Text playerTwoScore;

    void Start()
    {
        Text[] textPals = GetComponentsInChildren<Text>();
        timerText = textPals.Where(c => c.name == "TimerText").First();
        playerOneScore = textPals.Where(c => c.name == "ScorePlyrOne").First();
        playerTwoScore = textPals.Where(c => c.name == "ScorePlyrTwo").First();
    }
    public void updateTimer(float timeLeft)
    {
        int minutes = Mathf.FloorToInt(timeLeft/60);
        int seconds = Mathf.FloorToInt(timeLeft%60);

        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }
    public void updateScore(int player, int score)
    {
        if (player == 0) // player one
        {
            playerOneScore.text = score.ToString();
        }
        else if (player == 1) // player two
        {
            playerTwoScore.text = score.ToString();
        }
    }
}
