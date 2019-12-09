using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManager : MonoBehaviour {

    public float changeTimeGrip = 0.075f;
    public float intensityGrip = 0.315f;
    public PostProcessVolume gripVolume;

    public float changeTimeHit = 0.6f;
    public float intensityHit= 0.4f;
    public PostProcessVolume hitVolume;

    protected Coroutine gripCoroutine = null;
    protected Coroutine hitCoroutine1 = null;
    protected Coroutine hitCoroutine2 = null;
    protected GameManager gm;
    protected new Camera camera;

    public void Initialize() {
        gm = GameManager.Instance;
        camera = gm.player.camera;
    }

    public void UpdateGripEffect(Player.EtatPersonnage previousState) {
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
    }

}
