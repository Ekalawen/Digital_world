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
    }

    public List<GoalTresholds> goalTresholds;

    protected GameManager gm;
    protected List<int> allTresholds;

    public void Initialize() {
        gm = GameManager.Instance;
        AssertOnTresholds();
    }

    protected void AssertOnTresholds() {
        if(goalTresholds.Count == 0) {
            Debug.LogWarning($"La liste de tresholds est vide !");
        } else {
            if(goalTresholds.Select(gt => gt.type).Distinct().ToList().Count > 1) {
                Debug.LogError($"La liste de tresholds contient plusieurs types !");
            }
        }
    }

    public GoalType GetGoalType() {
        return goalTresholds[0].type;
    }

    public List<int> GetAllTresholds() {
        if (allTresholds == null) {
            allTresholds = goalTresholds.SelectMany(gt => gt.tresholds).Distinct().OrderBy(n => n).ToList();
            allTresholds.Add(int.MaxValue);
        }
        return allTresholds;
    }

    public List<int> GetAllNotUnlockedTresholds() {
        List<int> tresholds = GetAllTresholds();
        float seuil = 0;
        switch (GetGoalType()) {
            case GoalType.DATA:
                seuil = Lumiere.GetCurrentDataCount();
                break;
            case GoalType.BLOCK:
                seuil = gm.eventManager.GetBestScore();
                break;
            case GoalType.VICTORY:
                seuil = gm.eventManager.GetNbWins();
                break;
        }
        return tresholds.FindAll(t => t > seuil).ToList();
    }

    public int GetNextTresholdFor(int dataCount) {
        return GetAllTresholds().Find(n => n > dataCount);
    }

    public int GetNextNotUnlockedTresholdFor(int dataCount) {
        return GetAllNotUnlockedTresholds().Find(n => n > dataCount);
    }

    public int GetFirstTreshold() {
        return GetAllTresholds().First();
    }

    public int GetLastTreshold() {
        return GetAllTresholds().Last();
    }

    public int GetLastTresholdNotInfinite() {
        return GetAllTresholds().FindAll(v => v < int.MaxValue).Last();
    }

    public string GetNextTresholdSymbolFor(int dataCount) {
        int nextTreshold = GetNextTresholdFor(dataCount);
        return nextTreshold == int.MaxValue ? "∞" : nextTreshold.ToString();
    }

    public string GetNextNotUnlockedTresholdSymbolFor(int dataCount) {
        int nextTreshold = GetNextNotUnlockedTresholdFor(dataCount);
        return nextTreshold == int.MaxValue ? "∞" : nextTreshold.ToString();
    }
}
