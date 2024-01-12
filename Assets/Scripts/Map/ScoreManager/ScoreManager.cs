using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ScoreManager : MonoBehaviour
{
    public float dataProbability = 1 / 3.0f;
    public int nbDataPerBlock = 1;
    [Tooltip("On Crossing Block")]
    public int scoreIncrement = 1;
    [Tooltip("On Catching Data")]
    public int scoreIncrement2 = 1;

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
        InitializeDisplayers();
        InitializeScore();
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
        int scoreGain = gm.goalManager.treshold;
        SetCurrentScore(currentScore + scoreGain);
        string scoreGainString = scoreDisplayerUpdater.ApplyToCreditsFormating(scoreGain);
        scoreDisplayer.AddVolatileText($"+ {scoreGainString}", scoreDisplayer.GetTextColor());
        scoreDisplayerUpdater.UpdateValue();
    }

    public virtual int GetNbDataForBlock() {
        return UnityEngine.Random.value < dataProbability ? nbDataPerBlock : 0;
    }

    public int GetCurrentScore() {
        return currentScore;
    }

    public void SetCurrentScore(int newValue) {
        if(newValue == currentScore) {
            return;
        }
        currentScore = newValue;
        onScoreChange.Invoke(currentScore);
    }

    protected void UpdateAllDisplayersInstantly() {
        scoreDisplayerUpdater.UpdateValueInstantly();
        incrementDisplayerUpdater.UpdateValueInstantly();
        increment2DisplayerUpdater.UpdateValueInstantly();
    }

    public void AddScoreToCreditCount() {
        SkillTreeManager.Instance.AddCredits(GetCurrentScore());
    }
}
