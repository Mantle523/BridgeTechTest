using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameResult
{
    float attemptTime;
    int finalScore;
    int totalCollected;

    public GameResult(float time, int score, int total)
    {
        attemptTime = time;
        finalScore = score;
        totalCollected = total;
    }
}
