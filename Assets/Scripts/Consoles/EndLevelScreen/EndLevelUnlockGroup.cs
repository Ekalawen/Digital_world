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
using UnityEngine.Localization.Components;
using Coffee.UIExtensions;

[Serializable]
public class AttractedParticle
{
    public int creditValue = 10;
    public GameObject particleSystemPrefab;
    public UIParticleAttractor attractor;
}

public class EndLevelUnlockGroup : MonoBehaviour {

    public Button unlockButtonEnabled;
    public Button unlockButtonDisabled;
    public UIParticle unlockButtonParticles;
    public float durationUnlockButtonParticles = 2.5f;
    public LevelProgressBar progressBar;
    public List<AttractedParticle> attractedParticles;

    protected GameManager gm;
    protected GoalLevel goalLevel;

    public void Initialize(GoalLevel goalLevel) {
        gm = GameManager.Instance;
        this.goalLevel = goalLevel;
        unlockButtonParticles.gameObject.SetActive(false);
        progressBar.Initialize(maxValue: goalLevel.treshold);
        progressBar.onReachMaxValueVisual.AddListener(SwapToEnabledUnlockButton);
        InitializeAttractedParticles();
    }

    protected void InitializeAttractedParticles() {
        Transform scoreCounterTransform = gm.GetInfiniteMap().scoreDisplayer.displayText.transform;
        foreach (AttractedParticle attractedParticle in attractedParticles) {
            ParticleSystem particleSystem = Instantiate(attractedParticle.particleSystemPrefab, parent: scoreCounterTransform).GetComponentInChildren<ParticleSystem>();
            attractedParticle.attractor.gameObject.SetActive(false);
            attractedParticle.attractor.particleSystem = particleSystem;
            attractedParticle.attractor.gameObject.SetActive(true);
            for (int i = 0; i < 10; i++) {
                particleSystem.Play();
            }
        }
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
        string levelName = goalLevel.IsSet() ? goalLevel.GetNextMenuLevel().GetVisibleName() : "";
        unlockButtonEnabled.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { levelName };
        unlockButtonDisabled.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { levelName };
    }

    private void DisplayProgressBar() {
        int currentValue = gm.goalManager.GetTotalCreditScore();
        progressBar.SetCurrentValue(currentValue);
    }

    public void UnlockNextLevelButton() {
        UnlockPath();
        PlayUnlockButtonParticles();
        //ReturnToSelectorIn(durationUnlockButtonParticles);
    }

    protected void PlayUnlockButtonParticles() {
        unlockButtonParticles.gameObject.SetActive(true);
        unlockButtonParticles.Play();
    }

    protected void ReturnToSelectorIn(float delay) {
        StartCoroutine(CReturnToSelectorIn(delay));
    }

    protected IEnumerator CReturnToSelectorIn(float delay) {
        yield return new WaitForSecondsRealtime(delay);
        gm.QuitterPartie();
    }

    protected void UnlockPath() {
        if (goalLevel.IsSet()) {
            goalLevel.GetPath().UnlockPath();
        }
    }
}
