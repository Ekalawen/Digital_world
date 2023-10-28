using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager_Quadratic : ScoreManager {

    public int scoreIncrement = 1;
    public int scoreIncrement2 = 5;
    public int scoreIncrement3 = 5;
    public int scoreIncrement4 = 5;
    public float downOffset = 30.0f;

    protected int scoreMultiplier = 2;
    protected bool hasAlreadyDoubleScoreIncrement4 = false;

    protected override void InitializeScore() {
        currentScore = 0;
        UpdateDisplayer();
    }

    public override void SetMultiplier(int multiplier) {
        scoreMultiplier = multiplier;
        scoreIncrement *= scoreMultiplier;
        scoreIncrement2 *= scoreMultiplier;
        scoreIncrement3 *= scoreMultiplier;
    }

    public override void OnNewBlockCrossed() {
        currentScore += scoreIncrement;
        displayer.AddVolatileText($"+ {scoreIncrement}", displayer.GetTextColor());
        UpdateDisplayer();
    }

    public override void OnCatchData() {
        scoreIncrement += scoreIncrement2;
        displayer.AddVolatileText($"++ {scoreIncrement2}", displayer.GetTextColor(), downOffset);
        UpdateDisplayer();
    }

    public override void OnNewTresholdCrossed() {
        scoreIncrement2 += scoreIncrement3;
        displayer.AddVolatileText($"+++ {scoreIncrement3} !!!", gm.console.basicColor, downOffset * 2);
        scoreIncrement3 += scoreIncrement4;
        if(!hasAlreadyDoubleScoreIncrement4) {
            scoreIncrement4 *= 2;
            hasAlreadyDoubleScoreIncrement4 = true;
        }
        UpdateDisplayer();
    }

    protected override string SpecificDisplayScore() {
        return $"(+{scoreIncrement}, ++{scoreIncrement2}, +++{scoreIncrement3}) ";
    }
}
