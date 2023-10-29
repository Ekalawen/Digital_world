using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScoreManager : MonoBehaviour
{
    public float dataProbability = 1 / 3.0f;

    protected GameManager gm;
    protected InfiniteMap infiniteMap;
    protected CounterDisplayer scoreDisplayer;
    protected CounterDisplayer incrementDisplayer;
    protected CounterDisplayer increment2Displayer;
    protected CounterDisplayerUpdater scoreDisplayerUpdater;
    protected CounterDisplayerUpdater incrementDisplayerUpdater;
    protected CounterDisplayerUpdater increment2DisplayerUpdater;
    protected int currentScore;
    protected int scoreIncrement = 1;
    protected int scoreIncrement2 = 5;

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
    }

    protected int GetCurrentIncrement() {
        return scoreIncrement;
    }

    protected int GetCurrentIncrement2() {
        return scoreIncrement2;
    }

    protected abstract void InitializeScore();

    public abstract void SetMultiplier(int multiplier);

    public abstract void OnNewBlockCrossed();

    public abstract void OnCatchData();

    public abstract void OnNewTresholdCrossed();

    public virtual int GetNbDataForBlock() {
        return UnityEngine.Random.value < dataProbability ? 1 : 0;
    }

    public int GetCurrentScore() {
        return currentScore;
    }

    protected void UpdateAllDisplayersInstantly() {
        scoreDisplayerUpdater.UpdateValueInstantly();
        incrementDisplayerUpdater.UpdateValueInstantly();
        increment2DisplayerUpdater.UpdateValueInstantly();
    }
}
