using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SelectorPath_Password : SelectorPath {

    [Header("DonneesHackees")]
    public LocalizedTextAsset donneesHackees;
    public GoalTresholds goalTresholds;
    public int nbTresholdsToSeeTraceHint = 1;
    public TMPro.TMP_SpriteAsset imagesAtlas;

    [Header("Password")]
    public LocalizedString passwordPasse;
    public bool dontUseTrace = false;
    public Trace.AdviceType adviceType = Trace.AdviceType.COMPLETE;
    public int levenshteinDistance = 4;

    [Header("Links")]
    public SelectorPathCadenas cadena;

    public override void Initialize(SelectorPathUnlockScreen unlockScreen) {
        base.Initialize(unlockScreen);
        InitializeCadena();
    }

    protected void InitializeCadena() {
        cadena.Initialize();
        SetCadenaPosition();
        HighlightCadena(GetHighlitedState());
    }

    public override void Update() {
        base.Update();
        CloseIfEscape();
    }

    protected void CloseIfEscape() {
        if (!selectorManager.PopupIsEnabled()) {
            if (!MenuManager.DISABLE_HOTKEYS && Input.GetKeyDown(KeyCode.Escape)) {
                CloseUnlockScreen();
            }
        }
    }

    public TextAsset GetDataHackeesTextAsset() {
        AsyncOperationHandle<TextAsset> handle = donneesHackees.LoadAssetAsync();
        return handle.Result;
    }

    public List<int> GetTresholds() {
        return goalTresholds.tresholds;
    }

    public int GetNbUnlockedTresholds() {
        int currentTresholdValue = startLevel.menuLevel.GetCurrentTresholdValue();
        return GetTresholds().FindAll(i => i <= currentTresholdValue).Count;
    }

    protected void SetCadenaPosition() {
        Vector3 middle = GetMiddlePoint();
        cadena.transform.position = middle;
    }

    public void OnCadenaClicked() {
        OpenUnlockScreen();
    }

    public void OpenUnlockScreenInstant() {
        OpenUnlockScreen(instantDisplay: true);
    }

    public void OpenUnlockScreen(bool instantDisplay = false) {
        if (selectorManager.HasVerticalMenuOpen()) {
            if(selectorManager.HasUnlockScreenOpen() && selectorManager.GetCurrentPath() == this) {
                return;
            }
            selectorManager.selectorTarget.GoTo(cadena.transform.position, selectorManager.selectorTarget.GetMovingTime());
            selectorManager.BackAndDisplayUnlockScreen(this, instantDisplay);
        } else {
            selectorManager.GetCameraController().PlaceCameraInFrontOfPath(this);
            selectorManager.selectorTarget.GoTo(cadena.transform.position, selectorManager.selectorTarget.GetInTime());
            ReallyOpenUnlockScreen(instantDisplay);
        }
    }

    protected void ReallyOpenUnlockScreen(bool instantDisplay = false) {
        unlockScreen.gameObject.SetActive(true);
        unlockScreen.Initialize(this, GetHighlitedState());
        //if (!instantDisplay) {
        //    selectorManager.FadeIn(selectorManager.background.gameObject, selectorManager.dureeFading);
        //    selectorManager.FadeIn(unlockScreen.gameObject, selectorManager.dureeFading);
        //} else {
        //    selectorManager.background.gameObject.SetActive(true);
        //    selectorManager.background.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
        //    unlockScreen.gameObject.SetActive(true);
        //    unlockScreen.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
        //}
        selectorManager.background.gameObject.SetActive(true);
        unlockScreen.gameObject.SetActive(true);
        selectorManager.verticalMenuHandler.Open(instantOpen: instantDisplay);
        selectorManager.SetCurrentUnlockScreen(true, this);
    }

    public void CloseUnlockScreen(bool instantDisplay = false) {
        unlockScreen.DestroyAllParticles();
        //if (!instantDisplay) {
        //    selectorManager.FadeOut(selectorManager.background.gameObject, selectorManager.dureeFading);
        //    selectorManager.FadeOut(unlockScreen.gameObject, selectorManager.dureeFading);
        //} else {
        //    selectorManager.background.gameObject.SetActive(false);
        //    selectorManager.background.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
        //    unlockScreen.gameObject.SetActive(false);
        //    unlockScreen.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
        //}
        selectorManager.DisableIn(selectorManager.background.gameObject, selectorManager.verticalMenuHandler.closeTime);
        selectorManager.DisableIn(unlockScreen.gameObject, selectorManager.verticalMenuHandler.closeTime);
        selectorManager.verticalMenuHandler.Close(instantClose: instantDisplay);
        StartCoroutine(CDisableScreenOpennessNextFrame());
        selectorManager.selectorTarget.Shrink(selectorManager.selectorTarget.GetOutTime());
    }

    public void CloseUnlockScreenForFastUI() {
        unlockScreen.gameObject.SetActive(false);
        unlockScreen.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
        StartCoroutine(CDisableScreenOpennessNextFrame());
    }

    protected IEnumerator CDisableScreenOpennessNextFrame() {
        yield return null;
        selectorManager.SetCurrentUnlockScreen(false, this);
    }

    public string GetPassword() {
        if (!dontUseTrace)
            return GetTrace() + GetPasse();
        else
            return GetPasse();
    }

    public string GetPasse() {
        if (GetComponent<CustomPasse>() != null) {
            return GetCustomPasse();
        }
        return GetNormalPasse();
    }

    protected string GetNormalPasse() {
        AsyncOperationHandle<string> handle = passwordPasse.GetLocalizedString();
        string passe = handle.Result; // Peut générer des bugs ! Dans l'idée il faudrait vérifier. Mais si c'est pas load, je sais pas comment attendre sans pourrir tout le code avec de l'async :/
        if (passe == "No translation found for 'Empty' in Passes")
        { // Pas le choix car ils supportent pas les LocalizedStrings vides et qu'on est pas foutu de connaître les tables/entrys associés à une LocalizedString !
            passe = "";
        }
        Debug.Log($"Passe = {passe}");
        return passe;
    }

    protected string GetCustomPasse() {
        CustomPasse customPasse = GetComponent<CustomPasse>();
        return customPasse.GetPasse(this);
    }

    public string GetTrace() {
        string key = name + PrefsManager.TRACE;
        if (!PrefsManager.HasKey(key)) {
            InitTrace();
        }
        return PrefsManager.GetString(key, "0000");
    }

    protected void InitTrace() {
        string trace = Trace.GenerateTrace();
        print(trace);

        string key = name + PrefsManager.TRACE;
        PrefsManager.SetString(key, trace);
    }

    public int GetMaxTreshold() {
        return GetTresholds().Max();
    }

    public override void HighlightPath(bool state) {
        base.HighlightPath(state);
        HighlightCadena(state);
    }

    public void HighlightCadena(bool state) {
        cadena.GetComponent<AutoBouncer>().enabled = state;
    }

    public override TYPE GetPathType() {
        return TYPE.PASSWORD;
    }
}
