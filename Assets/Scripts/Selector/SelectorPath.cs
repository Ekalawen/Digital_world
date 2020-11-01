using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorPath : MonoBehaviour {

    [Header("Path")]
    public List<GameObject> pathPoints;

    [Header("Trails")]
    public float timeBetweenTrails = 0.3f;
    public Gradient trailColorUnlocked;
    public Gradient trailColorLocked;
    public GameObject trailPrefab;

    protected Timer trailTimer;

    public void Start() {
        trailTimer = new Timer(timeBetweenTrails);
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
                Gradient gradient = i % 2 == 0 ? trailColorUnlocked : trailColorLocked;
                ThrowTrail(source, target, gradient);
            }
            trailTimer.Reset();
        }
    }
    
    protected void ThrowTrail(Vector3 source, Vector3 target, Gradient gradient) {
        GameObject tr = Instantiate(trailPrefab, source, Quaternion.identity, transform) as GameObject;
        tr.GetComponent<Trail>().SetTarget(target);
        tr.GetComponent<TrailRenderer>().colorGradient = gradient;
    }
}
