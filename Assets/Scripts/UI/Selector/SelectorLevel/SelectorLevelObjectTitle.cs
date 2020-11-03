using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorLevelObjectTitle : MonoBehaviour {

    public SelectorLevelObject objectLevel;

    public Text text;
    public Image background;
    public Color normalColor;
    public Color focusedColor;

    public void Initialize() {
        SetTitleToLevelName();
    }

    protected void SetTitleToLevelName() {
        text.text = objectLevel.level.menuLevel.textLevelName.text;
    }

    public void SetFocused() {
        background.color = focusedColor;
    }

    public void SetUnfocused() {
        background.color = normalColor;
    }
}
