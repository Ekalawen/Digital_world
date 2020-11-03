using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorPath : MonoBehaviour {

    [Header("Path")]
    public SelectorLevel startLevel;
    public SelectorLevel endLevel;
    public List<GameObject> intermediatePoints;
    public TextAsset donneesHackees;

    [Header("Password")]
    public string password = "passwd";

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
        SetCadenaPosition();
    }

    protected void LinkAllPoints() {
        pathPoints = intermediatePoints;
        if (startLevel != null)
            pathPoints.Insert(0, startLevel.gameObject);
        if (endLevel != null)
            pathPoints.Insert(pathPoints.Count, endLevel.gameObject);
    }

    public void Update() {
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
    
    protected void ThrowTrail(Vector3 source, Vector3 target, Gradient gradient) {
        GameObject tr = Instantiate(trailPrefab, source, Quaternion.identity, transform) as GameObject;
        tr.GetComponent<Trail>().SetTarget(target);
        tr.GetComponent<TrailRenderer>().colorGradient = gradient;
    }

    public bool IsUnlocked() {
        return true;
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
        selectorManager.FadeIn(selectorManager.background.gameObject, selectorManager.dureeFading);
        unlockScreen.gameObject.SetActive(true);
        unlockScreen.Initialize(this);
        selectorManager.FadeIn(unlockScreen.gameObject, selectorManager.dureeFading);
        selectorManager.SetSelectorPathUnlockScreenOpenness(true);
    }

    public string GetTrace() {
        return "00TODOPATH00";
    }

    public void CloseUnlockScreen() {
        selectorManager.FadeOut(selectorManager.background.gameObject, selectorManager.dureeFading);
        selectorManager.FadeOut(unlockScreen.gameObject, selectorManager.dureeFading);
        StartCoroutine(CDisableScreenOpennessNextFrame());
    }

    protected IEnumerator CDisableScreenOpennessNextFrame() {
        yield return null;
        selectorManager.SetSelectorPathUnlockScreenOpenness(false);
    }

    public string GetPassword() {
        return password;
    }

    public void Unlock(string passwordUsed) {
        Debug.Log("I am unlocked !");
    }
}
