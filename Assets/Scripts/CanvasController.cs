using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class CanvasController : MonoBehaviour {

    private Text timerText;

    void Start()
    {
        timerText = GetComponentsInChildren<Text>().Where(c => c.name == "TimerText").First();
    }
    public void updateTimer(float timeLeft)
    {
        int minutes = Mathf.FloorToInt(timeLeft/60);
        int seconds = Mathf.FloorToInt(timeLeft%60);

        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }
}
