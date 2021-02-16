using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour {

    public List<GoalTresholds> goalTresholds;

    protected GameManager gm;

    public void Initialize() {
        gm = GameManager.Instance;
    }
}
