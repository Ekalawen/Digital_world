using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using System.Linq;

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
    public Button levelButtonOpenned;
    public Button levelButtonClosed;
    public Button unlockScreenButtonOpenned;
    public Button unlockScreenButtonClosed;

    protected SelectorManager selectorManager;
    protected SelectorPath path;
    protected SelectorLevel level;
    protected FromType fromType;
    protected Button unlockScreenButton;
    protected Button levelButton;
    
    public void Initialize(SelectorPath path, DirectionType directionType, FromType fromType)
    {
        selectorManager = SelectorManager.Instance;
        this.path = path;
        this.level = GetLevel(path, directionType);
        this.fromType = fromType;
        string levelName = level.GetName();
        ChoseGoodButtons(path);
        SetNameInRect(levelName);
        SetTooltips(levelName, directionType);
        SetButtonsActivations();
        SetHighlighterIfNeeded();
    }

    protected void ChoseGoodButtons(SelectorPath path) {
        if (fromType == FromType.LEVEL) {
            bool pathUnlocked = path.IsUnlocked();
            unlockScreenButton = pathUnlocked ? unlockScreenButtonOpenned : unlockScreenButtonClosed;
            unlockScreenButtonOpenned.gameObject.SetActive(pathUnlocked);
            unlockScreenButtonClosed.gameObject.SetActive(!pathUnlocked);
        } else {
            unlockScreenButtonOpenned.gameObject.SetActive(false);
            unlockScreenButtonClosed.gameObject.SetActive(false);
        }

        bool levelFinished = level.IsSucceeded();
        levelButton = levelFinished ? levelButtonOpenned : levelButtonClosed;
        levelButtonOpenned.gameObject.SetActive(levelFinished);
        levelButtonClosed.gameObject.SetActive(!levelFinished);
    }

    protected void SetNameInRect(string levelName) {
        levelNameText.text = levelName;
        UIHelper.FitTextHorizontaly(levelNameText.text, levelNameText);
    }

    protected void SetTooltips(string levelName, DirectionType directionType) {
        string suivant = directionType == DirectionType.FORWARD ? "suivant" : "precedant";
        levelNameText.GetComponent<TooltipActivator>().message = $"Vers le niveau {suivant}";
        if (unlockScreenButton != null) {
            unlockScreenButton.GetComponent<TooltipActivator>().message = $"Data Hackées()";
        }
        levelButton.GetComponent<TooltipActivator>().message = $"Niveau()";
    }

    protected void SetButtonsActivations() {
        UnityAction levelAction = new UnityAction(this.OnLevelButtonClick);
        levelButton.onClick.AddListener(levelAction);
        UnityAction unlockScreenAction = new UnityAction(this.OnUnlockScreenButtonClick);
        if (unlockScreenButton != null) {
            unlockScreenButton.onClick.AddListener(unlockScreenAction);
        }
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

    protected void SetHighlighterIfNeeded() {
        if (unlockScreenButton != null) {
            ButtonHighlighter pathHighlighter = unlockScreenButton.GetComponent<ButtonHighlighter>();
            if (fromType == FromType.LEVEL) {
                string key = path.GetName() + SelectorPath.IS_HIGHLIGHTED_PATH_KEY;
                bool state = PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == MenuManager.TRUE;
                pathHighlighter.enabled = state;
            } else {
                pathHighlighter.enabled = false;
            }
        }

        ButtonHighlighter levelHighlighter = levelButton.GetComponent<ButtonHighlighter>();
        levelHighlighter.enabled = level.IsHighlighted();
    }
}
