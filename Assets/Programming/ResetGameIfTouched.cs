﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetGameIfTouched : MonoBehaviour
{
    public float TimeFromTouchToRestartInSeconds = 1f;
    Scoreboard scoreboard;

    bool alreadyRecordedTheWinner;

    void Start()
    {
        scoreboard = FindObjectOfType<Scoreboard>();
    }

    void OnTriggerEnter(Collider collidedWith)
    {
        if (alreadyRecordedTheWinner) {
            return;
        }

        //Debug.Log(string.Format("{0} touched the floor!", collidedWith));
        StartCoroutine(RestartTheGameAfterSeconds());

        if (collidedWith.CompareTag("Player"))
        {
            scoreboard.RecordEnemyWin();
            alreadyRecordedTheWinner = true;
        }
        else
        {
            scoreboard.RecordPlayerWin();
            alreadyRecordedTheWinner = true;
        }
    }

    IEnumerator RestartTheGameAfterSeconds()
    {
        yield return new WaitForSeconds(TimeFromTouchToRestartInSeconds);

        SceneManager.LoadScene(0);
    }
}
