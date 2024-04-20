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

    public void Initialize() {
        gm = GameManager.Instance;
        progressBar.Initialize(maxValue: gm.goalManager.GetTreshold());
    }

    public void StartProgressBar() {
        int currentValue = SkillTreeManager.Instance.GetCredits();
        progressBar.SetCurrentValue(currentValue);
    }
}
