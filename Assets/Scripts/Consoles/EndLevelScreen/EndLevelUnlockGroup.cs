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

    public void Initialize() {
        progressBar.Initialize(maxValue: 1200);
    }

    public void StartProgressBar() {
        progressBar.SetCurrentValue(500);
    }
}
