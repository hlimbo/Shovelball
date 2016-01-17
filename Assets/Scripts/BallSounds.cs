using UnityEngine;
using System.Collections;

public class BallSounds : MonoBehaviour
{

    public AudioClip hitSound;
    private AudioSource source;

    public bool canPlaySound;

    // Use this for initialization
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.clip = hitSound;
    }

    // Update is called once per frame
    void Update()
    {
       // source.PlayOneShot(hitSound, 25.0f);

    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("hello");
        //source.PlayOneShot(hitSound, 25.0f);

    }

    private IEnumerator PlaySoundOverTime()
    {
        if (canPlaySound)
        {
           
            canPlaySound = false;
        }

        yield return new WaitForSeconds(hitSound.length);
    }

}
