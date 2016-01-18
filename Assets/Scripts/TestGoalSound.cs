using UnityEngine;
using System.Collections;

public class TestGoalSound : MonoBehaviour
{

    public AudioSource source;
    public AudioClip sfxClip;

    // Use this for initialization
    void Awake()
    {
        source.GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        source.PlayOneShot(sfxClip);
    }
}
