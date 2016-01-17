using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {
    public int assignedPlayer;

    private GameState gstate;

    void Start()
    {
        gstate = Object.FindObjectOfType<GameState>().GetComponent<GameState>();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == TagManager.ball)
        {
            gstate.increaseScore(assignedPlayer);
            //ThrowFlashyEffects
            gstate.removeBall(other);
        }
    }
}
