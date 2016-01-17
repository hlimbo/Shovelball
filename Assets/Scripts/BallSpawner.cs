using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallSpawner : MonoBehaviour {

    public static List<Ball> ballList;
    public Ball ballPrefab;
    public int maxBallCount;
    public string tag;


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
            ballList[i].gameObject.tag = ballList[i].gameObject.tag + tag;
        }
    }

    private void ActivateBall(Ball ball)
    {
        ball.enabled = true;
        ball.gameObject.SetActive(true);
        ball.gameObject.transform.position = this.transform.position;
    }


    private void DisableBall(Ball ball)
    {
        ball.enabled = false;
        ball.gameObject.SetActive(false);
        ball.gameObject.transform.position = Vector2.zero;
    }



    void Awake()
    {
        GeneratePool();
        Debug.Log(ballList.Count);
    }


	// Use this for initialization
	void Start () {

        ActivateBall(ballList[0]);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
