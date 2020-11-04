using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLevelRunIntroduction : MonoBehaviour {

    public SelectorLevel selectorLevel;
    public TextAsset introductionTextAsset;

    public void RunIntroduction() {
        SelectorManager selectorManager = SelectorManager.Instance;
        if (!selectorManager.HasThisSelectorLevelOpen(selectorLevel) || selectorManager.PopupIsEnabled())
            return;
        selectorManager.popup.Initialize(
            title: "Bienvenue dans Digital World !",
            useTextAsset: true,
            textAsset: introductionTextAsset,
            theme: TexteExplicatif.Theme.NEUTRAL);
        selectorManager.popup.Run();
    }
}
