using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManager : MonoBehaviour {

    public float skyboxRotationSpeed = 25.0f;
    public float changeTimeGrip = 0.075f;
    public float intensityGrip = 0.315f;
    public PostProcessVolume gripVolume;

    public float changeTimeHit = 0.6f;
    public float intensityHit= 0.4f;
    public PostProcessVolume hitVolume;

    public bool useScan = false;
    public float scanRange = 10.0f;
    public float scanFrequence = 2.0f;
    public float scanSpeed = 15.0f;
    public float scanWidth = 2.0f;

    public PostProcessVolume pousseeVolume;

    protected Coroutine gripCoroutine = null;
    protected Coroutine hitCoroutine1 = null;
    protected Coroutine hitCoroutine2 = null;
    protected GameManager gm;
    protected new Camera camera;

    public void Initialize() {
        gm = GameManager.Instance;
        camera = gm.player.camera;

        ScanInitialize();
    }

    public void Update() {
        // On fait tourner la skybox !
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyboxRotationSpeed);

        OnlyRenderScannableCubes();
    }

    protected void ScanInitialize() {
        if (!useScan)
            return;
        foreach(Cube cube in gm.map.GetAllCubes()) {
            cube.gameObject.SetActive(false);
        }
    }

    protected void OnlyRenderScannableCubes() {
        if (!useScan)
            return;

        // On active ce qui est proche du joueur
        Vector3 center = gm.player.transform.position;
        foreach(Cube cube in gm.map.GetCubesInSphere(center, scanRange * 2)) { // * 2 pour être sur que la vitesse ne nous carotte pas x)
            cube.gameObject.SetActive(Vector3.Distance(center, cube.transform.position) <= scanRange);
        }

        // Puis on applique les scans !
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
        hitVolume.profile.GetSetting<Vignette>().intensity.Override(intensityHit);
        if (hitCoroutine1 != null) {
            StopCoroutine(hitCoroutine1);
            StopCoroutine(hitCoroutine2);
        }
        hitCoroutine2 = StartCoroutine(CUpdateHitEffect());
    }

    IEnumerator CUpdateHitEffect() {
        hitCoroutine1 = StartCoroutine(SetVignette(0.0f, changeTimeHit, hitVolume));
        yield return new WaitForSeconds(changeTimeHit);
        hitVolume.enabled = false;
    }

    IEnumerator SetVignette(float targetValue, float changeTime, PostProcessVolume volumeVignette) {
        Vignette vignette = volumeVignette.profile.GetSetting<Vignette>();
        float debut = Time.timeSinceLevelLoad;
        float current = Time.timeSinceLevelLoad;
        float amountNeeded = targetValue - vignette.intensity;

        while(Time.timeSinceLevelLoad - debut < changeTime) {
            float percentToAdd = (Time.timeSinceLevelLoad - current) / changeTime;
            float newValue = vignette.intensity + percentToAdd * amountNeeded;
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
