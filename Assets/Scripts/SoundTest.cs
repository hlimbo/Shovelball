using UnityEngine;
using System.Collections;

public class SoundTest : MonoBehaviour {

    public AudioClip moveSound;

    private AudioSource source;


	// Use this for initialization
	void Awake () {

        source = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

        source.PlayOneShot(moveSound, 1.0f);

	}
}
