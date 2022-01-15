using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    public float dureeDestructionLowShrinkVoronoiSphere = 0.08f;

    [Header("Vue")]
    public VisualEffect lumiereHighVfx;
    public GameObject lumiereLow;
    public MeshRenderer lumiereLowVoronoiSphere;
    public MeshRenderer lumiereLowGlowingHeart;
    public VisualEffect lumiereLowVfx;
    public VisualEffect lumiereTrails;
    public GameObject pointLight;

    protected GameManager gm;
    protected bool isCaptured = false;
    protected LumiereQuality lumiereQuality;
    protected bool isAccessible = true;
    protected UnityEvent<Lumiere> onCapture = new UnityEvent<Lumiere>();

    protected virtual void Start () {
        gm = GameManager.Instance;
        SetName();
        //SetLumiereQuality((LumiereQuality)PrefsManager.GetInt(PrefsManager.DATA_QUALITY_KEY, (int)MenuOptions.defaultLumiereQuality));
        SetLumiereQuality(LumiereQuality.LOW);
	}

    public void SetName() {
        name = $"{GetType()} {transform.position} ({type})";
    }

    protected virtual void OnTriggerEnter(Collider hit) {
        if (hit.gameObject.name == "Joueur"){
            Captured();
		}
	}

    protected void Captured()
    {
        if (IsCaptured())
        {
            return;
        }
        isCaptured = true;

        onCapture.Invoke(this);

        gm.map.RemoveLumiere(this);

        AddToDataCount();

        AutoDestroy();

        CapturedSpecific();

        NotifyEventManagerLumiereCapture();

        NotifyConsoleLumiereCatpure();

        NotifySoundManager();

        ScreenShakeOnLumiereCapture();

        gm.timerManager.AddTime(timeBonus);
    }

    protected void NotifySoundManager() {
        gm.soundManager.PlayGetLumiereClip(transform.position);
        int nbDataTotal = gm.map.GetMaxNbLumieres();
        int nbDataRestantes = gm.map.GetLumieres().Count;
        gm.timerManager.TryUpdatePhase(nbDataTotal - nbDataRestantes, nbDataTotal);
    }

    public void DeactivateTrails() {
        lumiereTrails.enabled = false;
    }

    public void ActivateTrails() {
        lumiereTrails.enabled = true;
    }

    public bool HasTrails() {
        return type == LumiereType.ALMOST_FINAL || type == LumiereType.FINAL;
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
        Destroy(this.gameObject, lumiereQuality == LumiereQuality.HIGH ? dureeDestructionHigh : dureeDestructionLow);
        DestroyAnimation();
    }

    private void DestroyAnimation() {
        DestroyAnimationTrails();
        if (lumiereQuality == LumiereQuality.HIGH) {
            DestroyAnimationHigh();
        } else {
            DestroyAnimationLow();
        }
        Destroy(pointLight);
    }

    protected void DestroyAnimationTrails() {
        float trailsEjectionSpeed = lumiereTrails.GetFloat("TrailsSphereSpeed") * 2;
        lumiereTrails.SetFloat("TrailsEjectionSpeed", -trailsEjectionSpeed);
        lumiereTrails.SetFloat("TrailsSpawnRate", 0);
    }

    protected void DestroyAnimationHigh() {
        float turbulenceAttractionSpeed = lumiereHighVfx.GetFloat("EnveloppeSphereAttractionSpeed");
        lumiereHighVfx.SetFloat("EnveloppeSphereAttractionSpeed", -turbulenceAttractionSpeed);
        lumiereHighVfx.SetFloat("EnveloppeSpawnRate", 0);
        lumiereHighVfx.SetVector4("BeamColor", Vector4.zero);
        lumiereHighVfx.SetFloat("TailSpawnRate", 0);
    }

    protected void DestroyAnimationLow() {
        lumiereLowVfx.SendEvent("Capture");
        lumiereLowGlowingHeart.enabled = false;
        StartCoroutine(CShrinkVoronoiSphereLow());
    }

    protected IEnumerator CShrinkVoronoiSphereLow() {
        if (type != LumiereType.FINAL) { // Shrink Size
            Timer timer = new Timer(dureeDestructionLowShrinkVoronoiSphere);
            while (!timer.IsOver()) {
                lumiereLowVoronoiSphere.transform.localScale = Vector3.one * (1.0f - timer.GetAvancement());
                yield return null;
            }
            lumiereLowVoronoiSphere.transform.localScale = Vector3.zero;
        } else { // Shrink Alpha
            lumiereLowVoronoiSphere.material.SetFloat("_VoronoiRotationSpeed", 0.0f);
            Timer timer = new Timer(dureeDestructionLow);
            Vector2 initialTransparencyValues = lumiereLowVoronoiSphere.material.GetVector("_VoronoiTransparencyValues");
            float initialProportion = lumiereLowVoronoiSphere.material.GetFloat("_VoronoiTransparencyProportion");
            while (!timer.IsOver()) {
                float avancement = timer.GetAvancement();
                lumiereLowVoronoiSphere.material.SetVector("_VoronoiTransparencyValues", initialTransparencyValues * (1 - avancement));
                lumiereLowVoronoiSphere.material.SetFloat("_VoronoiTransparencyProportion", MathCurves.Linear(initialProportion, 1.1f, avancement));
                yield return null;
            }
            lumiereLowVoronoiSphere.material.SetVector("_VoronoiTransparencyValues", initialTransparencyValues * 0.0f);
            lumiereLowVoronoiSphere.material.SetFloat("_VoronoiTransparencyProportion", 1.1f);
        }
    }

    protected virtual void NotifyEventManagerLumiereCapture() {
        gm.eventManager.OnLumiereCaptured(type);
    }

    protected void ScreenShakeOnLumiereCapture() {
        CameraShaker.Instance.ShakeOnce(screenShakeMagnitude, screenShakeRoughness, 0.05f, 0.05f);
    }

    protected virtual void NotifyConsoleLumiereCatpure() {
        gm.console.AttraperLumiere(gm.map.GetLumieres().Count + gm.ennemiManager.GetNbDataSondeTriggers());
        gm.console.UpdateLastLumiereAttrapee();
    }

    protected virtual void CapturedSpecific() {
    }

    public bool IsCaptured() {
        return isCaptured;
    }

    public virtual void SetLumiereQuality(LumiereQuality quality) {
        lumiereQuality = quality;
        lumiereHighVfx.gameObject.SetActive(quality == LumiereQuality.HIGH);
        lumiereLow.SetActive(quality == LumiereQuality.LOW);
        if(quality == LumiereQuality.LOW) {
            lumiereLowGlowingHeart.material = new Material(lumiereLowGlowingHeart.material);
        }
    }

    public void SetInaccessible() {
        isAccessible = false;
    }

    public void SetAccessible() {
        isAccessible = true;
    }

    public bool IsAccessible() {
        return isAccessible;
    }

    public void RegisterOnCapture(UnityAction<Lumiere> call) {
        onCapture.AddListener(call);
    }
}
