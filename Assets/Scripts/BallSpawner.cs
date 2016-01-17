using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallSpawner : MonoBehaviour {

    public static List<Ball> ballList;
    public Ball ballPrefab;
    public int maxBallCount;

    //controls
    public float xLaunchVelocity;
    public float yLaunchVelocity;
    public float LaunchDelay;

    private bool coroutinesDone;

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
    }


    public void DisableBall(Ball ball)
    {
        ball.enabled = false;
        ball.gameObject.SetActive(false);
        ball.gameObject.transform.position = Vector2.zero;
        ball.TagNumber = Ball.INACTIVE;
    }

    public bool IsBallDisabled(Ball ball)
    {
        return ball.enabled == false || ball.gameObject.activeInHierarchy == false;
    }

    private void SetBallVelocity(Ball ball, float? x, float? y)
    {
        Rigidbody2D rb = ball.gameObject.GetComponent<Rigidbody2D>();
        rb.velocity = PhysicsUtility.SetVelocity(rb.velocity, x,y);
    }

    public IEnumerator SpawnBall(Ball ball, float waitTime)
    {
        ActivateBall(ball);
        yield return new WaitForSeconds(waitTime);
    }


    private IEnumerator SpawnBalls(float waitTime)
    {
        foreach(Ball b in ballList)
        {
            ActivateBall(b);
            yield return new WaitForSeconds(waitTime);
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

    private IEnumerator Delay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }




    void Awake()
    {
        GeneratePool();
        coroutinesDone = false;
        //Debug.Log(ballList.Count);
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(SpawnBalls(LaunchDelay));
        StartCoroutine(ApplyVelocities(LaunchDelay,xLaunchVelocity,yLaunchVelocity));
        coroutinesDone = true;

    }

    void Update()
    {
        //if disabled balls
        //if (coroutinesDone)
        //{
        //    foreach (Ball ball in ballList)
        //    {
        //        if (IsBallDisabled(ball))
        //        {
        //            Delay(LaunchDelay);
        //            ActivateBall(ball);
        //            SetBallVelocity(ball, xLaunchVelocity, yLaunchVelocity);
        //        }
        //    }
        //}

        

    }


}
