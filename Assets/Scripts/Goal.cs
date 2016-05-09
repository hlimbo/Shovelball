using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {
    public int assignedPlayer;
    public ParticleSystem scoreExplosion;
    public AudioSource pointSound;
    public AudioSource goalSound;

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
            Vector2 ballv = other.GetComponent<Rigidbody2D>().velocity;

            Vector3 scoreLocation = other.transform.position;
            ParticleSystem boom = Instantiate(scoreExplosion, scoreLocation, scoreExplosion.transform.rotation) as ParticleSystem;
            float angle = (Mathf.Atan2(ballv.y, ballv.x) / (2 * Mathf.PI)) * 360;
            boom.transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);

            ccont.addScreenShake(10);
            //explode the ball
            gstate.removeBall(other);
            pointSound.Play();
            goalSound.Play();
        }
    }
}
