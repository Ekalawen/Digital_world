using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class FastUISystem : MonoBehaviour {

    public enum DirectionType {
        FORWARD,
        BACKWARD,
    };

    public TMP_Text levelNameText;
    public Button levelButton;
    public Button unlockScreenButton;

    protected SelectorManager selectorManager;
    protected SelectorPath path;
    protected SelectorLevel level;
    
    public void Initialize(SelectorPath path, DirectionType directionType) {
        selectorManager = SelectorManager.Instance;
        this.path = path;
        this.level = GetLevel(path, directionType);
        string levelName = level.GetName();
        SetName(levelName);
        SetTooltips(levelName, directionType);
        SetButtonsActivations();
    }

    protected void SetName(string levelName) {
        levelNameText.text = levelName;
        UIHelper.FitTextHorizontaly(levelNameText.text, levelNameText);
    }

    protected void SetTooltips(string levelName, DirectionType directionType) {
        string suivant = directionType == DirectionType.FORWARD ? "suivant" : "precedant";
        levelNameText.GetComponent<TooltipActivator>().message = $"Vers le niveau {suivant}";
        unlockScreenButton.GetComponent<TooltipActivator>().message = $"Data Hackées()";
        levelButton.GetComponent<TooltipActivator>().message = $"Niveau()";
    }

    protected void SetButtonsActivations() {
        UnityAction levelAction = new UnityAction(this.OnLevelButtonClick);
        levelButton.onClick.AddListener(levelAction);
        UnityAction unlockScreenAction = new UnityAction(this.OnUnlockScreenButtonClick);
        unlockScreenButton.onClick.AddListener(unlockScreenAction);
    }

    public void OnLevelButtonClick() {
        selectorManager.BackToSelectorForFastUI();
        selectorManager.TryDisplayLevel(level, instantDisplay: true);
    }

    public void OnUnlockScreenButtonClick() {
        selectorManager.BackToSelectorForFastUI();
        path.OpenUnlockScreen(instantDisplay: true);
    }

    protected SelectorLevel GetLevel(SelectorPath path, DirectionType directionType) {
        if (directionType == DirectionType.FORWARD) {
            return path.endLevel;
        } else {
            return path.startLevel;
        }
    }
}
