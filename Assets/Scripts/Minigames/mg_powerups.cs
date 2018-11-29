﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mg_powerups : AbstractMinigame
{
    //temp for testing
    [SerializeField] GameObject[] powerups;
    [SerializeField] GameObject[] ItemSpawnLocations;
    public int NumSpawns = 4;

    public override void RunMinigame(){
        SetMinigameToRunning();
        MinigameAliveTimer = 0f;

        ItemSpawnLocations = GameObject.FindGameObjectsWithTag("ItemSpawn");
        
        for(int i = 0; i < NumSpawns; i++){
            Instantiate(powerups[Random.Range(0, powerups.Length)], 
                            ItemSpawnLocations[Random.Range(0, 
                            ItemSpawnLocations.Length)].transform.position, 
                            Quaternion.identity);
        }
    }
}
