using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoalTresholds", menuName = "GoalTresholds")]
public class GoalTresholds : ScriptableObject {

    public GoalManager.GoalType type;
    public List<int> tresholds;
}
