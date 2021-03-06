﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class Game : MonoBehaviour {

    [System.Serializable]
    public class Wave
    {
        // Note: add to this in the future for any wave attributes
        public int waveSize;
        public int numberOfWaves;
    }

    public enum GameState
    { START, DEFEND, REST, END }

    public GameState currentState;
    public float defendDuration;
    public float restDuration;
    public GameObject[] enemies;

    // Information regarding how waves behave (i.e. What and How Many enemies gets spawned)
	[Header("Current Wave Information")]
	public int wave;
    public Wave waveInfo;

    GameObject playerObj;
    PlayerManager playerMngr;
    SpawnPoint[] enemySpawners;

    private float timer;
    private float spawnTimer;
    private int enemiesRemaining;
    private float waveTimer;

    // Use this for initialization
    void Start () {
        playerObj = GameObject.Find("Player");
        enemySpawners = GameObject.FindObjectsOfType<SpawnPoint>();

        playerMngr = playerObj.GetComponent<PlayerManager>();

        // Disable the player initially
        playerObj.GetComponent<PlayerMovement>().enabled = false;

		wave = 1;
        currentState = GameState.START;
	}
	
	// Update is called once per frame
	void Update () {
		switch (currentState)
        {
            case GameState.START:
                if (Input.GetKeyDown(KeyCode.Return))
                {
					
                    timer = 0.0f;
                    spawnTimer = 0.0f;
                    currentState = GameState.DEFEND;
                    // Enable the player
                    playerObj.GetComponent<PlayerMovement>().enabled = true;

                    waveTimer = (defendDuration / waveInfo.numberOfWaves);

                    Debug.Log("Game State: " + currentState.ToString());
                }
                break;
            case GameState.DEFEND:

                // Update the time
                timer += Time.deltaTime;
                spawnTimer += Time.deltaTime;

                // If the player dies
                if (playerMngr.currentHealth <= 0)
                {
                    currentState = GameState.END;
                    ClearMapEntities();

                    Debug.Log("Game State: " + currentState.ToString());
                }

                // Split wave spawn rate to = duration of wave / number of waves
                if (spawnTimer > waveTimer && timer <= defendDuration)
                {
                    SpawnNextWave();
                    spawnTimer = 0.0f;

                    Debug.Log("Spawning Wave...");
                }

                enemiesRemaining = GameObject.FindGameObjectsWithTag("Enemy").Length;

                // End of Defend State
                if (timer > defendDuration && enemiesRemaining <= 0)
                { 
                    timer = 0.0f;
                    ClearMapEntities();
					wave++;
                    currentState = GameState.REST;

                    waveInfo.waveSize += 1;
                    waveInfo.numberOfWaves += 1;

                    Debug.Log("Game State: " + currentState.ToString());
                }
                break;
            case GameState.REST:
                timer += Time.deltaTime;

                // End of Rest State
                if (timer > restDuration)
                {
                    timer = 0.0f;
                    currentState = GameState.DEFEND;

                    waveTimer = (defendDuration / waveInfo.numberOfWaves);

                    Debug.Log("Game State: " + currentState.ToString());
                }
                break;
            case GameState.END:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                break;
        }
	}

    void ClearMapEntities()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject e in enemies)
        {
            Destroy(e);
        }
    }

    // Next wave to spawn.
    // NOTE: Number of enemies per wave is also base off of the number of spawners that we have
    //          on the map.
    void SpawnNextWave()
    {
        GameObject[] wave = new GameObject[waveInfo.waveSize];
        for (int i = 0; i < waveInfo.waveSize; i++)
        {
            int enemyIdx = (int)UnityEngine.Random.Range(0, enemies.Length);
            wave[i] = enemies[enemyIdx];
        }

        foreach (SpawnPoint sp in enemySpawners)
        {
            sp.SpawnWave(wave);
        }
    }
}
