using UnityEngine;
using System.Collections.Generic;

public static class PhysicsUtility
{
    public static Vector2 ApplyAccelerations(Vector2 currentVelocity, Dictionary<string, Acceleration>.ValueCollection accelerations)
    {
        foreach (Acceleration accel in accelerations)
        {
            // Get new target velocity
            Vector2 targetVelocity = new Vector2(currentVelocity.x, currentVelocity.y);

            // If the target velocity has a non-null value, then use its value
            if (accel.maxVelX.HasValue)
                targetVelocity.x = accel.maxVelX.GetValueOrDefault();
            if (accel.maxVelY.HasValue)
                targetVelocity.y = accel.maxVelY.GetValueOrDefault();

            // Linearly interpolate towards the new velocity
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accel.magnitude * Time.fixedDeltaTime);
        }
        return currentVelocity;
    }

    // NULL values are PASS-THROUGH. AKA they do NOT change the player velocity.
    public static Vector2 SetVelocity(Vector2 currentVelocity, float? x, float? y)
    {
        Vector2 newVelocity = new Vector2(currentVelocity.x, currentVelocity.y);
        if (x.HasValue)
            newVelocity.x = x.GetValueOrDefault();
        if (y.HasValue)
            newVelocity.y = y.GetValueOrDefault();
        return newVelocity;
    }
}
