using EZCameraShake;
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

    [Header("Popups")]
    public EndGamesPopup firstPopup;
    public List<EndGamesPopup> separerPopups;
    public List<EndGamesPopup> conserverPopups;

    [Header("Parameters")]
    public Vector4 buttonsMargins = new Vector4(5, 5, 10, 5);
    public float fadeOutDelay = 8.0f;
    public float fadeOutDuration = 8.0f;
    public float afterFadeOutDelay = 1.0f;
    public float cameraShakeMagnitude = 10.0f;
    public float cameraShakeRoughness = 10.0f;
    public float cameraShakeFadeIn = 16.0f;

    protected SelectorManager sm;
    protected State state = State.FIRST_POPUP;
    protected int popupIndice = 0;
    protected Fluctuator fadeOutFluctuator;
    protected Fluctuator musicVolumeFluctuator;

    public void Initialize() {
        sm = SelectorManager.Instance;
        InitFluctuators();
    }

    protected void InitFluctuators() {
        UnityEngine.Rendering.Volume luminosityVolume = sm.GetCameraController().luminosityVolume;
        fadeOutFluctuator = new Fluctuator(this,
            () => PostProcessManager.GetLuminosityIntensity(luminosityVolume),
            (value) => PostProcessManager.SetLuminosityIntensity(luminosityVolume, value));
        UISoundManager soundManager = UISoundManager.Instance;
        musicVolumeFluctuator = new Fluctuator(this, soundManager.GetMusicVolume, soundManager.SetMusicVolume);
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
        sm.popup.StopAllCoroutines();

        AsyncOperationHandle<string> handleTitle = popup.title.GetLocalizedString();
        yield return handleTitle;
        string titleString = handleTitle.Result;

        AsyncOperationHandle<TextAsset> handleText = popup.texte.LoadAssetAsync();
        yield return handleText;
        string textString = handleText.Result.text;

        sm.popup.Initialize(title: titleString, mainText: textString, theme: popup.theme, canBeDisabledWithShortcut: false);
        sm.popup.Run(replacements: sm.archivesReplacementStrings);
        sm.popup.RemoveDoneButton();
        sm.popup.RemoveAddedButtons();
        if (popup.hasNo) {
            UnityAction action = state == State.FIRST_POPUP ? new UnityAction(RunFirstConserverPopup) : new UnityAction(RestartEndGame);
            Button button = sm.popup.AddButton(popup.no, popup.noTooltip, TexteExplicatif.Theme.NEGATIF, action: action);
            button.gameObject.GetComponentInChildren<TMPro.TMP_Text>().margin = buttonsMargins;
        }
        if (popup.hasYes) {
            UnityAction action = state == State.FIRST_POPUP ? new UnityAction(RunFirstSeparerPopup) : new UnityAction(RunNextPopup);
            Button button = sm.popup.AddButton(popup.yes, popup.yesTooltip, TexteExplicatif.Theme.POSITIF, action: action);
            button.gameObject.GetComponentInChildren<TMPro.TMP_Text>().margin = buttonsMargins;
        }

        if(popup.shouldAutomaticallyTransition) {
            RunNextPopupIn(popup.automaticTransitionDelay);
        }
    }

    protected void RunNextPopupIn(float delay) {
        StartCoroutine(CRunNextPopupIn(delay));
    }

    protected IEnumerator CRunNextPopupIn(float delay) {
        yield return new WaitForSeconds(delay);
        RunNextPopup();
    }

    protected void RestartEndGame() {
        StartCoroutine(CRestartEndGame());
    }

    protected IEnumerator CRestartEndGame() {
        yield return new WaitForSeconds(sm.popup.dureeCloseAnimation);
        StartEndGame();
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
        sm.popup.Disable();
        yield return new WaitForSeconds(sm.popup.dureeCloseAnimation);
        if (popupIndice < separerPopups.Count) { // On assume que separerPopups.Count == conserverPopups.Count
            EndGamesPopup nextPopup = state == State.SEPARER ? separerPopups[popupIndice] : conserverPopups[popupIndice];
            popupIndice += 1;
            StartPopup(nextPopup);
            if(state == State.SEPARER && popupIndice == separerPopups.Count - 1) {
                StartShaking();
            }
            if (state == State.SEPARER && popupIndice == separerPopups.Count) {
                StartCredits();
            }
        }
    }

    protected void StartShaking() {
        CameraShaker.Instance.StartShake(cameraShakeMagnitude, cameraShakeRoughness, cameraShakeFadeIn);
    }

    protected void StartCredits() {
        StartCoroutine(CStartCredits());
    }

    protected IEnumerator CStartCredits() {
        Debug.Log($"StartCredits ! :)");
        yield return new WaitForSeconds(fadeOutDelay);

        fadeOutFluctuator.GoTo(-1, fadeOutDuration);
        musicVolumeFluctuator.GoTo(0, fadeOutDuration);

        yield return new WaitForSeconds(fadeOutDuration + afterFadeOutDelay);

        ResetSaves();
        SceneManager.LoadScene($"CreditsScene");
    }

    protected void ResetSaves() {
        PrefsManager.SetBool(PrefsManager.SHOULD_RESET_SAVE_ON_NEXT_MENU_SCENE, true);
    }
}
