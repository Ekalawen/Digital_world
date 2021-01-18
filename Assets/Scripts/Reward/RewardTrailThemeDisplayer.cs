using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardTrailThemeDisplayer : RewardTrailDisplayer {

    protected List<ColorManager.Theme> themes;

    public void Initialize(GameObject prefab, ObjectHistory history, float duration, float delay, float acceleration, List<ColorManager.Theme> themes) {
        this.themes = themes;
        base.Initialize(prefab, history, duration, delay, acceleration, 1.0f);
    }

    public override void ResetObject() {
        base.ResetObject();
        trail.startColor = ColorManager.GetColor(themes);
    }

    public override void Update() {
        base.Update();
        if(trail != null)
            trail.time = durationTimer.GetRemainingTime();
    }

    public float GetAcceleration() {
        return acceleration;
    }
}
