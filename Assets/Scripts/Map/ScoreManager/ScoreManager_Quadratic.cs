using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager_Quadratic : ScoreManager {

    [Tooltip("On Crossing Treshold")]
    public int scoreIncrement3 = 5;
    public float downOffset = 30.0f;

    protected int scoreIncrement4 = 0;
    protected int scoreMultiplier = 2;
    protected bool hasAlreadyDoubleScoreIncrement4 = false;

    protected override void InitializeScore() {
        InitializeScoreIncrement3();
        InitializeScoreIncrement4();
        SetCurrentScore(0);
        UpdateAllDisplayersInstantly();
    }

    protected void InitializeScoreIncrement3() {
        scoreIncrement3 += SkillTreeManager.Instance.IsEnabled(SkillKey.QUADRATIC_TRESHOLDS) ? 1 : 0;
    }

    protected void InitializeScoreIncrement4() {
        scoreIncrement4 = SkillTreeManager.Instance.IsEnabled(SkillKey.QUADRATIC_TRESHOLDS) ? 1 : 0;
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
        AddToScore(scoreIncrement);
    }

    public override void OnCatchData() {
        AddToScoreIncrement(scoreIncrement2);
    }

    private void AddToScoreIncrement(int scoreIncrementToAdd) {
        scoreIncrement += scoreIncrementToAdd;
        string scoreIncrementToAddString = incrementDisplayerUpdater.ApplyToCreditsFormating(scoreIncrementToAdd);
        incrementDisplayer.AddVolatileText($"+ {scoreIncrementToAddString} !", incrementDisplayer.GetTextColor());
        incrementDisplayerUpdater.UpdateValue();
    }

    public override void OnNewTresholdCrossed() {
        AddToScoreIncrement2(scoreIncrement3);
        AddToScoreIncrement3(scoreIncrement4);
        //if(!hasAlreadyDoubleScoreIncrement4) {
        //    scoreIncrement4 *= 2;
        //    hasAlreadyDoubleScoreIncrement4 = true;
        //}
    }

    protected void AddToScoreIncrement3(int scoreIncrementToAdd) {
        scoreIncrement3 += scoreIncrementToAdd;
    }

    protected void AddToScoreIncrement4(int scoreIncrementToAdd) {
        scoreIncrement4 += scoreIncrementToAdd;
    }

    private void AddToScoreIncrement2(int scoreIncrement2ToAdd) {
        scoreIncrement2 += scoreIncrement2ToAdd;
        string scoreIncrement2ToAddString = increment2DisplayerUpdater.ApplyToCreditsFormating(scoreIncrement2ToAdd);
        increment2Displayer.AddVolatileText($"+ {scoreIncrement2ToAddString} !!!", incrementDisplayer.GetTextColor());
        increment2DisplayerUpdater.UpdateValue();
    }

    public override void MultiplyAllScores(float multiplier) {
        base.MultiplyAllScores(multiplier);
        int scoreIncrementToAdd = Mathf.RoundToInt(scoreIncrement * (multiplier - 1));
        int scoreIncrement2ToAdd = Mathf.RoundToInt(scoreIncrement2 * (multiplier - 1));
        int scoreIncrement3ToAdd = Mathf.RoundToInt(scoreIncrement3 * (multiplier - 1));
        AddToScoreIncrement(scoreIncrementToAdd);
        AddToScoreIncrement2(scoreIncrement2ToAdd);
        AddToScoreIncrement3(scoreIncrement3ToAdd);
        AddToScoreIncrement4(scoreIncrement3ToAdd);
    }
}
