using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class GoalManager : MonoBehaviour {

    public enum GoalType {
        DATA,
        BLOCK,
        VICTORY,
        SCORE,
    }

    public GoalType goalType = GoalType.SCORE;
    public int treshold = 200_000;
    public int playerIsInControlTreshold = 10;
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

    public int GetTotalCreditScore() {
        return PrefsManager.GetInt(StringHelper.GetKeyFor(PrefsManager.TOTAL_CREDITS_SCORE), 0);
    }

    public int GetBestCreditScore() {
        return PrefsManager.GetInt(StringHelper.GetKeyFor(PrefsManager.BEST_CREDITS_SCORE), 0);
    }

    public int GetTotalBlocksScore() {
        return PrefsManager.GetInt(StringHelper.GetKeyFor(PrefsManager.TOTAL_BLOCKS_SCORE), 0);
    }

    public int GetBestBlocksScore() {
        return PrefsManager.GetInt(StringHelper.GetKeyFor(PrefsManager.BEST_BLOCKS_SCORE), 0);
    }

    public bool IsUnlocked() {
        return GetTotalCreditScore() >= GetTreshold();
    }

    public string GetTresholdString() {
        return GetTreshold().ToString();
    }

    public bool IsPlayerInControl() {
        return GetBestBlocksScore() >= playerIsInControlTreshold;
    }

    public int GetInfiniteModeNbBlocksTreshold() {
        return infiniteModeNbBlocksTreshold;
    }

    public bool IsInfiniteModeUnlocked() {
        return GetBestBlocksScore() >= GetInfiniteModeNbBlocksTreshold();
    }

    //public string GetNextTresholdSymbolFor(int dataCount) {
    //    int nextTreshold = GetNextTresholdFor(dataCount);
    //    return nextTreshold == int.MaxValue ? "∞" : nextTreshold.ToString();
    //}
}
