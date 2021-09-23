using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Lightning : MonoBehaviour {

    public enum PivotType {
        EXTREMITY,
        CENTER,
    }

    public enum LightningMode {
        RAY,
        LINK,
    }

    public LightningMode lightningMode = LightningMode.RAY;
    [ConditionalHide("lightningMode", LightningMode.RAY)]
    public float vitesse = 10.0f;
    [ConditionalHide("lightningMode", LightningMode.RAY)]
    public float durationAfterArriving = 0.5f;
    [ConditionalHide("lightningMode", LightningMode.LINK)]
    public float lifetime = 1.0f;
    [ConditionalHide("lightningMode", LightningMode.LINK)]
    public float refreshRate = 1.0f;
    [ConditionalHide("lightningMode", LightningMode.LINK)]
    public float timeAtMaxWidth = 0.3f;
    public VisualEffect vfx;

    protected GameManager gm;
    protected Vector3 start;
    protected Vector3 end;
    protected PivotType pivotType;
    protected Coroutine refreshCoroutine = null;

    public void Initialize(Vector3 start, Vector3 end, PivotType pivotType = PivotType.EXTREMITY) {
        SetGmAndParent();
        InitializeLightning(start, end, pivotType);
        AutoDestroyRayIn();
    }

    public void InitializeWithoutGM(Vector3 start, Vector3 end, Transform parent, PivotType pivotType = PivotType.EXTREMITY) {
        transform.SetParent(parent);
        InitializeLightning(start, end, pivotType);
        AutoDestroyRayIn();
    }

    protected void InitializeLightning(Vector3 start, Vector3 end, PivotType pivotType) {
        if (lightningMode == LightningMode.RAY) {
            this.pivotType = pivotType;
            SetPosition(start, end);
            SetdurationAfterArriving(durationAfterArriving);
        } else { // LightningMode.LINK
            this.pivotType = pivotType;
            SetPosition(start, end);
            SetLifetime(lifetime, timeAtMaxWidth);
            refreshCoroutine = StartCoroutine(CUpdateConstantLightning());
        }
    }

    protected void SetGmAndParent() {
        gm = GameManager.Instance;
        if (transform.parent == null) {
            transform.SetParent(gm.map.lightningsFolder);
        }
    }

    protected void AutoDestroyRayIn() {
        if(lightningMode == LightningMode.RAY) {
            float distance = (end - start).magnitude;
            float durationToArriving = distance / vitesse;
            float lifetime = durationAfterArriving + durationToArriving;
            Destroy(gameObject, lifetime);
        }
    }

    public void SetDurees(float dureeAvantConnection, float distance, float dureeApresConnection) {
        // Must be called before Initialize ! x)
        vitesse = distance / dureeAvantConnection;
        durationAfterArriving = dureeApresConnection;
    }

    public void SetPosition(Vector3 start, Vector3 end, float parentSize = 1) {
        this.start = start;
        this.end = end;
        if (pivotType == PivotType.EXTREMITY) {
            transform.position = start;
        } else {
            transform.position = (start + end) / 2;
        }
        transform.forward = (start != end) ? (end - start).normalized : Vector3.right;
        float distance = (end - start).magnitude;
        distance /= parentSize;
        vfx.SetFloat("Lenght", distance);
    }

    public void SetdurationAfterArriving(float durationAfterArriving) {
        float distance = (end - start).magnitude;
        float durationToArriving = distance / vitesse;
        float duration = durationToArriving + durationAfterArriving;
        vfx.SetFloat("Lifetime", duration);
        vfx.SetFloat("TimeAtMaxLength", durationToArriving / duration);
    }

    public void SetLifetime(float lifetime, float timeAtMaxWidth) {
        vfx.SetFloat("Lifetime", lifetime);
        vfx.SetFloat("TimeAtMaxLength", 0);
        vfx.SetFloat("TimeAtMaxWidth", timeAtMaxWidth / lifetime);
    }

    protected IEnumerator CUpdateConstantLightning() {
        while(true) {
            yield return new WaitForSeconds(refreshRate);
            vfx.SendEvent("OnPlay");
        }
    }

    public void StopRefreshing() {
        if(refreshCoroutine != null) {
            StopCoroutine(refreshCoroutine);
        }
    }
}
