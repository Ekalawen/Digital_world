using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FastUISystem : MonoBehaviour {

    public enum DirectionType {
        FORWARD,
        BACKWARD,
    };

    public TMP_Text levelNameText;
    public Button levelButton;
    public Button unlockScreenButton;
    
    public void Initialize(SelectorPath path, DirectionType directionType) {
        string levelName = GetLevelName(path, directionType);
        SetName(levelName);
        SetTooltips(levelName, directionType);
    }

    protected void SetName(string levelName) {
        levelNameText.text = levelName;
        UIHelper.FitTextHorizontaly(levelNameText.text, levelNameText);
    }

    protected void SetTooltips(string levelName, DirectionType directionType) {
        string suivant = directionType == DirectionType.FORWARD ? "suivant" : "precedant";
        levelNameText.GetComponent<TooltipActivator>().message = $"Vers le niveau {suivant}";
        unlockScreenButton.GetComponent<TooltipActivator>().message = $"Déverouiller {levelName}";
        levelButton.GetComponent<TooltipActivator>().message = $"Niveau {levelName}";
    }

    protected string GetLevelName(SelectorPath path, DirectionType directionType) {
        if (directionType == DirectionType.FORWARD) {
            return path.endLevel.GetName();
        } else {
            return path.startLevel.GetName();
        }
    }
}
