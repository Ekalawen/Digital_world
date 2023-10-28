using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager_Exponential : ScoreManager {

    public int scoreIncrement = 1;
    public int scoreIncrementIncrement = 1;
    public float downOffset = 30.0f;

    protected int scoreMultiplier = 2;

    protected override void InitializeScore() {
        currentScore = 0;
        UpdateDisplayer();
    }

    public override void SetMultiplier(int multiplier) {
        scoreIncrement *= multiplier;
        scoreIncrementIncrement *= multiplier;
        scoreMultiplier = multiplier;
    }

    public override void OnNewBlockCrossed() {
        currentScore += scoreIncrement;
        displayer.AddVolatileText($"+ {scoreIncrement}", displayer.GetTextColor());
        UpdateDisplayer();
    }

    public override void OnCatchData() {
        scoreIncrement += scoreIncrementIncrement;
        displayer.AddVolatileText($"+ {scoreIncrementIncrement}", displayer.GetTextColor(), downOffset);
        UpdateDisplayer();
    }

    public override void OnNewTresholdCrossed() {
        scoreIncrement *= scoreMultiplier;
        scoreIncrementIncrement *= scoreMultiplier;
        displayer.AddVolatileText($"× {scoreMultiplier} !!!", displayer.GetTextColor(), downOffset);
        UpdateDisplayer();
    }
}
