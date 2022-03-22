using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SelectorLevelRunIntroduction : MonoBehaviour {

    public LocalizedString introductionTitle;
    public LocalizedTextAsset introductionTextAsset;
    public AudioClipParams archivesClip;

    public void RunIntroduction() {
        StartCoroutine(RunInitializationPopup());
    }

    protected IEnumerator RunInitializationPopup() {
        SelectorManager selectorManager = SelectorManager.Instance;
        AsyncOperationHandle<TextAsset> handleTexte = introductionTextAsset.LoadAssetAsync();
        yield return handleTexte;
        selectorManager.popupArchives.Initialize(
            title: introductionTitle.GetLocalizedString().Result,
            useTextAsset: true,
            textAsset: handleTexte.Result,
            theme: TexteExplicatif.Theme.POSITIF);
        selectorManager.popupArchives.Run();
        StartArchivesClip(selectorManager.popupArchives);
    }

    protected void StartArchivesClip(TexteExplicatif popup) {
        if (archivesClip.clips.Count != 0) {
            popup.onDisable.AddListener(UISoundManager.Instance.StopArchivesClip);
            UISoundManager.Instance.PlayArchivesClip(archivesClip, usingInitializationArchives: true);
        } else {
            popup.GetComponentInChildren<PausePlayArchivesClipButtonsInitializer>().Disable();
        }
    }
}
