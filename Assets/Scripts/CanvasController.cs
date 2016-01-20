using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class CanvasController : MonoBehaviour {

    public Text timerText;
    public Text playerOneScore;
    public Text playerTwoScore;
    public Text GameOverText;
    public Text RedWinsText;
    public Text BlueWinsText;
    public Text TieText;
    public Text InstructionText;
    public Text Countdown;


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
    public void showGameOver(int winner)
    {
        //0 is tie, 1 is blue, 2 is red
        GameOverText.gameObject.SetActive(true);
        InstructionText.gameObject.SetActive(true);
        switch (winner)
        {
            case 0:
                TieText.gameObject.SetActive(true);
                break;
            case 1:
                BlueWinsText.gameObject.SetActive(true);
                break;
            case 2:
                RedWinsText.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
    public void showCountdown(string count)
    {
        Countdown.text = count;
    }
}
