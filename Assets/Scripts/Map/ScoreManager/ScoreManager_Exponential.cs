using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager_Exponential : ScoreManager {

    public float downOffset = 30.0f;

    protected int scoreMultiplier = 2;

    protected override void InitializeScore() {
        currentScore = 0;
        UpdateAllDisplayersInstantly();
    }

    public override void SetMultiplier(int multiplier) {
        scoreIncrement *= multiplier;
        scoreIncrement2 *= multiplier;
        scoreMultiplier = multiplier;
        UpdateAllDisplayersInstantly();
    }

    public override void OnNewBlockCrossed() {
        currentScore += scoreIncrement;
        scoreDisplayer.AddVolatileText($"+ {scoreIncrement}", scoreDisplayer.GetTextColor());
        scoreDisplayerUpdater.UpdateValue();
    }

    public override void OnCatchData() {
        scoreIncrement += scoreIncrement2;
        incrementDisplayer.AddVolatileText($"+ {scoreIncrement2}", incrementDisplayer.GetTextColor());
        incrementDisplayerUpdater.UpdateValue();
    }

    public override void OnNewTresholdCrossed() {
        scoreIncrement *= scoreMultiplier;
        scoreIncrement2 *= scoreMultiplier;
        increment2Displayer.AddVolatileText($"× {scoreMultiplier} !!!", increment2Displayer.GetTextColor());
        increment2DisplayerUpdater.UpdateValue();
    }
}
