using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GeoPoint : MonoBehaviour {

    public static string VFX_COLOR = "Color";
    public static string VFX_DURATION = "Duration";
    public static string VFX_SPAWN_RATE = "SpawnRate";
    public static string VFX_BURST_COUNT = "BurstCount";
    public static string VFX_LIFETIME = "Lifetime";

    public GeoData data;
    public VisualEffect vfx;

    protected GeoSphere geoSphere;
    protected float distanceToSphere;
    protected Timer timer;

    public void Initialize(GeoSphere geoSphere, GeoData data) {
        this.geoSphere = geoSphere;
        this.data = data;
        this.distanceToSphere = transform.localPosition.y * transform.lossyScale.x;
        timer = new Timer(data.GetDuration() + GetMaxParticleDuration());
        InitVfx();
        SetPosition();
    }

    protected void SetPosition() {
        Vector3 target = data.GetTargetAndSaveIt();
        Vector3 geoSpherePosition = geoSphere.transform.position;
        Vector3 playerPosition = geoSphere.GetPlayerPosition();
        Vector3 direction = (target - playerPosition).normalized;
        transform.position = geoSpherePosition + direction * distanceToSphere;
        transform.LookAt(geoSpherePosition);
    }

    protected void InitVfx() {
        vfx.SetVector4(VFX_COLOR, data.color);
        vfx.SetFloat(VFX_DURATION, data.duration);
        if (data.type == GeoData.GeoPointType.IMPACT) {
            vfx.SetFloat(VFX_BURST_COUNT, vfx.GetFloat(VFX_SPAWN_RATE));
        }
        vfx.SendEvent("Start");
    }

    public void Update() {
        SetPosition();
        if(timer.IsOver()) {
            RemovePoint();
        }
    }

    public void StopVfx() {
        vfx.SendEvent("Stop");
        StartCoroutine(CRemovePointIn(GetMaxParticleDuration()));
    }

    protected IEnumerator CRemovePointIn(float duration) {
        yield return new WaitForSeconds(duration);
        RemovePoint();
    }

    protected void RemovePoint() {
        geoSphere.RemoveGeoPoint(this);
        Destroy(gameObject);
    }

    public float GetMaxParticleDuration() {
        return vfx.GetVector2(VFX_LIFETIME).y;
    }
}
