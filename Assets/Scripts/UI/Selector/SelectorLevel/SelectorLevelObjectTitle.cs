using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorLevelObjectTitle : MonoBehaviour {

    public SelectorLevelObject objectLevel;

    public TMPro.TMP_Text text;
    public Image background;
    public Material materialUnlocked;
    public Material materialLocked;
    public Material materialFocused;

    public void Initialize() {
        SetTitleToLevelName();
        SetUnfocused();
    }

    protected void SetTitleToLevelName() {
        text.text = objectLevel.level.GetVisibleName();
        UIHelper.FitTextHorizontaly(objectLevel.level.GetVisibleName(), text);
    }

    public void SetFocused() {
        background.material = materialFocused;
        background.GetComponent<UpdateUnscaledTime>().Start();
    }

    public void SetUnfocused() {
        SetNormalColor();
    }

    protected void SetNormalColor() {
        background.material = objectLevel.level.IsAccessible() ? materialUnlocked : materialLocked;
        background.GetComponent<UpdateUnscaledTime>().Start();
    }
}
