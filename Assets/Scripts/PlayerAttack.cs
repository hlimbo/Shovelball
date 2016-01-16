using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour {
    public float knockback;
    public int windupFrames = 2; // startup frames where player has no action
    public int attackFrames = 4; // how long the hitbox lasts
    public int firstActionableFrame = 6; // how long the player waits til they have control again, counting from the start
    public string attackInput;
 

    public bool isAttacking = false;

    private int windupLeft = 0;
    private int attackLeft = 0;
    private int disabledLeft = 0;

    void FixedUpdate()
    {
        //Check for button press
        if (!isAttacking)
        {
            if (Input.GetButtonDown(attackInput))
            {
                isAttacking = true;
                windupLeft = windupFrames; attackLeft = attackFrames; disabledLeft = firstActionableFrame;
            }
        }

        //If pressed, disable all other actions and continue with attack frames
        if (isAttacking)
        {
            disabledLeft--;
            if (windupLeft > 0)
            {
                windupLeft--;
            }
            else if (attackLeft > 0)
            {
                attackLeft--;
            }
            if(disabledLeft <= 0 & attackLeft <= 0)
            {
                isAttacking = false;
            }
        }
    }

}
