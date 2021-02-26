using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SelectorPath : MonoBehaviour {

    [Header("Path")]
    public SelectorLevel startLevel;
    public SelectorLevel endLevel;
    public List<GameObject> intermediatePoints;

    [Header("DonneesHackees")]
    public LocalizedTextAsset donneesHackees;
    public GoalTresholds goalTresholds;
    public int nbTresholdsToSeeTraceHint = 1;

    [Header("Password")]
    public LocalizedString passwordPasse;
    public bool dontUseTrace = false;
    public Trace.AdviceType adviceType = Trace.AdviceType.COMPLETE;

    [Header("Trails")]
    public float timeBetweenTrails = 0.3f;
    public Gradient trailColorUnlocked;
    public Gradient trailColorLocked;
    public GameObject trailPrefab;

    [Header("Links")]
    public SelectorPathCadenas cadena;

    protected SelectorManager selectorManager;
    protected Timer trailTimer;
    protected List<GameObject> pathPoints;
    protected SelectorPathUnlockScreen unlockScreen;

    public void Initialize(SelectorPathUnlockScreen unlockScreen) {
        selectorManager = SelectorManager.Instance;
        this.unlockScreen = unlockScreen;
        trailTimer = new Timer(timeBetweenTrails);
        LinkAllPoints();
        InitializeCadena();
    }

    protected void InitializeCadena() {
        cadena.Initialize();
        SetCadenaPosition();
        HighlightCadena(GetHighlitedState());
    }

    protected void LinkAllPoints() {
        pathPoints = intermediatePoints;
        if (startLevel != null)
            pathPoints.Insert(0, startLevel.gameObject);
        if (endLevel != null)
            pathPoints.Insert(pathPoints.Count, endLevel.gameObject);
    }

    public void Update() {
        if (selectorManager == null) // We don't want to update before being initialized !
            return;
        LinkPathPoints();
        CloseIfEscape();
    }

    protected void CloseIfEscape() {
        if (!selectorManager.PopupIsEnabled()) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                CloseUnlockScreen();
            }
        }
    }

    protected void LinkPathPoints() {
        if(pathPoints.Count < 2) {
            Debug.LogWarning("Un SelectorPath ne contient pas assez de pathPoints !");
            return;
        }

        if(trailTimer.IsOver()) {
            for(int i = 0; i < pathPoints.Count - 1; i++) {
                Vector3 source = pathPoints[i].transform.position;
                Vector3 target = pathPoints[i + 1].transform.position;
                ThrowTrail(source, target, GetGradientColor());
            }
            trailTimer.Reset();
        }
    }

    public TextAsset GetDataHackeesTextAsset() {
        AsyncOperationHandle<TextAsset> handle = donneesHackees.LoadAssetAsync();
        return handle.Result;
    }

    public List<int> GetTresholds() {
        return goalTresholds.tresholds;
    }

    protected void ThrowTrail(Vector3 source, Vector3 target, Gradient gradient) {
        GameObject tr = Instantiate(trailPrefab, source, Quaternion.identity, transform) as GameObject;
        tr.GetComponent<Trail>().SetTarget(target);
        tr.GetComponent<TrailRenderer>().colorGradient = gradient;
    }

    public Gradient GetGradientColor() {
        if (IsUnlocked())
            return trailColorUnlocked;
        return trailColorLocked;
    }

    protected void SetCadenaPosition() {
        Vector3 middle = GetMiddlePoint();
        cadena.transform.position = middle;
    }

    protected Vector3 GetMiddlePoint() {
        if(pathPoints.Count % 2 == 0) {
            Vector3 firstMiddle = pathPoints[pathPoints.Count / 2 - 1].transform.position;
            Vector3 secondMiddle = pathPoints[pathPoints.Count / 2].transform.position;
            return (firstMiddle + secondMiddle) / 2;
        } else {
            return pathPoints[(pathPoints.Count - 1) / 2].transform.position;
        }
    }

    public void OnCadenaClicked() {
        OpenUnlockScreen();
    }

    public void OpenUnlockScreen(bool instantDisplay = false) {
        unlockScreen.gameObject.SetActive(true);
        unlockScreen.Initialize(this, GetHighlitedState());
        if (!instantDisplay) {
            selectorManager.FadeIn(selectorManager.background.gameObject, selectorManager.dureeFading);
            selectorManager.FadeIn(unlockScreen.gameObject, selectorManager.dureeFading);
        } else {
            selectorManager.background.gameObject.SetActive(true);
            selectorManager.background.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            unlockScreen.gameObject.SetActive(true);
            unlockScreen.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
        }
        selectorManager.SetSelectorPathUnlockScreenOpenness(true);
    }

    public void CloseUnlockScreen(bool instantDisplay = false) {
        if (!instantDisplay) {
            selectorManager.FadeOut(selectorManager.background.gameObject, selectorManager.dureeFading);
            selectorManager.FadeOut(unlockScreen.gameObject, selectorManager.dureeFading);
        } else {
            selectorManager.background.gameObject.SetActive(false);
            selectorManager.background.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
            unlockScreen.gameObject.SetActive(false);
            unlockScreen.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
        }
        StartCoroutine(CDisableScreenOpennessNextFrame());
    }

    public void CloseUnlockScreenForFastUI() {
        unlockScreen.gameObject.SetActive(false);
        unlockScreen.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
        StartCoroutine(CDisableScreenOpennessNextFrame());
    }

    protected IEnumerator CDisableScreenOpennessNextFrame() {
        yield return null;
        selectorManager.SetSelectorPathUnlockScreenOpenness(false);
    }

    public string GetPassword() {
        if (!dontUseTrace)
            return GetTrace() + GetPasse();
        else
            return GetPasse();
    }

    public string GetPasse() {
        AsyncOperationHandle<string> handle = passwordPasse.GetLocalizedString();
        Debug.Log($"Passe = {handle.Result}");
        return handle.Result; // Peut générer des bugs ! Dans l'idée il faudrait vérifier. Mais si c'est pas load, je sais pas comment attendre sans pourrir tout le code avec de l'async :/
    }

    public string GetTrace() {
        string key = name + PrefsManager.TRACE_KEY;
        if (!PrefsManager.HasKey(key)) {
            InitTrace();
        }
        return PrefsManager.GetString(key, "0000");
    }

    protected void InitTrace() {
        string trace = Trace.GenerateTrace();
        print(trace);

        string key = name + PrefsManager.TRACE_KEY;
        PrefsManager.SetString(key, trace);
    }

    public void UnlockPath() {
        string key = name + PrefsManager.IS_UNLOCKED_PATH_KEY;
        PrefsManager.SetBool(key, true);
    }

    public string GetVisibleName() {
        return $"{startLevel.GetVisibleName()} ==> {endLevel.GetVisibleName()}";
    }

    public string GetNameId() {
        return $"{startLevel.GetNameId()} ==> {endLevel.GetNameId()}";
    }

    public bool IsUnlocked() {
        string key = name + PrefsManager.IS_UNLOCKED_PATH_KEY;
        return PrefsManager.GetBool(key, false);
    }

    public void LockPath() {
        string key = name + PrefsManager.IS_UNLOCKED_PATH_KEY;
        PrefsManager.SetBool(key, false);
        Debug.Log($"{GetNameId()} locked !");
    }

    public int GetMaxTreshold() {
        return GetTresholds().Max();
    }

    public void HighlightPath(bool state) {
        string key = GetNameId() + PrefsManager.IS_HIGHLIGHTED_PATH_KEY;
        PrefsManager.SetBool(key, state);
        HighlightCadena(state);
    }

    public void HighlightCadena(bool state) {
        cadena.GetComponent<AutoBouncer>().enabled = state;
    }

    public bool GetHighlitedState() {
        string key = GetNameId() + PrefsManager.IS_HIGHLIGHTED_PATH_KEY;
        return PrefsManager.GetBool(key, false);
    }
}
