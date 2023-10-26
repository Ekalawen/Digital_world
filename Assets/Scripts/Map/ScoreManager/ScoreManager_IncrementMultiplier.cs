using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager_IncrementMultiplier : ScoreManager {

    public int scoreIncrement = 1;
    public int scoreIncrementIncrement = 1;
    public float downOffset = 30.0f;

    protected override void InitializeScore() {
        currentScore = 0;
        UpdateDisplayer();
    }

    public override void SetMultiplier(int multiplier) {
        scoreIncrement *= multiplier;
        scoreIncrementIncrement *= multiplier;
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
        scoreIncrement *= 2;
        scoreIncrementIncrement *= 2;
        displayer.AddVolatileText($"× 2 !!!", displayer.GetTextColor(), downOffset);
        UpdateDisplayer();
    }
}
