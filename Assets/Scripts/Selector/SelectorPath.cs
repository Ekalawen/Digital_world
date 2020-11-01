using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorPath : MonoBehaviour {

    [Header("Path")]
    public SelectorLevel startLevel;
    public SelectorLevel endLevel;
    public List<GameObject> intermediatePoints;

    [Header("Password")]
    public string password = "passwd";

    [Header("Trails")]
    public float timeBetweenTrails = 0.3f;
    public Gradient trailColorUnlocked;
    public Gradient trailColorLocked;
    public GameObject trailPrefab;

    protected Timer trailTimer;
    protected List<GameObject> pathPoints;

    public void Start() {
        trailTimer = new Timer(timeBetweenTrails);
        LinkAllPoints();
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
}
