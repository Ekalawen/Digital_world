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

    public Button unlockButtonEnabled;
    public Button unlockButtonDisabled;
    public LevelProgressBar progressBar;

    protected GameManager gm;
    protected GoalLevel goalLevel;

    public void Initialize(GoalLevel goalLevel) {
        gm = GameManager.Instance;
        this.goalLevel = goalLevel;
        progressBar.Initialize(maxValue: goalLevel.treshold);
        progressBar.onReachMaxValueVisual.AddListener(SwapToEnabledUnlockButton);
    }

    public void Display() {
        DisplayProgressBar();
        DisplayUnlockButton();
    }

    public void SwapToEnabledUnlockButton() {
        unlockButtonEnabled.gameObject.SetActive(true);
        unlockButtonDisabled.gameObject.SetActive(false);
    }

    protected void DisplayUnlockButton() {
        unlockButtonEnabled.gameObject.SetActive(false);
        unlockButtonDisabled.gameObject.SetActive(true);
    }

    private void DisplayProgressBar() {
        int currentValue = gm.goalManager.GetTotalCreditScore();
        progressBar.SetCurrentValue(currentValue);
    }

    public void UnlockPath() {
        goalLevel.GetPath().UnlockPath();
    }

    public void QuitterPartie() {
        gm.QuitterPartie();
    }
}
