using UnityEngine;
using System.Collections;

public class Camera : MonoBehaviour {
    public float shakeScale = 1.0f; //multiplier of shake added by scripts
    public float decrementFactor = 1.0f; //amount of shake decremented per frame
    public float randomIntensity = 10.0f; //how erratic the camera shakes

    private Vector3 originPosition;
    private float shakeLeft = 0.0f;

	// Use this for initialization
	void Start () {
        originPosition = transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (shakeLeft > 0)
        {
            //Add perlin noise to the current camera position
            float shakeX = (2 * Mathf.PerlinNoise(Time.time * randomIntensity, 0.0f) - 1) * shakeLeft;
            float shakeY = (2 * Mathf.PerlinNoise(0.0f, Time.time * randomIntensity) - 1) * shakeLeft;
            shakeLeft -= decrementFactor;

            transform.position = new Vector3(originPosition.x + shakeX, originPosition.y = shakeY, originPosition.z);

            if (shakeLeft <= 0)
            {
                resetCamera();
            }
        }

	}

    public void addScreenShake (float shakeAmount) //Call this to begin the screen shaking
    {
        shakeLeft += shakeScale * shakeAmount;
    }

    private void resetCamera ()
    {
        transform.position = originPosition;
        shakeLeft = 0.0f;
    }
}
