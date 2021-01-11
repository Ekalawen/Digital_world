using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
    public UnityEngine.Rendering.PostProcessing.PostProcessVolume hitVolume;

    [Header("Poussee")]
    public UnityEngine.Rendering.PostProcessing.PostProcessVolume pousseeVolume;

    [Header("Skybox")]
    public Color skyboxRectangleColor;
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

    public void Initialize() {
        gm = GameManager.Instance;
        camera = gm.player.camera;

        ResetSkyboxParameters();
    }

    protected void ResetSkyboxParameters() {
        RenderSettings.skybox.SetColor("_RectangleColor", skyboxRectangleColor);
        RenderSettings.skybox.SetFloat("_ProportionRectangles", skyboxProportionRectangles);
        RenderSettings.skybox.SetFloat("_ScrollSpeedPower", skyboxScrollSpeedPower);
        RenderSettings.skybox.SetFloat("_VariationsAmplitude", skyboxVariationsAmplitude);
    }

    public void UpdateGripEffect(Player.EtatPersonnage previousState) {
        if (PlayerPrefs.GetString(MenuOptions.GRIP_KEY) == "False")
            return;

        Player.EtatPersonnage etat = gm.player.GetEtat();
        if(previousState != Player.EtatPersonnage.AU_MUR && etat == Player.EtatPersonnage.AU_MUR) {
            // Activer !
            if(gripCoroutine != null)
                StopCoroutine(gripCoroutine);
            gripCoroutine = StartCoroutine(SetVignette(intensityGrip, changeTimeGrip, gripVolume));
        } else if (previousState == Player.EtatPersonnage.AU_MUR && etat != Player.EtatPersonnage.AU_MUR) {
            // Désactiver !
            if(gripCoroutine != null)
                StopCoroutine(gripCoroutine);
            gripCoroutine = StartCoroutine(SetVignette(0.0f, changeTimeGrip, gripVolume));
        }
    }

    public void UpdateHitEffect() {
        hitVolume.enabled = true;
        // REMOVE THIS COMMENT !
        //hitVolume.profile.GetSetting<Vignette>().intensity.Override(intensityHit);
        if (hitCoroutine1 != null) {
            StopCoroutine(hitCoroutine1);
            StopCoroutine(hitCoroutine2);
        }
        hitCoroutine2 = StartCoroutine(CUpdateHitEffect());
    }

    IEnumerator CUpdateHitEffect() {
        // REMOVE THIS COMMENT !
        //hitCoroutine1 = StartCoroutine(SetVignette(0.0f, changeTimeHit, hitVolume));
        yield return new WaitForSeconds(changeTimeHit);
        hitVolume.enabled = false;
    }

    IEnumerator SetVignette(float targetValue, float changeTime, Volume volume) {
        VolumeProfile profile = volume.profile;
        Vignette vignette;
        if(!profile.TryGet<Vignette>(out vignette)) {
            Debug.LogError($"Le profil {profile} doit contenir une vignette !");
        }
        float debut = Time.timeSinceLevelLoad;
        float current = Time.timeSinceLevelLoad;
        float amountNeeded = targetValue - vignette.intensity.GetValue<float>();

        while(Time.timeSinceLevelLoad - debut < changeTime) {
            float percentToAdd = (Time.timeSinceLevelLoad - current) / changeTime;
            float newValue = vignette.intensity.GetValue<float>() + percentToAdd * amountNeeded;
            vignette.intensity.Override(newValue);
            current = Time.timeSinceLevelLoad;
            yield return null;
        }
        vignette.intensity.Override(targetValue);
    }

    public void SetBlur(bool state) {
        pousseeVolume.gameObject.SetActive(state);
        //MotionBlur blur = pousseeVolume.profile.GetSetting<MotionBlur>();
        //blur.active = state;
    }

}
