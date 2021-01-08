using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Lumiere : MonoBehaviour {

    public enum LumiereType {
        NORMAL,
        FINAL,
        ALMOST_FINAL,
        SPECIAL,
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
    public float dureeDestruction = 1.0f;
    public VisualEffect vfx;
    public GameObject pointLight;

    protected GameManager gm;
    protected bool isCaptured = false;

    protected virtual void Start () {
        gm = GameManager.Instance;
	}

	protected virtual void OnTriggerEnter(Collider hit) {
        if (hit.gameObject.name == "Joueur"){
            Captured();
		}
	}

    protected void Captured()
    {
        if (isCaptured)
        {
            return;
        }
        isCaptured = true;

        gm.map.RemoveLumiere(this);

        AutoDestroy();

        CapturedSpecific();

        NotifyEventManagerLumiereCapture();

        NotifyConsoleLumiereCatpure();

        gm.soundManager.PlayGetLumiereClip(transform.position);

        ScreenShakeOnLumiereCapture();

        gm.timerManager.AddTime(timeBonus);
    }

    private void AutoDestroy() {
        Destroy(this.gameObject, dureeDestruction);
        DestroyAnimation();
    }

    private void DestroyAnimation() {
        float turbulenceAttractionSpeed = vfx.GetFloat("EnveloppeSphereAttractionSpeed");
        vfx.SetFloat("EnveloppeSphereAttractionSpeed", -turbulenceAttractionSpeed);
        vfx.SetFloat("EnveloppeSpawnRate", 0);
        vfx.SetVector4("BeamColor", Vector4.zero);
        Destroy(pointLight);
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
}
