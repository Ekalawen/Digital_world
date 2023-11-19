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
        string scoreIncrementString = scoreDisplayerUpdater.ApplyToCreditsFormating(scoreIncrement);
        scoreDisplayer.AddVolatileText($"+ {scoreIncrementString}", scoreDisplayer.GetTextColor());
        scoreDisplayerUpdater.UpdateValue();
    }

    public override void OnCatchData() {
        scoreIncrement += scoreIncrement2;
        string scoreIncrement2String = incrementDisplayerUpdater.ApplyToCreditsFormating(scoreIncrement2);
        incrementDisplayer.AddVolatileText($"+ {scoreIncrement2String}", incrementDisplayer.GetTextColor());
        incrementDisplayerUpdater.UpdateValue();
    }

    public override void OnNewTresholdCrossed() {
        scoreIncrement *= scoreMultiplier;
        scoreIncrement2 *= scoreMultiplier;
        string scoreMultiplierString = increment2DisplayerUpdater.ApplyToCreditsFormating(scoreMultiplier);
        increment2Displayer.AddVolatileText($"× {scoreMultiplier} !!!", increment2Displayer.GetTextColor());
        increment2DisplayerUpdater.UpdateValue();
    }
}
