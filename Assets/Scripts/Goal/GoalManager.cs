using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour {

    public enum GoalType {
        DATA,
        BLOCK,
        VICTORY,
        SCORE,
    }

    public GoalType goalType = GoalType.SCORE;
    public int treshold = 200_000;
    public int playerIsInControlTreshold = 200;
    [ConditionalHide("goalType", GoalType.SCORE)]
    public int infiniteModeNbBlocksTreshold = 100;

    protected GameManager gm;

    public void Initialize() {
        gm = GameManager.Instance;
    }

    public GoalType GetGoalType() {
        return goalType;
    }

    public int GetTreshold() {
        return treshold;
    }

    public int GetTotalScore() {
        return PrefsManager.GetInt(StringHelper.GetKeyFor(PrefsManager.TOTAL_SCORE), 0);
    }

    public int GetBestScore() {
        return PrefsManager.GetInt(StringHelper.GetKeyFor(PrefsManager.BEST_SCORE), 0);
    }

    public bool IsUnlocked() {
        return GetTotalScore() >= GetTreshold();
    }

    public string GetTresholdString() {
        return GetTreshold().ToString();
    }

    public bool IsPlayerInControl() {
        return GetBestScore() >= playerIsInControlTreshold;
    }

    public int GetInfiniteModeNbBlocksTreshold() {
        return infiniteModeNbBlocksTreshold;
    }

    //public string GetNextTresholdSymbolFor(int dataCount) {
    //    int nextTreshold = GetNextTresholdFor(dataCount);
    //    return nextTreshold == int.MaxValue ? "∞" : nextTreshold.ToString();
    //}
}
