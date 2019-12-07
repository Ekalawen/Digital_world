using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumiere : MonoBehaviour {

    public enum LumiereType { NORMAL, FINAL };

    public LumiereType type;

    protected GameManager gm;
    protected AudioSource source;

    protected void Start () {
        //ParticleSystem ps = GetComponent<ParticleSystem> ();
        gm = FindObjectOfType<GameManager>();
        source = GetComponentInChildren<AudioSource>();
	}

	// Si le joueur nous touche, on disparait !
	protected virtual void OnTriggerEnter(Collider hit) {
		if (hit.gameObject.name == "Joueur"){
            gm.map.lumieres.Remove(this);
            gm.console.AttraperLumiere(gm.map.lumieres.Count);
			Destroy (this.gameObject);

			// Et on certifie qu'on a bien attrapé une orbe
			gm.console.UpdateLastLumiereAttrapee();

            gm.eventManager.OnLumiereCaptured();

            gm.soundManager.PlayGetLumiereClip(transform.position);
		}
	}
}
