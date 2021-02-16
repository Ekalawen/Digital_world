using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour {

    public enum GoalType {
        DATA,
        BLOCK,
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
        Debug.Log($"allTresholds = {allTresholds}");
        return allTresholds;
    }

    public int GetNextTresholdFor(int dataCount) {
        return GetAllTresholds().Find(n => n > dataCount);
    }

    public string GetNextTresholdSymbolFor(int dataCount) {
        int nextTreshold = GetNextTresholdFor(dataCount);
        return nextTreshold == int.MaxValue ? "∞" : nextTreshold.ToString();
    }
}
