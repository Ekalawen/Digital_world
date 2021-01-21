using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLevelRunIntroduction : MonoBehaviour {

    public SelectorLevel selectorLevel;
    public TextAsset introductionTextAsset;

    public void RunIntroduction(bool forcePrint = false) {
        SelectorManager selectorManager = SelectorManager.Instance;
        if (!forcePrint && (!selectorManager.HasThisSelectorLevelOpen(selectorLevel) || selectorManager.PopupIsEnabled()))
            return;
        selectorManager.popup.Initialize(
            title: "The Netrunner Awaken1ng",
            useTextAsset: true,
            textAsset: introductionTextAsset,
            theme: TexteExplicatif.Theme.NEUTRAL);
        selectorManager.popup.Run();
    }
}
