using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorLevelObjectTitle : MonoBehaviour {

    public SelectorLevelObject objectLevel;

    public TMPro.TMP_Text text;
    public Image background;
    public Color unlockedColor;
    public Color lockedColor;
    public Color focusedColor;

    public void Initialize() {
        SetTitleToLevelName();
        SetUnfocused();
    }

    protected void SetTitleToLevelName() {
        text.text = objectLevel.level.GetVisibleName();
        UIHelper.FitTextHorizontaly(objectLevel.level.GetVisibleName(), text);
    }

    public void SetFocused() {
        background.color = focusedColor;
    }

    public void SetUnfocused() {
        SetNormalColor();
    }

    protected void SetNormalColor() {
        background.color = objectLevel.level.IsAccessible() ? unlockedColor : lockedColor;
    }
}
