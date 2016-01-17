using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {
    public int assignedPlayer;

    private GameState gstate;
    private CameraController ccont;

    void Start()
    {
        gstate = Object.FindObjectOfType<GameState>().GetComponent<GameState>();
        ccont = Object.FindObjectOfType<CameraController>();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == TagManager.ball)
        {
            gstate.increaseScore(assignedPlayer);
            //ThrowFlashyEffects
            ccont.addScreenShake(10);
            //explode the ball
            gstate.removeBall(other);
        }
    }
}
