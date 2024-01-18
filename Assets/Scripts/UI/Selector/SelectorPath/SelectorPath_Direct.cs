using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SelectorPath_Direct : SelectorPath {

    public Material lineMaterial;

    [Header("Unlock")]
    public GoalManager startLevelGoalManager;

    public override void Initialize(SelectorPathUnlockScreen unlockScreen) {
        base.Initialize(unlockScreen);
        SetLineMaterial();
    }

    protected void SetLineMaterial() {
        float avancement = startLevel.GetTotalCreditsScore() / (float)startLevelGoalManager.treshold;
        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[] {
            new GradientColorKey(Color.white, 0),
            new GradientColorKey(Color.white, 1) },
            new GradientAlphaKey[] { });
        line.colorGradient = gradient;
        line.material = new Material(lineMaterial);
        line.material.SetFloat("Avancement", avancement);
        lineColorAvancement = avancement;
    }

    public override TYPE GetPathType() {
        return TYPE.DIRECT;
    }

    public override bool IsUnlocked() {
        return startLevel.HasCrossedCreditsTreshold(startLevelGoalManager.treshold);
    }
}
