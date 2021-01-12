using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        levelNameText.text = levelName;
        UIHelper.FitTextHorizontaly(levelNameText.text, levelNameText);
    }

    protected string GetLevelName(SelectorPath path, DirectionType directionType) {
        if (directionType == DirectionType.FORWARD) {
            return path.endLevel.GetName();
        } else {
            return path.startLevel.GetName();
        }
    }
}
