using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoSphere : MonoBehaviour {

    public GameObject geoPointPrefab;

    protected GameManager gm;
    protected List<GeoPoint> geoPoints;

    public void Initialize() {
        gm = GameManager.Instance;
        geoPoints = new List<GeoPoint>();
    }

    public void Update() {
        KeepOrientation();
    }

    protected void KeepOrientation() {
        transform.LookAt(transform.position + Vector3.forward);
    }

    public GeoPoint AddGeoPoint(GeoData geoData) {
        GeoPoint geoPoint = Instantiate(geoPointPrefab, parent: transform).GetComponent<GeoPoint>();
        geoPoint.Initialize(this, geoData);
        geoPoints.Add(geoPoint);
        return geoPoint;
    }

    public GeoPoint AddGeoPoint(GeoData geoData, Transform target) {
        geoData.targetObject = target;
        return AddGeoPoint(geoData);
    }

    public GeoPoint AddGeoPoint(GeoData geoData, Vector3 target) {
        geoData.targetObject = null;
        geoData.SetTargetPosition(target);
        return AddGeoPoint(geoData);
    }

    public bool RemoveGeoPoint(GeoPoint geoPoint) {
        return geoPoints.Remove(geoPoint);
    }

    public Vector3 GetPlayerPosition() {
        return gm.player.transform.position;
    }
}
