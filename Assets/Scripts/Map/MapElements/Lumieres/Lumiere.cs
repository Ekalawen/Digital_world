using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumiere : MonoBehaviour {

    public enum LumiereType { NORMAL, FINAL, SPECIAL };

    public LumiereType type;
    public float timeBonus = 10.0f;

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

        // Et on certifie qu'on a bien attrapé une orbe
        gm.console.AttraperLumiere(gm.map.GetLumieres().Count);

        gm.console.UpdateLastLumiereAttrapee();

        CapturedSpecific();

        gm.eventManager.OnLumiereCaptured(type);

        gm.soundManager.PlayGetLumiereClip(transform.position);

        gm.timerManager.AddTime(timeBonus);
    }

    protected virtual void CapturedSpecific() {
    }
}
