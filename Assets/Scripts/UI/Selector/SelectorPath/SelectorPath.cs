using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class SelectorPath : MonoBehaviour {

    public enum TYPE { PASSWORD, DIRECT };

    [Header("Path")]
    public SelectorLevel startLevel;
    public SelectorLevel endLevel;
    public List<GameObject> intermediatePoints;
    public bool canUnlockInDemo = false;

    [Header("Trails")]
    public float timeBetweenTrails = 0.3f;
    public Gradient trailColorUnlocked;
    public Gradient trailColorLocked;
    public Gradient lineColorUnlocked;
    public Gradient lineColorLocked;
    public GameObject trailPrefab;
    public GameObject linePrefab;

    protected SelectorManager selectorManager;
    protected Timer trailTimer;
    protected List<Vector3> pathPoints;
    protected SelectorPathUnlockScreen unlockScreen;
    protected LineRenderer line;
    protected Fluctuator lineColorFluctuator;
    protected float lineColorAvancement;

    public virtual void Initialize(SelectorPathUnlockScreen unlockScreen) {
        selectorManager = SelectorManager.Instance;
        Debug.Log($"Path {this} initialized !");
        this.unlockScreen = unlockScreen;
        trailTimer = new Timer(timeBetweenTrails);
        lineColorFluctuator = new Fluctuator(this, GetLineColor, SetLineColor);
        LinkAllPoints();
        InitializeLine();
    }

    protected void LinkAllPoints() {
        pathPoints = intermediatePoints.Select(p => p.transform.position).ToList();
        if (startLevel != null)
            pathPoints.Insert(0, startLevel.gameObject.transform.position);
        if (endLevel != null)
            pathPoints.Insert(pathPoints.Count, endLevel.gameObject.transform.position);

        int nbPoints = pathPoints.Count;
        pathPoints[0] += (pathPoints[1] - pathPoints[0]).normalized * 0.5f;
        pathPoints[nbPoints - 1] += (pathPoints[nbPoints - 2] - pathPoints[nbPoints - 1]).normalized * 0.5f;
    }

    public virtual void Update() {
        if (selectorManager == null) // We don't want to update before being initialized !
            return;
        LinkPathPoints();
    }

    protected void LinkPathPoints() {
        if(pathPoints.Count < 2) {
            Debug.LogWarning("Un SelectorPath ne contient pas assez de pathPoints !");
            return;
        }

        if(trailTimer.IsOver()) {
            for(int i = 0; i < pathPoints.Count - 1; i++) {
                Vector3 source = pathPoints[i];
                Vector3 target = pathPoints[i + 1];
                ThrowTrail(source, target, GetGradientColor());
            }
            trailTimer.Reset();
        }
    }

    protected void InitializeLine()
    {
        if (pathPoints.Count < 2)
        {
            Debug.LogWarning("Un SelectorPath ne contient pas assez de pathPoints !");
            return;
        }

        line = Instantiate(linePrefab, parent: transform).GetComponent<LineRenderer>();
        line.positionCount = pathPoints.Count;
        line.SetPositions(pathPoints.ToArray());
        SetLineColor(IsUnlocked() ? 1 : 0);
    }

    protected void SetLineColor(float avancement) {
        line.colorGradient = MathTools.LerpGradients(lineColorLocked, lineColorUnlocked, avancement);
        lineColorAvancement = avancement;
    }

    protected float GetLineColor() {
        return lineColorAvancement;
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

    protected Vector3 GetMiddlePoint() {
        if(pathPoints.Count % 2 == 0) {
            Vector3 firstMiddle = pathPoints[pathPoints.Count / 2 - 1];
            Vector3 secondMiddle = pathPoints[pathPoints.Count / 2];
            return (firstMiddle + secondMiddle) / 2;
        } else {
            return pathPoints[(pathPoints.Count - 1) / 2];
        }
    }

    public void UnlockPath() {
        string key = name + PrefsManager.IS_UNLOCKED_PATH;
        PrefsManager.SetBool(key, true);
        lineColorFluctuator.GoTo(1.0f, unlockScreen.dureeUnlockAnimation);
    }

    public string GetVisibleName() {
        return $"{startLevel.GetVisibleName()} ==> {endLevel.GetVisibleName()}";
    }

    public string GetNameId() {
        return $"{startLevel.GetNameId()} ==> {endLevel.GetNameId()}";
    }

    public bool IsUnlocked() {
        string key = name + PrefsManager.IS_UNLOCKED_PATH;
        return PrefsManager.GetBool(key, false);
    }

    public void LockPath() {
        string key = name + PrefsManager.IS_UNLOCKED_PATH;
        PrefsManager.SetBool(key, false);
        Debug.Log($"{GetNameId()} locked !");
    }

    public virtual void HighlightPath(bool state) {
        string key = GetNameId() + PrefsManager.IS_HIGHLIGHTED_PATH;
        PrefsManager.SetBool(key, state);
    }

    public bool GetHighlitedState() {
        string key = GetNameId() + PrefsManager.IS_HIGHLIGHTED_PATH;
        return PrefsManager.GetBool(key, false);
    }

    public bool CanUnlockInDemo() {
        return !selectorManager.isDemo || canUnlockInDemo;
    }

    public abstract TYPE GetPathType();
}
