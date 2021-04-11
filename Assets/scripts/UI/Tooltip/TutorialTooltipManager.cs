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

    public GameObject tutorialTooltipRedoPreviousLevel;
    public RectTransform tutorialTooltipRedoPreviousLevelTransform;

    protected SelectorManager selectorManager;
    protected List<GameObject> prefabsAlreadyInstantiated;
    protected List<Tuple<TutorialTooltip, int>> tutorialTooltipsPathWithIndice;

    public void Initialize(SelectorManager selectorManager) {
        this.selectorManager = selectorManager;
        prefabsAlreadyInstantiated = new List<GameObject>();
        tutorialTooltipsPathWithIndice = new List<Tuple<TutorialTooltip, int>>();
        selectorManager.onDisplayLevel.AddListener(DisplayLevelTutorialTooltips);
        selectorManager.onDisplayPath.AddListener(DisplayPathTutorialTooltips);
        selectorManager.onOpenDHPath.AddListener(DisplayPathOnOpenDHTutorialTooltips);
        selectorManager.onUnlockPath.AddListener(DisplayPathOnUnlockTutorialTooltips);
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
            if (level.IsSucceeded() && !PrefsManager.GetBool(GetKey(tutorialTooltipToDH), false)) {
                InstantiateTutorialTooltip(tutorialTooltipToDH, tutorialTooltipToDHTransform, parent: level.menuLevel.transform);
            }
        }
    }

    protected void DisplayPathTutorialTooltips(SelectorPath path) {
        int pathIndice = selectorManager.GetPathIndice(path);
        if (pathIndice == 0) {
            if (!PrefsManager.GetBool(GetKey(tutorialTooltipOpenDH), false)) {
                TutorialTooltip tutorialTooltip = InstantiateTutorialTooltip(tutorialTooltipOpenDH, tutorialTooltipOpenDHTransform, parent: selectorManager.unlockScreen.transform);
                if(tutorialTooltip != null) {
                    tutorialTooltipsPathWithIndice.Add(new Tuple<TutorialTooltip, int>(tutorialTooltip, 0));
                }
            }
        }
        foreach(Tuple<TutorialTooltip, int> pair in tutorialTooltipsPathWithIndice) {
            TutorialTooltip tutorialTooltip = pair.Item1;
            int tooltipPathIndice = pair.Item2;
            tutorialTooltip.gameObject.SetActive(pathIndice == tooltipPathIndice);
        }
    }

    protected void DisplayPathOnOpenDHTutorialTooltips(SelectorPath path) {
        if (selectorManager.GetPathIndice(path) == 0) {
            if (!PrefsManager.GetBool(GetKey(tutorialTooltipHacker), false)) {
                TutorialTooltip tutorialTooltip = InstantiateTutorialTooltip(tutorialTooltipHacker, tutorialTooltipHackerTransform, parent: selectorManager.unlockScreen.transform);
                if(tutorialTooltip != null) {
                    tutorialTooltipsPathWithIndice.Add(new Tuple<TutorialTooltip, int>(tutorialTooltip, 0));
                }
            }
        }
        if (selectorManager.GetPathIndice(path) == 1) {
            if (!PrefsManager.GetBool(GetKey(tutorialTooltipRedoPreviousLevel), false)) {
                TutorialTooltip tutorialTooltip = InstantiateTutorialTooltip(tutorialTooltipRedoPreviousLevel, tutorialTooltipRedoPreviousLevelTransform, parent: selectorManager.unlockScreen.transform);
                if(tutorialTooltip != null) {
                    tutorialTooltipsPathWithIndice.Add(new Tuple<TutorialTooltip, int>(tutorialTooltip, 1));
                }
            }
        }
    }

    protected void DisplayPathOnUnlockTutorialTooltips(SelectorPath path) {
        if (selectorManager.GetPathIndice(path) == 0) {
            if (path.IsUnlocked() && !PrefsManager.GetBool(GetKey(tutorialTooltipToNextLevel), false)) {
                TutorialTooltip tutorialTooltip = InstantiateTutorialTooltip(tutorialTooltipToNextLevel, tutorialTooltipToNextLevelTransform, parent: selectorManager.unlockScreen.transform);
                if(tutorialTooltip != null) {
                    tutorialTooltipsPathWithIndice.Add(new Tuple<TutorialTooltip, int>(tutorialTooltip, 0));
                }
            }
        }
    }

    protected TutorialTooltip InstantiateTutorialTooltip(GameObject tutorialTooltipPrefab, Transform transform, Transform parent = null) {
        if (prefabsAlreadyInstantiated.Contains(tutorialTooltipPrefab))
            return null;
        prefabsAlreadyInstantiated.Add(tutorialTooltipPrefab);
        Vector3 pos = transform.transform.position;
        TutorialTooltip tutorialTooltip = Instantiate(tutorialTooltipPrefab, pos, Quaternion.identity, transform).GetComponent<TutorialTooltip>();
        if (parent != null) {
            transform.parent = parent;
        }
        tutorialTooltip.Initialize(this);
        return tutorialTooltip;
    }

    public void NotifyTutorialTooltipPressed(string keySuffix) {
        PrefsManager.SetBool(GetKey(keySuffix), true);
        if (keySuffix == GetKeySuffix(tutorialTooltipSelectorMouvement)
         || keySuffix == GetKeySuffix(tutorialTooltipSelectorLevel)) {
            DisplaySelectorTutorialTooltips();
        }
        if(keySuffix == GetKeySuffix(tutorialTooltipOpenDH)) {
            DisplayPathOnOpenDHTutorialTooltips(selectorManager.GetPaths()[0]);
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
