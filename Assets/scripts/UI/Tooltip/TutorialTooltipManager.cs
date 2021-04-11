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
    protected List<GameObject> prefabsAlreadyInstantiated;

    public void Initialize(SelectorManager selectorManager) {
        this.selectorManager = selectorManager;
        prefabsAlreadyInstantiated = new List<GameObject>();
        selectorManager.onDisplayLevel.AddListener(DisplayLevelTutorialTooltips);
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

    protected void DisplayLevelTutorialTooltips(SelectorLevel level) {
        if (selectorManager.GetLevelIndice(level) == 0) {
            if (level.IsSucceeded()/* && !PrefsManager.GetBool(GetKey(tutorialTooltipToDH), false)*/) {
                InstantiateTutorialTooltip(tutorialTooltipToDH, tutorialTooltipToDHTransform, parent: level.menuLevel.transform);
            }
        }
    }

    protected void InstantiateTutorialTooltip(GameObject tutorialTooltipPrefab, Transform transform, Transform parent = null) {
        if (prefabsAlreadyInstantiated.Contains(tutorialTooltipPrefab))
            return;
        prefabsAlreadyInstantiated.Add(tutorialTooltipPrefab);
        Vector3 pos = transform.transform.position;
        TutorialTooltip tutorialTooltip = Instantiate(tutorialTooltipPrefab, pos, Quaternion.identity, transform).GetComponent<TutorialTooltip>();
        if (parent != null) {
            transform.parent = parent;
        }
        tutorialTooltip.Initialize(this);
    }

    public void NotifyTutorialTooltipPressed(string keySuffix) {
        PrefsManager.SetBool(GetKey(keySuffix), true);
        if (keySuffix == GetKeySuffix(tutorialTooltipSelectorMouvement)
         || keySuffix == GetKeySuffix(tutorialTooltipSelectorLevel)) {
            DisplaySelectorTutorialTooltips();
        }
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
