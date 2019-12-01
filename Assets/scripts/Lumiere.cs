using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumiere : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	[HideInInspector]
	public Console console;
	[HideInInspector]
	public MapManager mapManager;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// Initialisation
		ParticleSystem ps = GetComponent<ParticleSystem> ();
		console = GameObject.Find("Console").GetComponent<Console>();
		mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
	}

	// Si le joueur nous touche, on disparait !
	void OnTriggerEnter(Collider hit) {
		if (hit.gameObject.name == "Joueur"){
			mapManager.nbLumieres--;
			console.AttraperLumiere(mapManager.nbLumieres);
			Destroy (this.gameObject);

			// Et on certifie qu'on a bien attrapé une orbe
			console.UpdateLastLumiereAttrapee();
		}
	}
}
