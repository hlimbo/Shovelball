using UnityEngine;

public class Acceleration
{
    public float? maxVelX;
    public float? maxVelY;
    public float magnitude;

    public Acceleration(float? maxVelX, float? maxVelY, float magnitude)
    {
        this.maxVelX = maxVelX;
        this.maxVelY = maxVelY;
        this.magnitude = magnitude;
    }
}
