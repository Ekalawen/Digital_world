using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;

public class GeoTargetUnderTreshold : MonoBehaviour {

    public GeoData geoData;
    public float enterTreshold = 3.0f;
    public float exitTreshold = 3.5f;

    protected GameManager gm;
    protected Player player;
    protected GeoPoint currentGeoPoint = null;

    public void Start() {
        gm = GameManager.Instance;
        player = gm.player;
        Assert.IsTrue(exitTreshold >= enterTreshold);
    }

    public void Update() {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if(currentGeoPoint == null && distanceToPlayer <= enterTreshold) {
            StartGeoPoint();
        }
        if(currentGeoPoint != null && distanceToPlayer > exitTreshold) {
            RemoveGeoPoint();
        }
    }

    protected void StartGeoPoint() {
        currentGeoPoint = player.geoSphere.AddGeoPoint(geoData);
    }

    protected void RemoveGeoPoint() {
        currentGeoPoint.Stop();
        currentGeoPoint = null;
    }
}
