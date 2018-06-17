using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectifScript : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	[HideInInspector]
	public ConsoleScript console;
	[HideInInspector]
	public MapManagerScript mapManager;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// Initialisation
		ParticleSystem ps = GetComponent<ParticleSystem> ();
		console = GameObject.Find("Console").GetComponent<ConsoleScript>();
		mapManager = GameObject.Find("MapManager").GetComponent<MapManagerScript>();
	}

	// Si le joueur nous touche, on disparait !
	void OnTriggerEnter(Collider hit) {
		if (hit.gameObject.name == "Joueur"){
			int nbLumieresRestantes = mapManager.nbLumieres--;
			nbLumieresRestantes--;
			console.attraperLumiere(nbLumieresRestantes);
			Destroy (this.gameObject);

			// Et on certifie qu'on a bien attrapé une orbe
			console.updateLastOrbeAttrapee();
		}
	}
}
