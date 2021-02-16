using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoalTresholds", menuName = "GoalTresholds")]
public class GoalTresholds : ScriptableObject {

    public enum GoalType {
        DATA,
        BLOCK,
    }

    public GoalType type;
    public List<int> tresholds;
}
