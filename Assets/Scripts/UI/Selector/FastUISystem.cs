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

    public enum FromType {
        LEVEL,
        UNLOCK_SCREEN,
    };

    public TMP_Text levelNameText;
    public Button levelButton;
    public Button unlockScreenButton;

    protected SelectorManager selectorManager;
    protected SelectorPath path;
    protected SelectorLevel level;
    protected FromType fromType;
    
    public void Initialize(SelectorPath path, DirectionType directionType, FromType fromType) {
        selectorManager = SelectorManager.Instance;
        this.path = path;
        this.level = GetLevel(path, directionType);
        this.fromType = fromType;
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
        if (fromType == FromType.LEVEL) {
            if (selectorManager.IsLevelAccessible(level)) {
                selectorManager.BackToSelectorForFastUI();
                selectorManager.PlaceCameraInFrontOfInterestPoint(level.objectLevel.transform.position);
            }
            selectorManager.TryDisplayLevel(level, instantDisplay: true);
        } else { // UnlockScreen
            if (selectorManager.IsLevelAccessible(level)) {
                path.CloseUnlockScreenForFastUI();
                selectorManager.PlaceCameraInFrontOfInterestPoint(level.objectLevel.transform.position);
            }
            selectorManager.TryDisplayLevel(level, instantDisplay: true);
        }
    }

    public void OnUnlockScreenButtonClick() {
        selectorManager.BackToSelectorForFastUI();
        path.OpenUnlockScreen(instantDisplay: true);
        selectorManager.PlaceCameraInFrontOfInterestPoint(path.cadena.transform.position);
    }

    protected SelectorLevel GetLevel(SelectorPath path, DirectionType directionType) {
        if (directionType == DirectionType.FORWARD) {
            return path.endLevel;
        } else {
            return path.startLevel;
        }
    }
}
