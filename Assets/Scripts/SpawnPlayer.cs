using UnityEngine;
using System.Collections;

public class SpawnPlayer : MonoBehaviour {

    public GameObject[] players;
    
	// Update is called once per frame
	void Update () {
	    foreach(GameObject player in players)
        {
            if(player.GetComponent<Movement>().enabled == false)
            {
                player.GetComponent<Movement>().enabled = true;
                player.SetActive(true);
                player.transform.position = transform.position;
            }
        }
	}
}
