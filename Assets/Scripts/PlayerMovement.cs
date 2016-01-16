using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {


    private Rigidbody2D body;
    private float moveVelocity;

    public float moveSpeed;
    public float jumpSpeed;



	// Use this for initialization
	void Start () {
        body = GetComponent<Rigidbody2D>();
        moveVelocity = 0f;
	}
	
	// Update is called once per frame
	void Update () {

        moveVelocity = moveSpeed * Input.GetAxisRaw("Horizontal");

        body.velocity = new Vector2(moveVelocity, body.velocity.y);

        //if (Input.GetKey(KeyCode.RightArrow))
        //{
            
        //}

       

	
	}
}
