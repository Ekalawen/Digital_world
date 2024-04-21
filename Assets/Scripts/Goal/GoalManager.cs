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
    public int playerIsInControlTreshold = 10;
    public int infiniteModeNbBlocksTreshold = 100;
    public List<GoalLevel> goalLevels;

    protected GameManager gm;

    public void Initialize() {
        gm = GameManager.Instance;
    }

    public GoalType GetGoalType() {
        return goalType;
    }

    public int GetMaxTreshold() {
        return goalLevels.Select(g => g.treshold).Max();
    }

    public int GetTotalCreditScore() {
        return PrefsManager.GetInt(StringHelper.GetKeyFor(PrefsManager.TOTAL_CREDITS_SCORE), 0);
    }

    public int GetBestCreditScore() {
        return PrefsManager.GetInt(StringHelper.GetKeyFor(PrefsManager.BEST_CREDITS_SCORE), 0);
    }

    public int GetCurrentTotalCreditScore() {
        return GetTotalCreditScore() + gm.GetInfiniteMap().scoreManager.GetCurrentScore();
    }

    public int GetTotalBlocksScore() {
        return PrefsManager.GetInt(StringHelper.GetKeyFor(PrefsManager.TOTAL_BLOCKS_SCORE), 0);
    }

    public int GetBestBlocksScore() {
        return (int)PrefsManager.GetFloat(StringHelper.GetKeyFor(PrefsManager.BEST_BLOCKS_SCORE), 0);
    }

    public int GetCurrentTotalBlocksScore() {
        return GetTotalBlocksScore() + gm.GetInfiniteMap().GetNonStartNbBlocksRun();
    }

    public string GetTresholdString() {
        return GetMaxTreshold().ToString();
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

    public GoalLevel GetGoalLevel(SelectorLevel endLevel) {
        //return goalLevels.Find(g => g.GetNextLevel() == endLevel.menuLevel);
        foreach(GoalLevel goalLevel in goalLevels) {
            if(goalLevel.GetNextMenuLevel().GetNameId() == endLevel.menuLevel.GetNameId()) {
                return goalLevel;
            }
        }
        return null;
    }
}
