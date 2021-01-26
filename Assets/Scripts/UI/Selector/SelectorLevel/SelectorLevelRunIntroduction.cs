using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SelectorLevelRunIntroduction : MonoBehaviour {

    public SelectorLevel selectorLevel;
    public LocalizedString introductionTitle;
    public LocalizedTextAsset introductionTextAsset;

    public void RunIntroduction(bool forcePrint = false)
    {
        SelectorManager selectorManager = SelectorManager.Instance;
        if (!forcePrint && (!selectorManager.HasThisSelectorLevelOpen(selectorLevel) || selectorManager.PopupIsEnabled()))
        {
            return;
        }
        StartCoroutine(RunInitializationPopup(selectorManager));
    }

    protected IEnumerator RunInitializationPopup(SelectorManager selectorManager) {
        AsyncOperationHandle<TextAsset> handleTexte = introductionTextAsset.LoadAssetAsync();
        yield return handleTexte;
        selectorManager.popup.Initialize(
            title: introductionTitle.GetLocalizedString().Result,
            useTextAsset: true,
            textAsset: handleTexte.Result,
            theme: TexteExplicatif.Theme.NEUTRAL);
        selectorManager.popup.Run();
    }
}
