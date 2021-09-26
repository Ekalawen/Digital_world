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
        ResetColor();
    }

    protected void ResetColor() {
        themes = ColorManager.ReplaceSpecificColorFromThemes(themes, ColorManager.Theme.MULTICOLOR, ColorManager.GetRandomNonMulticolorTheme());
        themes = ColorManager.ReplaceSpecificColorFromThemes(themes, ColorManager.Theme.MULTICOLOR_NON_BRIGHT, ColorManager.GetRandomNonBrightNonMulticolorTheme());
        themes = ColorManager.RemoveSpecificColorFromThemes(themes, ColorManager.Theme.ROUGE);
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
