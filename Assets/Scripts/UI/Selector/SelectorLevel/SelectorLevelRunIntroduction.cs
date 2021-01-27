using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SelectorLevelRunIntroduction : MonoBehaviour {

    public LocalizedString introductionTitle;
    public LocalizedTextAsset introductionTextAsset;

    public void RunIntroduction() {
        StartCoroutine(RunInitializationPopup());
    }

    protected IEnumerator RunInitializationPopup() {
        SelectorManager selectorManager = SelectorManager.Instance;
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
