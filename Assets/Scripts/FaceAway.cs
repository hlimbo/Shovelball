using UnityEngine;
using System.Collections;

public class FaceAway : MonoBehaviour {

    public int assignedPlayer;

    private ParticleSystem ps;
    private Ball b;
	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
        b = GetComponentInParent<Ball>();
	}
	
	// Update is called once per frame
	void Update () {
        //Always face opposite direction of parent's movement
        Vector2 ballv = GetComponentInParent<Rigidbody2D>().velocity;
        float angle = (Mathf.Atan2(ballv.y, ballv.x) / (2 * Mathf.PI)) * 360;
        transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);

        //Enable and disable the emitter
        if(ps.isStopped && b.isFlying)
        {
            ps.Play();
        }
        else if(ps.isPlaying && !b.isFlying)
        {
            ps.Stop();
        }
    }
}
