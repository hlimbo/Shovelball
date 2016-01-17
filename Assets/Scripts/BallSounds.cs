using UnityEngine;
using System.Collections;

public class BallSounds : MonoBehaviour
{

    public AudioClip hitSound;
    private AudioSource source;

    // Use this for initialization
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.clip = hitSound;
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        source.PlayOneShot(hitSound, 25.0f);

    }

}
