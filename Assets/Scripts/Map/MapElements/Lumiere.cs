using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumiere : MonoBehaviour {

	private Console console;
    private MapManager map;

	protected void Start () {
		//ParticleSystem ps = GetComponent<ParticleSystem> ();
		console = GameObject.Find("Console").GetComponent<Console>();
        map = FindObjectOfType<MapManager>();
	}

	// Si le joueur nous touche, on disparait !
	void OnTriggerEnter(Collider hit) {
		if (hit.gameObject.name == "Joueur"){
            map.lumieres.Remove(this);
            console.AttraperLumiere(map.lumieres.Count);
			Destroy (this.gameObject);

			// Et on certifie qu'on a bien attrapé une orbe
			console.UpdateLastLumiereAttrapee();
		}
	}
}
