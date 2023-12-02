using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager_Quadratic : ScoreManager {

    public int scoreIncrement3 = 5;
    public int scoreIncrement4 = 5;
    public float downOffset = 30.0f;

    protected int scoreMultiplier = 2;
    protected bool hasAlreadyDoubleScoreIncrement4 = false;

    protected override void InitializeScore() {
        SetCurrentScore(0);
        UpdateAllDisplayersInstantly();
    }

    public override void SetMultiplier(int multiplier) {
        scoreMultiplier = multiplier;
        scoreIncrement *= scoreMultiplier;
        scoreIncrement2 *= scoreMultiplier;
        scoreIncrement3 *= scoreMultiplier;
        scoreIncrement4 *= scoreMultiplier;
        UpdateAllDisplayersInstantly();
    }

    public override void OnNewBlockCrossed() {
        SetCurrentScore(currentScore + scoreIncrement);
        string scoreIncrementString = scoreDisplayerUpdater.ApplyToCreditsFormating(scoreIncrement);
        scoreDisplayer.AddVolatileText($"+ {scoreIncrementString}", scoreDisplayer.GetTextColor());
        scoreDisplayerUpdater.UpdateValue();
    }

    public override void OnCatchData() {
        scoreIncrement += scoreIncrement2;
        string scoreIncrement2String = incrementDisplayerUpdater.ApplyToCreditsFormating(scoreIncrement2);
        incrementDisplayer.AddVolatileText($"+ {scoreIncrement2String} !", incrementDisplayer.GetTextColor());
        incrementDisplayerUpdater.UpdateValue();
    }

    public override void OnNewTresholdCrossed() {
        scoreIncrement2 += scoreIncrement3;
        string scoreIncrement3String = increment2DisplayerUpdater.ApplyToCreditsFormating(scoreIncrement3);
        increment2Displayer.AddVolatileText($"+ {scoreIncrement3String} !!!", incrementDisplayer.GetTextColor());
        increment2DisplayerUpdater.UpdateValue();

        scoreIncrement3 += scoreIncrement4;
        if(!hasAlreadyDoubleScoreIncrement4) {
            scoreIncrement4 *= 2;
            hasAlreadyDoubleScoreIncrement4 = true;
        }
    }
}
