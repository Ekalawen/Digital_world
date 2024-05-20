using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public abstract class ScoreManager : MonoBehaviour
{
    [Tooltip("On Crossing Block")]
    public int scoreIncrement = 1;
    [Tooltip("On Catching Data")]
    public int scoreIncrement2 = 1;

    protected List<float> dataProbabilities; // For each probability, their is a chance to spawn 1 more Data !
    protected GameManager gm;
    protected InfiniteMap infiniteMap;
    protected CounterDisplayer scoreDisplayer;
    protected CounterDisplayer incrementDisplayer;
    protected CounterDisplayer increment2Displayer;
    protected CounterDisplayerUpdater scoreDisplayerUpdater;
    protected CounterDisplayerUpdater incrementDisplayerUpdater;
    protected CounterDisplayerUpdater increment2DisplayerUpdater;
    protected int currentScore;
    [HideInInspector]
    public UnityEvent<long> onScoreChange;

    public void Initialize() {
        gm = GameManager.Instance;
        infiniteMap = gm.GetInfiniteMap();
        InitializeDataProbability();
        InitializeDisplayers();
        InitializeScore();
    }

    protected void InitializeDataProbability() {
        //dataProbabilities = new List<float>() { 1.0f }; // Always one data
        //return;
        dataProbabilities = new List<float>();
        if(SkillTreeManager.Instance.IsEnabled(SkillKey.DATA_BREACH)) {
            dataProbabilities.Add(1.0f / 3.0f);
        }
        if (SkillTreeManager.Instance.IsEnabled(SkillKey.DATA_EXPLOIT)) {
            dataProbabilities.Add(1.0f / 2.0f);
        }
    }

    private void InitializeDisplayers() {
        scoreDisplayer = infiniteMap.scoreDisplayer;
        incrementDisplayer = infiniteMap.incrementDisplayer;
        increment2Displayer = infiniteMap.increment2Displayer;
        Color color = gm.console.allyColor;
        scoreDisplayer.SetColor(color);
        incrementDisplayer.SetColor(color);
        increment2Displayer.SetColor(color);
        scoreDisplayerUpdater = scoreDisplayer.gameObject.GetComponent<CounterDisplayerUpdater>();
        incrementDisplayerUpdater = incrementDisplayer.gameObject.GetComponent<CounterDisplayerUpdater>();
        increment2DisplayerUpdater = increment2Displayer.gameObject.GetComponent<CounterDisplayerUpdater>();
        scoreDisplayerUpdater.Initialize(scoreDisplayer, GetCurrentScore);
        incrementDisplayerUpdater.Initialize(incrementDisplayer, GetCurrentIncrement);
        increment2DisplayerUpdater.Initialize(increment2Displayer, GetCurrentIncrement2);
        if (!SkillTreeManager.Instance.IsEnabled(SkillKey.DATA_BREACH)) {
            incrementDisplayer.gameObject.SetActive(false);
            infiniteMap.incrementDisplayerLockedText.gameObject.SetActive(true);
        }
        if (!SkillTreeManager.Instance.IsEnabled(SkillKey.UNLOCK_TRESHOLDS)) {
            increment2Displayer.gameObject.SetActive(false);
            infiniteMap.increment2DisplayerLockedText.gameObject.SetActive(true);
        }
    }

    protected int GetCurrentIncrement() {
        return scoreIncrement;
    }

    protected int GetCurrentIncrement2() {
        return scoreIncrement2;
    }

    protected abstract void InitializeScore();

    public abstract void SetMultiplier(int multiplier);

    // Score Increment
    public abstract void OnNewBlockCrossed();

    // Score Increment 2
    public abstract void OnCatchData();

    // Score Increment 3 and 4
    public abstract void OnNewTresholdCrossed();

    public virtual void OnWinGame() {
        if(!SkillTreeManager.Instance.IsEnabled(SkillKey.EPIPHANIC_EXPLOIT)) {
            return;
        }
        AddToScore(gm.goalManager.GetMaxTreshold());
    }

    public void AddToScore(int value) {
        SetCurrentScore(currentScore + value);
        string scoreGainString = scoreDisplayerUpdater.ApplyToCreditsFormating(value);
        string sign = value >= 0 ? "+" : "-";
        scoreDisplayer.AddVolatileText($"{sign} {scoreGainString}", scoreDisplayer.GetTextColor());
        scoreDisplayerUpdater.UpdateValue();
    }

    public virtual int GetNbDataForBlock() {
        return dataProbabilities.Select(p => UnityEngine.Random.value < p ? 1 : 0).Sum();
    }

    public void SetDataProbabilities(List<float> dataProbabilities) {
        this.dataProbabilities = dataProbabilities;
    }

    public int GetCurrentScore() {
        return currentScore;
    }

    public void SetCurrentScore(int newValue) {
        if(newValue == currentScore) {
            return;
        }
        currentScore = Math.Max(0, newValue);
        onScoreChange.Invoke(currentScore);
    }

    protected void UpdateAllDisplayersInstantly() {
        scoreDisplayerUpdater.UpdateValueInstantly();
        incrementDisplayerUpdater.UpdateValueInstantly();
        increment2DisplayerUpdater.UpdateValueInstantly();
    }

    public void AddScoreToCreditCount() {
        int scoreToAdd = GetCurrentScore() - gm.console.GetPauseMenu().skillTreeMenu.GetAddedCreditsThiGame();
        SkillTreeManager.Instance.AddCredits(scoreToAdd);
    }

    public virtual void MultiplyAllScores(float multiplier) {
        int scoreToAdd = Mathf.RoundToInt(GetCurrentScore() * (multiplier - 1.0f));
        AddToScore(scoreToAdd);
    }
}
