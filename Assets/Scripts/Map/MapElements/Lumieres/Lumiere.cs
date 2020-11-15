using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumiere : MonoBehaviour {

    public enum LumiereType { NORMAL, FINAL, SPECIAL };

    public LumiereType type;
    public float timeBonus = 10.0f;
    public GameObject rewardLumierePrefab;

    [Header("ScreenShake")]
    public float screenShakeMagnitude = 10;
    public float screenShakeRoughness = 10;

    protected GameManager gm;

    protected virtual void Start () {
        //ParticleSystem ps = GetComponent<ParticleSystem> ();
        gm = GameManager.Instance;
	}

	// Si le joueur nous touche, on disparait !
	protected virtual void OnTriggerEnter(Collider hit) {
		if (hit.gameObject.name == "Joueur"){
            Captured();
		}
	}

    protected void Captured() {
        gm.map.RemoveLumiere(this);
        Destroy (this.gameObject);

        gm.console.UpdateLastLumiereAttrapee();

        CapturedSpecific();

        gm.eventManager.OnLumiereCaptured(type);

        NotifyConsoleLumiereCatpure();

        gm.soundManager.PlayGetLumiereClip(transform.position);

        ScreenShakeOnLumiereCapture();

        gm.timerManager.AddTime(timeBonus);
    }

    protected void ScreenShakeOnLumiereCapture() {
        CameraShaker.Instance.ShakeOnce(screenShakeMagnitude, screenShakeRoughness, 0.05f, 0.05f);
    }

    protected virtual void NotifyConsoleLumiereCatpure() {
        gm.console.AttraperLumiere(gm.map.GetLumieres().Count);
    }

    protected virtual void CapturedSpecific() {
    }
}
