using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.Localization;

public class FastUISystem : MonoBehaviour {

    public enum DirectionType {
        FORWARD,
        BACKWARD,
    };

    public enum FromType {
        LEVEL,
        UNLOCK_SCREEN,
    };

    [Header("Links")]
    public TMP_Text levelNameText;
    public Button levelButtonOpenned;
    public Button levelButtonClosed;
    public Button unlockScreenButtonOpenned;
    public Button unlockScreenButtonClosed;

    [Header("Localized Strings")]
    public LocalizedString niveauPrecedantString;
    public LocalizedString niveauSuivantString;
    public LocalizedString dataHackeesString;
    public LocalizedString niveauString;

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
        string levelName = level.GetVisibleName();
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

        bool levelUnlocked = selectorManager.GetInPaths(level).All(p => p.IsUnlocked());
        levelButton = levelUnlocked ? levelButtonOpenned : levelButtonClosed;
        levelButtonOpenned.gameObject.SetActive(levelUnlocked);
        levelButtonClosed.gameObject.SetActive(!levelUnlocked);
    }

    protected void SetNameInRect(string levelName) {
        levelNameText.text = levelName;
        UIHelper.FitTextHorizontaly(levelNameText.text, levelNameText);
    }

    protected void SetTooltips(string levelName, DirectionType directionType) {
        if(directionType == DirectionType.FORWARD) {
            levelNameText.GetComponent<TooltipActivator>().localizedMessage = niveauSuivantString;
        } else {
            levelNameText.GetComponent<TooltipActivator>().localizedMessage = niveauPrecedantString;
        }
        if (unlockScreenButton != null) {
            unlockScreenButton.GetComponent<TooltipActivator>().localizedMessage = dataHackeesString;
        }
        levelButton.GetComponent<TooltipActivator>().localizedMessage = niveauString;
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
                selectorManager.PlaceCameraInFrontOfInterestTransform(level.objectLevel.transform);
            }
            selectorManager.TryDisplayLevel(level, instantDisplay: true);
        } else { // UnlockScreen
            if (selectorManager.IsLevelAccessible(level)) {
                path.CloseUnlockScreenForFastUI();
                selectorManager.PlaceCameraInFrontOfInterestTransform(level.objectLevel.transform);
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
                string key = path.GetNameId() + PrefsManager.IS_HIGHLIGHTED_PATH_KEY;
                pathHighlighter.enabled = PrefsManager.GetBool(key, false);
            } else {
                pathHighlighter.enabled = false;
            }
        }

        ButtonHighlighter levelHighlighter = levelButton.GetComponent<ButtonHighlighter>();
        levelHighlighter.enabled = level.IsHighlighted();
    }
}
