using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GoalLevel {
    public MenuLevel menuLevel;
    //public GoalManager.GoalType type = GoalManager.GoalType.SCORE;
    public int treshold = 10_000;
}
