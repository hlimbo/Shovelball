using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallSpawner : MonoBehaviour {

    public static List<Ball> ballList;
    public Ball ballPrefab;
    public int maxBallCount;
    public int minBallCount;

    //controls
    public float xLaunchVelocity;
    public float yLaunchVelocity;
    public float LaunchDelay;

    public int activeBallCount;


    private void GeneratePool()
    {
        ballList = new List<Ball>(); 
        for(int i = 0;i < maxBallCount;++i)
        {
            ballList.Add(Instantiate(ballPrefab, Vector2.zero, Quaternion.identity) as Ball);
            //set Balls parent to spawner ~ share points
            ballList[i].transform.parent = this.transform;
            //disable Ball Script
            ballList[i].enabled = false;
            //disable GameObject.
            ballList[i].gameObject.SetActive(false);
            // ballList[i].gameObject.tag = ballList[i].gameObject.tag + tag;
            ballList[i].TagNumber = i;
        }
    }

    public void ActivateBall(Ball ball)
    {
        ball.enabled = true;
        ball.gameObject.SetActive(true);
        ball.gameObject.transform.position = this.transform.position;
        activeBallCount++;
    }


    public void DisableBall(Ball ball)
    {
        ball.enabled = false;
        ball.gameObject.SetActive(false);
        ball.gameObject.transform.position = Vector2.zero;
        ball.TagNumber = Ball.INACTIVE;
        activeBallCount--;

        //zero out velocity.
        Rigidbody2D rb = ball.gameObject.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;

    }

    public bool IsBallDisabled(Ball ball)
    {
        return  ball.gameObject.activeInHierarchy == false;
    }

    public void SetBallVelocity(Ball ball, float? x, float? y)
    {
        Rigidbody2D rb = ball.gameObject.GetComponent<Rigidbody2D>();
        if(rb.velocity == Vector2.zero)
            rb.velocity = PhysicsUtility.SetVelocity(rb.velocity, x,y);
    }

// =============== use when game starts only =================== //
    private IEnumerator SpawnBalls(float waitTime)
    {
        foreach (Ball b in ballList)
        {
            if (IsBallDisabled(b))
            {
                ActivateBall(b);
                yield return new WaitForSeconds(waitTime);
            }
        }
    }

    private IEnumerator ApplyVelocities(float waitTime,float? x,float? y)
    {
        foreach(Ball b in ballList)
        {
            SetBallVelocity(b, x, y);
            yield return new WaitForSeconds(waitTime);
        }
    }
// ===================================================================  //

   //use when you want balls spawning in clusters.
    private void LaunchBall()
    {

        foreach (Ball b in ballList)
        {
            if (IsBallDisabled(b))
            {
                ActivateBall(b);
                SetBallVelocity(b, xLaunchVelocity, yLaunchVelocity);
                return;
            }

        }
    }


    void Awake()
    {
        GeneratePool();
        activeBallCount = 0;
    }

    // Use this for initialization
    void Start()
    {
        //StartCoroutine(SpawnBalls(LaunchDelay));
  

    }

    void Update()
    {
        Debug.Log("Active ball count: " + activeBallCount);
        InvokeRepeating("LaunchBall", 0.1f,LaunchDelay);

        //if(activeBallCount <= 0)
        //{
        //    StartCoroutine(SpawnBalls(LaunchDelay));
        //    StartCoroutine(ApplyVelocities(LaunchDelay, xLaunchVelocity, yLaunchVelocity));
        //}

    }


}
