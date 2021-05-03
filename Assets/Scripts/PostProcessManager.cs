using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
//using UnityEngine.Rendering.PostProcessing;

public enum DissolveEffectType {
    REGULAR_MAP,
    INFINITE_MAP,
};

public class PostProcessManager : MonoBehaviour {

    [Header("Grip")]
    public float changeTimeGrip = 0.075f;
    public float intensityGrip = 0.315f;
    public Volume gripVolume;

    [Header("Hit")]
    public float changeTimeHit = 0.6f;
    public float intensityHit= 0.4f;
    public Volume hitVolume;

    [Header("Poussee")]
    public UnityEngine.Rendering.PostProcessing.PostProcessVolume pousseeVolume;

    [Header("Skybox")]
    public Color skyboxRectangleColor;
    public float skyboxRectangleColorIntensity;
    public float skyboxProportionRectangles = 0.4f;
    public float skyboxProportionRectanglesCriticalBound = 0.18f;
    public float skyboxScrollSpeedPower = 3.0f;
    public float skyboxVariationsAmplitude = 0.3f;

    [Header("CubeDissolve")]
    public float dissolveRegularTime = 3.0f;
    public float dissolveRegularPlayerProximityCoef = 0.037f;
    public float dissolveInfiniteTime = 0.0f;
    public float dissolveInfinitePlayerProximityCoef = -0.037f;
    public float dissolveInGameTime = 1.0f;
    public float dissolveInGamePlayerProximityCoef = 0.0f;

    protected Coroutine gripCoroutine = null;
    protected Coroutine hitCoroutine1 = null;
    protected Coroutine hitCoroutine2 = null;
    protected GameManager gm;
    protected new Camera camera;
    protected VisualEffect dashVfx;

    public void Initialize() {
        gm = GameManager.Instance;
        camera = gm.player.camera;
        dashVfx = gm.player.dashVfx;

        ResetSkyboxParameters();
        hitVolume.weight = 0;
    }

    protected void ResetSkyboxParameters() {
        RenderSettings.skybox.SetColor("_RectangleColor", GetSkyboxHDRColor(skyboxRectangleColor));
        RenderSettings.skybox.SetFloat("_ProportionRectangles", skyboxProportionRectangles);
        RenderSettings.skybox.SetFloat("_ScrollSpeedPower", skyboxScrollSpeedPower);
        RenderSettings.skybox.SetFloat("_VariationsAmplitude", skyboxVariationsAmplitude);
    }

    public Color GetSkyboxHDRColor(Color color) {
        return color;
        //return color * Mathf.Pow(2, skyboxRectangleColorIntensity);
    }

    public void UpdateGripEffect(Player.EtatPersonnage previousState) {
        if (!PrefsManager.GetBool(PrefsManager.GRIP_KEY, MenuOptions.defaultGripActivation)) {
            return;
        }

        Player.EtatPersonnage etat = gm.player.GetEtat();
        if(previousState != Player.EtatPersonnage.AU_MUR && etat == Player.EtatPersonnage.AU_MUR) {
            // Activer !
            if (gripCoroutine != null) {
                StopCoroutine(gripCoroutine);
            }
            gripCoroutine = StartCoroutine(SetVignette(intensityGrip, changeTimeGrip, gripVolume));
        } else if (previousState == Player.EtatPersonnage.AU_MUR && etat != Player.EtatPersonnage.AU_MUR) {
            // Désactiver !
            if (gripCoroutine != null) {
                StopCoroutine(gripCoroutine);
            }
            gripCoroutine = StartCoroutine(SetVignette(0.0f, changeTimeGrip, gripVolume));
        }
    }

    public void UpdateHitEffect() {
        hitVolume.weight = 1;
        Vignette vignette;
        if (!hitVolume.profile.TryGet<Vignette>(out vignette)) {
            Debug.LogError($"Le profil {hitVolume.profile} doit contenir une vignette !");
        }
        vignette.intensity.Override(intensityHit);
        if (hitCoroutine1 != null) {
            StopCoroutine(hitCoroutine1);
            StopCoroutine(hitCoroutine2);
        }
        hitCoroutine2 = StartCoroutine(CUpdateHitEffect());
    }

    IEnumerator CUpdateHitEffect() {
        hitCoroutine1 = StartCoroutine(SetVignette(0.0f, changeTimeHit, hitVolume));
        Timer timer = new Timer(changeTimeHit);
        while(!timer.IsOver()) {
            hitVolume.weight = MathCurves.Quadratic(0, 1, 1 - timer.GetAvancement());
            yield return null;
        }
        hitVolume.weight = 0;
    }

    IEnumerator SetVignette(float targetValue, float changeTime, Volume volume) {
        VolumeProfile profile = volume.profile;
        Vignette vignette;
        if(!profile.TryGet<Vignette>(out vignette)) {
            Debug.LogError($"Le profil {profile} doit contenir une vignette !");
        }
        float startValue = vignette.intensity.GetValue<float>();
        float amountNeeded = targetValue - startValue;

        Timer timer = new Timer(changeTime);
        while (!timer.IsOver()) {
            float newValue = startValue + timer.GetAvancement() * amountNeeded;
            vignette.intensity.Override(newValue);
            yield return null;
        }
        vignette.intensity.Override(targetValue);
    }

    public void SetBlur(bool state) {
        pousseeVolume.gameObject.SetActive(state);
        //MotionBlur blur = pousseeVolume.profile.GetSetting<MotionBlur>();
        //blur.active = state;
    }

    public void StartDashVfx(float duree) {
        dashVfx.SetFloat("Duration", duree);
        dashVfx.SendEvent("Dash");
    }
}
