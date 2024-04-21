using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GoalLevel {
    public GameObject selectorPathPrefab;
    public GameObject nextMenuLevelPrefab; // Because it isn't linked inside pathPrefab !
    //public GoalManager.GoalType type = GoalManager.GoalType.SCORE;
    public int treshold = 10_000;

    public bool IsSet() {
        return selectorPathPrefab && nextMenuLevelPrefab;
    }

    public SelectorPath GetPath() {
        return selectorPathPrefab?.GetComponent<SelectorPath>();
    }

    public MenuLevel GetNextMenuLevel() {
        return nextMenuLevelPrefab?.GetComponent<MenuLevel>(); }
}
