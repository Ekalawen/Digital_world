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
        ReplaceSpecificColorFromThemes(ColorManager.Theme.MULTICOLOR, ColorManager.GetRandomNonMulticolorTheme());
        ReplaceSpecificColorFromThemes(ColorManager.Theme.MULTICOLOR_NON_BRIGHT, ColorManager.GetRandomNonBrightNonMulticolorTheme());
        RemoveSpecificColorFromThemes(ColorManager.Theme.ROUGE);
        trail.startColor = ColorManager.GetColor(themes);
    }

    protected void ReplaceSpecificColorFromThemes(ColorManager.Theme source, ColorManager.Theme cible) {
        if(source == cible) {
            Debug.LogError($"La source ({source}) ne doit pas être égale à la cible ({cible}) dans ReplaceSpecificColorFromThemes ;)");
            return;
        }
        while (themes.Contains(source)) {
            themes.Remove(source);
            themes.Add(cible);
        }
    }

    protected void RemoveSpecificColorFromThemes(ColorManager.Theme theme) {
        while (themes.Contains(theme)) {
            themes.Remove(theme);
            if (themes.Count <= 0) {
                themes.Add(ColorManager.GetRandomNonBrightTheme());
            }
        }
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
