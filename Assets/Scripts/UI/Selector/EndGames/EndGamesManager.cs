using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGamesManager : MonoBehaviour {

    public enum State { FIRST_POPUP, SEPARER, CONSERVER };

    public EndGamesPopup firstPopup;
    public List<EndGamesPopup> separerPopups;
    public List<EndGamesPopup> conserverPopups;

    protected SelectorManager sm;
    protected State state = State.FIRST_POPUP;
    protected int popupIndice = 0;

    public void Initialize() {
        sm = SelectorManager.Instance;
    }

    public void StartEndGame() {
        state = State.FIRST_POPUP;
        popupIndice = 0;
        StartPopup(firstPopup);
    }

    protected void StartPopup(EndGamesPopup popup) {
        StartCoroutine(CStartPopup(popup));
    }

    protected IEnumerator CStartPopup(EndGamesPopup popup) {
        AsyncOperationHandle<string> handleTitle = popup.title.GetLocalizedString();
        yield return handleTitle;
        string titleString = handleTitle.Result;

        AsyncOperationHandle<TextAsset> handleText = popup.texte.LoadAssetAsync();
        yield return handleText;
        string textString = handleText.Result.text;

        sm.popup.Initialize(title: titleString, mainText: textString, theme: popup.theme);
        sm.popup.Run();
        sm.popup.RemoveDoneButton();
        if (popup.hasNo) {
            UnityAction action = state == State.FIRST_POPUP ? new UnityAction(RunFirstConserverPopup) : new UnityAction(RunNextPopup);
            sm.popup.AddButton(popup.no, popup.noTooltip, TexteExplicatif.Theme.NEGATIF, action: action);
        }
        if (popup.hasYes) {
            UnityAction action = state == State.FIRST_POPUP ? new UnityAction(RunFirstSeparerPopup) : new UnityAction(RunNextPopup);
            sm.popup.AddButton(popup.yes, popup.yesTooltip, TexteExplicatif.Theme.POSITIF, action: action);
        }
        sm.popup.EnableButtonsBlackBackground();
    }

    protected void RunFirstSeparerPopup() {
        state = State.SEPARER;
        RunNextPopup();
    }

    protected void RunFirstConserverPopup() {
        state = State.CONSERVER;
        RunNextPopup();
    }

    protected void RunNextPopup() {
        StartCoroutine(CRunNextPopup());
    }

    protected IEnumerator CRunNextPopup() {
        yield return new WaitForSeconds(sm.popup.dureeCloseAnimation);
        EndGamesPopup nextPopup = state == State.SEPARER ? separerPopups[popupIndice] : conserverPopups[popupIndice];
        popupIndice += 1;
        StartPopup(nextPopup);
    }
}
