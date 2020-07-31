using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterMinigame : Minigame
{
    bool isServer = false;
    List<GameObject> enteredPlayers = new List<GameObject>();

    void Start()
    {
        isServer = GameObject.Find("LocalPlayer").GetComponent<PlayerNet>().isServer;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player") && isServer && !enteredPlayers.Contains(col.gameObject))
        {
            enteredPlayers.Add(col.gameObject);
            var allWasIn = true;
            foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (!enteredPlayers.Contains(p))
                {
                    allWasIn = false;
                    break;
                }
            }
            if (allWasIn)
                Finish();
        }
    }
}
