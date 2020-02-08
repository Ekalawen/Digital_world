using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumiere : MonoBehaviour {

    public enum LumiereType { NORMAL, FINAL };

    public LumiereType type;
    public float timeBonus = 10.0f;

    protected GameManager gm;

    protected virtual void Start () {
        //ParticleSystem ps = GetComponent<ParticleSystem> ();
        gm = FindObjectOfType<GameManager>();
	}

	// Si le joueur nous touche, on disparait !
	protected virtual void OnTriggerEnter(Collider hit) {
		if (hit.gameObject.name == "Joueur"){
            gm.map.lumieres.Remove(this);
			Destroy (this.gameObject);

			// Et on certifie qu'on a bien attrapé une orbe
            gm.console.AttraperLumiere(gm.map.lumieres.Count);

			gm.console.UpdateLastLumiereAttrapee();

            OnTriggerEnterSpecific();

            gm.eventManager.OnLumiereCaptured(type);

            gm.soundManager.PlayGetLumiereClip(transform.position);

            gm.timerManager.AddTime(timeBonus);
		}
	}

    protected virtual void OnTriggerEnterSpecific() {
    }
}
