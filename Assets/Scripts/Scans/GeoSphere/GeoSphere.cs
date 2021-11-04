using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GeoSphere : MonoBehaviour {

    public static string VFX_BASE_STATE = "_BaseState";
    public static string VFX_TRANSITION_IN_STARTING_TIME = "_TransitionINStartingTime";
    public static string VFX_TRANSITION_IN_DURATION = "_TransitionINDuration";
    public static string VFX_TRANSITION_OUT_STARTING_TIME = "_TransitionOUTStartingTime";
    public static string VFX_TRANSITION_OUT_DURATION = "_TransitionOUTDuration";
    
    public enum SphereTransitionType { IN, OUT };

    [Header("Links")]
    public GameObject geoPointPrefab;

    [Header("Material")]
    public Renderer renderer;
    public float transitionInDuration = 3.0f;
    public float transitionOutDuration = 1.0f;

    protected GameManager gm;
    protected List<GeoPoint> geoPoints;
    protected Material material;

    public void Initialize() {
        gm = GameManager.Instance;
        geoPoints = new List<GeoPoint>();
        renderer.material = new Material(renderer.material);
        material = renderer.material;
        HideSphere();
    }

    protected void HideSphere() {
        material.SetFloat(VFX_BASE_STATE, 0);
        material.SetFloat(VFX_TRANSITION_IN_STARTING_TIME, Time.time - transitionInDuration);
        material.SetFloat(VFX_TRANSITION_IN_DURATION, transitionInDuration);
        material.SetFloat(VFX_TRANSITION_OUT_STARTING_TIME, Time.time - transitionOutDuration);
        material.SetFloat(VFX_TRANSITION_OUT_DURATION, transitionOutDuration);
    }

    protected void StartSphereTransition(SphereTransitionType type) {
        if (type == SphereTransitionType.IN) {
            material.SetFloat(VFX_BASE_STATE, 1);
            material.SetFloat(VFX_TRANSITION_IN_STARTING_TIME, Time.time);
            material.SetFloat(VFX_TRANSITION_IN_DURATION, transitionInDuration);
        } else {
            material.SetFloat(VFX_BASE_STATE, 0);
            material.SetFloat(VFX_TRANSITION_OUT_STARTING_TIME, Time.time);
            material.SetFloat(VFX_TRANSITION_OUT_DURATION, transitionOutDuration);
        }
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
        if(geoPoints.Count == 0) {
            StartSphereTransition(SphereTransitionType.IN);
        }
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
        bool succeeded = geoPoints.Remove(geoPoint);
        if(geoPoints.Count == 0) {
            StartSphereTransition(SphereTransitionType.OUT);
        }
        return succeeded;
    }

    public Vector3 GetPlayerPosition() {
        return gm.player.transform.position;
    }
}
