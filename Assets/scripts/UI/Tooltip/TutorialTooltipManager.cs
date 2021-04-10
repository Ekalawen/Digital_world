using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class TutorialTooltipManager : MonoBehaviour {

    [Header("TutorialTooltips")]
    public GameObject tutorialTooltipSelectorMouvement;
    public RectTransform tutorialTooltipSelectorMouvementTransform;

    public GameObject tutorialTooltipSelectorLevel;
    public RectTransform tutorialTooltipSelectorLevelTransform;

    public GameObject tutorialTooltipSelectorCadenas;
    public RectTransform tutorialTooltipSelectorCadenasTransform;

    public GameObject tutorialTooltipToDH;
    public RectTransform tutorialTooltipToDHTransform;

    public GameObject tutorialTooltipOpenDH;
    public RectTransform tutorialTooltipOpenDHTransform;

    public GameObject tutorialTooltipHacker;
    public RectTransform tutorialTooltipHackerTransform;

    public GameObject tutorialTooltipToNextLevel;
    public RectTransform tutorialTooltipToNextLevelTransform;

    protected SelectorManager selectorManager;

    public void Initialize(SelectorManager selectorManager) {
        this.selectorManager = selectorManager;
        DisplaySelectorTutorialTooltips();
    }

    protected void DisplaySelectorTutorialTooltips() {
        if(!PrefsManager.GetBool(GetKey(tutorialTooltipSelectorMouvement), false)) {
            InstantiateTutorialTooltip(tutorialTooltipSelectorMouvement, tutorialTooltipSelectorMouvementTransform);
        } else if (!PrefsManager.GetBool(GetKey(tutorialTooltipSelectorLevel), false)) {
            InstantiateTutorialTooltip(tutorialTooltipSelectorLevel, tutorialTooltipSelectorLevelTransform);
        } else if (!PrefsManager.GetBool(GetKey(tutorialTooltipSelectorCadenas), false)) {
            InstantiateTutorialTooltip(tutorialTooltipSelectorCadenas, tutorialTooltipSelectorCadenasTransform);
        }
    }

    protected void InstantiateTutorialTooltip(GameObject gameObject, Transform transform) {
        Vector3 pos = transform.transform.position;
        TutorialTooltip tutorialTooltip = Instantiate(gameObject, pos, Quaternion.identity, transform).GetComponent<TutorialTooltip>();
        tutorialTooltip.Initialize(this);
    }

    public void NotifyTutorialTooltipPressed(string keySuffix) {
        PrefsManager.SetBool(GetKey(keySuffix), true);
        DisplaySelectorTutorialTooltips();
    }

    public string GetKey(string keySuffix) {
        return PrefsManager.HAS_DISPLAY_TUTORIAL_TOOLTIP_PREFIX + keySuffix;
    }

    public string GetKey(GameObject tutorialTooltipObject) {
        return GetKey(GetKeySuffix(tutorialTooltipObject));
    }

    public string GetKeySuffix(GameObject tutorialTooltipObject) {
        return tutorialTooltipObject.GetComponent<TutorialTooltip>().keySuffix;
    }
}
