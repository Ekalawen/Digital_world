using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScoreManager : MonoBehaviour
{
    public float dataProbability = 1 / 3.0f;
    public float scoreUpdateDuration = 0.6f;

    protected GameManager gm;
    protected InfiniteMap infiniteMap;
    protected CounterDisplayer displayer;
    protected int currentScore;
    protected int currentlyDisplayedScore;
    protected SingleCoroutine updateScoreCoroutine;

    public void Initialize() {
        gm = GameManager.Instance;
        infiniteMap = gm.GetInfiniteMap();
        displayer = infiniteMap.nbBlocksDisplayer;
        displayer.SetColor(gm.console.allyColor);
        updateScoreCoroutine = new SingleCoroutine(this);
        InitializeScore();
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

    protected void UpdateDisplayer() {
        updateScoreCoroutine.Start(CUpdateDisplayer());
    }

    private IEnumerator CUpdateDisplayer() {
        Timer timer = new Timer(scoreUpdateDuration);
        int startScore = currentlyDisplayedScore;
        startScore = Mathf.Min(startScore + 1, currentScore);
        while (!timer.IsOver()) {
            float avancement = MathCurves.QuadraticInverse(0, 1, timer.GetAvancement());
            int score = Mathf.CeilToInt(MathCurves.Linear(startScore, currentScore, avancement));
            DisplayScore(score);
            yield return null;
        }
        DisplayScore(currentScore);
    }

    private void DisplayScore(int score) {
        int nbBlocks = infiniteMap.GetNonStartNbBlocksRun();
        displayer.Display($"{score} {SpecificDisplayScore()}({nbBlocks})");
        currentlyDisplayedScore = score;
    }

    protected virtual string SpecificDisplayScore() {
        return "";
    }
}
