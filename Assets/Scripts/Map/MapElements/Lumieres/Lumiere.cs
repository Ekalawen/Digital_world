﻿using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

public class Lumiere : MonoBehaviour {

    public enum LumiereType {
        NORMAL,
        FINAL,
        ALMOST_FINAL,
        SPECIAL,
    };

    public enum LumiereQuality {
        HIGH,
        LOW,
    };

    [Header("Propriétés")]
    public LumiereType type;
    public float timeBonus = 10.0f;

    [Header("Reward")]
    public GameObject rewardLumierePrefab;

    [Header("ScreenShake")]
    public float screenShakeMagnitude = 10;
    public float screenShakeRoughness = 10;

    [Header("Destruction")]
    public float dureeDestructionHigh = 3.0f;
    public float dureeDestructionLow = 1.0f;

    [Header("Vue")]
    public VisualEffect lumiereHighVfx;
    public GameObject lumiereLow;
    public GameObject pointLight;

    protected GameManager gm;
    protected bool isCaptured = false;
    protected LumiereQuality lumiereQuality;

    protected virtual void Start () {
        gm = GameManager.Instance;
        SetLumiereQuality(LumiereQuality.HIGH);
	}

    protected virtual void OnTriggerEnter(Collider hit) {
        if (hit.gameObject.name == "Joueur"){
            Captured();
		}
	}

    protected void Captured() {
        if (IsCaptured()) {
            return;
        }
        isCaptured = true;

        gm.map.RemoveLumiere(this);

        AddToDataCount();

        AutoDestroy();

        CapturedSpecific();

        NotifyEventManagerLumiereCapture();

        NotifyConsoleLumiereCatpure();

        gm.soundManager.PlayGetLumiereClip(transform.position);

        ScreenShakeOnLumiereCapture();

        gm.timerManager.AddTime(timeBonus);
    }

    public static int GetCurrentDataCount() {
        string key = SceneManager.GetActiveScene().name + PrefsManager.DATA_COUNT_KEY;
        return PrefsManager.GetInt(key, 0);
    }

    public static int IncrementDataCount(int nbAdded) {
        string key = SceneManager.GetActiveScene().name + PrefsManager.DATA_COUNT_KEY;
        int dataCount = GetCurrentDataCount() + nbAdded;
        PrefsManager.SetInt(key, dataCount);
        string keyHasJustIncreased = SceneManager.GetActiveScene().name + PrefsManager.HAS_JUST_INCREASED_DATA_COUNT_KEY;
        PrefsManager.SetBool(keyHasJustIncreased, true);
        return dataCount;
    }

    protected void AddToDataCount() {
        int dataCount = IncrementDataCount(1);
        gm.console.AddToDataCountText(dataCount, 1);
    }

    private void AutoDestroy() {
        Destroy(this.gameObject, GetDureeDestruction());
        DestroyAnimation();
    }

    private void DestroyAnimation() {
        if (lumiereQuality == LumiereQuality.HIGH) {
            DestroyAnimationHigh();
        } else {
            DestroyAnimationLow();
        }
        Destroy(pointLight);
    }

    protected void DestroyAnimationHigh() {
        float turbulenceAttractionSpeed = lumiereHighVfx.GetFloat("EnveloppeSphereAttractionSpeed");
        lumiereHighVfx.SetFloat("EnveloppeSphereAttractionSpeed", -turbulenceAttractionSpeed);
        float trailsEjectionSpeed = lumiereHighVfx.GetFloat("TrailsSphereSpeed") * 2;
        lumiereHighVfx.SetFloat("TrailsEjectionSpeed", -trailsEjectionSpeed);
        lumiereHighVfx.SetFloat("EnveloppeSpawnRate", 0);
        lumiereHighVfx.SetFloat("TrailsSpawnRate", 0);
        lumiereHighVfx.SetVector4("BeamColor", Vector4.zero);
        lumiereHighVfx.SetFloat("TailSpawnRate", 0);
    }

    protected void DestroyAnimationLow() {
        StartCoroutine(CDestroyAnimationLow());
    }

    protected IEnumerator CDestroyAnimationLow() {
        Timer timer = new Timer(GetDureeDestruction());
        while(!timer.IsOver()) {
            lumiereLow.transform.localScale = Vector3.one * (1.0f - timer.GetAvancement());
            yield return null;
        }
        lumiereLow.transform.localScale = Vector3.zero;
    }

    protected virtual void NotifyEventManagerLumiereCapture() {
        gm.eventManager.OnLumiereCaptured(type);
    }

    protected void ScreenShakeOnLumiereCapture() {
        CameraShaker.Instance.ShakeOnce(screenShakeMagnitude, screenShakeRoughness, 0.05f, 0.05f);
    }

    protected virtual void NotifyConsoleLumiereCatpure() {
        gm.console.AttraperLumiere(gm.map.GetLumieres().Count);
        gm.console.UpdateLastLumiereAttrapee();
    }

    protected virtual void CapturedSpecific() {
    }

    public bool IsCaptured() {
        return isCaptured;
    }

    public void SetLumiereQuality(LumiereQuality quality) {
        lumiereQuality = quality;
        lumiereHighVfx.gameObject.SetActive(quality == LumiereQuality.HIGH);
        lumiereLow.SetActive(quality == LumiereQuality.LOW);
    }

    public float GetDureeDestruction() {
        return lumiereQuality == LumiereQuality.HIGH ? dureeDestructionHigh : dureeDestructionLow;
    }
}
