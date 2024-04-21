using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.SceneManagement;


public class EndLevelUnlockGroup : MonoBehaviour {

    public Button unlockButton;
    public LevelProgressBar progressBar;

    protected GameManager gm;

    public void Initialize(GoalLevel goalLevel) {
        gm = GameManager.Instance;
        progressBar.Initialize(maxValue: goalLevel.treshold);
    }

    public void StartProgressBar() {
        int currentValue = gm.goalManager.GetCurrentTotalCreditScore();
        progressBar.SetCurrentValue(currentValue);
    }
}
