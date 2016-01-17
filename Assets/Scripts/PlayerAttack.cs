using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerAttack : MonoBehaviour {
    public float knockback;
    public int windupFrames = 2; // startup frames where player has no action
    public int attackFrames = 4; // how long the hitbox lasts
    public int firstActionableFrame = 6; // how long the player waits til they have control again, counting from the start
    public string attackInput;
 
	public int horizontalXSize = 4;
	public int horizontalYSize = 4;
	public int horizontalXDistance = 2;
	public int horizontalYDistance = 2;

    public bool isAttacking = false;
	public bool isDebug = false;

    private int windupLeft = 0;
    private int attackLeft = 0;
    private int disabledLeft = 0;

	private Texture greenDot;

	void Start()
	{
		if (isDebug) {
			greenDot = Resources.Load ("greendot.png") as Texture;
		}
	}

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
				Vector2 startRect = new Vector2(transform.position.x + horizontalXDistance - horizontalXSize / 2, transform.position.y + horizontalYDistance - horizontalYSize / 2);
				Vector2 endRect = new Vector2(transform.position.x + horizontalXDistance + horizontalXSize / 2, transform.position.y + horizontalYDistance + horizontalYSize / 2);

				if (isDebug) {
					//Draw a rectangle to visualize hitbox
					Gizmos.DrawGUITexture(new Rect(transform.position.x + horizontalXDistance + horizontalXSize / 2, transform.position.y + horizontalYDistance + horizontalYSize / 2, horizontalXSize, horizontalYSize), greenDot);
				}
				Collider2D[] OverlappedColliders = Physics2D.OverlapAreaAll (startRect, endRect);
				Collider2D[] PlayersFound = OverlappedColliders.Where (c => c.tag == "Player").ToArray ();
				Collider2D[] BallsFound = OverlappedColliders.Where (c => c.name == "TestArrow").ToArray ();

				if (BallsFound.Length > 0) {
					Debug.Log ("I Hit a thing!");
				}
                attackLeft--;
            }
            if(disabledLeft <= 0 & attackLeft <= 0)
            {
                isAttacking = false;
            }
        }
    }

}
