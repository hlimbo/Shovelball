using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallSpawner : MonoBehaviour {

    public static List<Ball> ballList;
    public Ball ballPrefab;
    public int maxBallCount;
    public int minBallCount;

    //controls
    public float xMinLaunchVelocity;
    public float yMinLaunchVelocity;
    public float xMaxLaunchVelocity;
    public float yMaxLaunchVelocity;
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
        activeBallCount--;
        ball.enabled = false;
        ball.gameObject.SetActive(false);
        ball.gameObject.transform.position = Vector2.zero;
        ball.TagNumber = Ball.INACTIVE;
      

        //zero out velocity.
        Rigidbody2D rb = ball.gameObject.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;

        //Debug.Log(activeBallCount);


    }

    public bool IsBallDisabled(Ball ball)
    {
        return  ball.gameObject.activeInHierarchy == false;
    }

    public Vector2 SetRandomVelocity()
    {
        int num = Random.Range(-50, 51);
        if (num == 0)
            num = 1;
        int dir = num / Mathf.Abs(num);
        float x = Random.Range(xMinLaunchVelocity,xMaxLaunchVelocity) * dir;
        float y = Random.Range(yMinLaunchVelocity, yMaxLaunchVelocity);

        return new Vector2(x, y);
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

    private IEnumerator ApplyVelocities(float waitTime)
    {
        foreach(Ball b in ballList)
        {
            Vector2 randomVel = SetRandomVelocity();
            //Debug.Log(randomVel.ToString());
            SetBallVelocity(b, randomVel.x, randomVel.y);
            yield return new WaitForSeconds(waitTime);
        }
    }
// ===================================================================  //
    
    public bool AllBallsInactive()
    {
       // int count = 0;

        foreach(Ball b in ballList)
        {
            if (IsBallDisabled(b))
                activeBallCount--;
        }

        return activeBallCount <= 0;
    }

   //use when you want balls spawning in clusters.
    private void LaunchBall()
    {

        foreach (Ball b in ballList)
        {
            if (IsBallDisabled(b))
            {
                ActivateBall(b);
                Vector2 randomVel = SetRandomVelocity();
                SetBallVelocity(b, randomVel.x, randomVel.y);
                return;
            }

        }
    }


    void Awake()
    {
        GeneratePool();
        activeBallCount = 0;
    }


    void Start()
    {
        //StartCoroutine(SpawnBalls(LaunchDelay));
        //StartCoroutine(ApplyVelocities(LaunchDelay));
    }

    void Update()
    {

        //if (activeBallCount >= maxBallCount)
        //    this.enabled = false;
        //spawn balls in a cluster.
        //Invoke("LaunchBall",1.0f);

        //spawn one at a time.
        StartCoroutine(SpawnBalls(LaunchDelay));
        StartCoroutine(ApplyVelocities(LaunchDelay));


    }


}
