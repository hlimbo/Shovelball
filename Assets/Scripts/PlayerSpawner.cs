using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour {

    public int numOfPlayers;
    public static int MIN_PLAYERS = 1;
    public int maxPlayers;
    public Movement playerPrefab;
    public GameObject spawnPoint;


    public static List<Movement> players;
    public int currentPlayerCount;


    private void GeneratePlayers()
    {
        players = new List<Movement>();

        for(int i = 0; i < numOfPlayers;++i)
        {
            players.Add(Instantiate(playerPrefab, Vector2.zero, Quaternion.identity) as Movement);

            players[i].transform.parent = this.transform;
            players[i].enabled = false;
            players[i].gameObject.SetActive(false);
            players[i].playerIndex = i + 1;
        }
    }

    public void ActivatePlayer(Movement player)
    {
        player.transform.position = spawnPoint.transform.position;
        player.enabled = true;
        player.gameObject.SetActive(true);
       
    }

    public void DisablePlayer(Movement player)
    {
        player.enabled = false;
        player.gameObject.SetActive(false);
    }

    public bool IsPlayerDisabled(Movement player)
    {
        return !player.gameObject.activeInHierarchy;
    }


    void Awake()
    {
        GeneratePlayers();
        currentPlayerCount = 0;
       // Debug.Log("Number of players: " + players.Count);
    }

	// Update is called once per frame
	void Update () {

        if(currentPlayerCount <= 0 || (currentPlayerCount < numOfPlayers || currentPlayerCount < maxPlayers))
        {
            foreach(Movement player in players)
            {
                if (IsPlayerDisabled(player))
                {
                    ActivatePlayer(player);
                    currentPlayerCount++;
                }
            }

           
        }

        //Debug.Log("Current Number of Players: " + currentPlayerCount);
	
	}
}
