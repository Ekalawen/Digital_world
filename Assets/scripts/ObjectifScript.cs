using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectifScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		ParticleSystem ps = GetComponent<ParticleSystem> ();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Si le joueur nous touche, on disparait !
	void OnTriggerEnter(Collider hit) {
		ConsoleScript cs = GameObject.Find ("Console").GetComponent<ConsoleScript> ();
		if (hit.gameObject.name == "Joueur"){
			cs.ajouterMessage ("ON A DES INFOS !", ConsoleScript.TypeText.ALLY_TEXT);
			int nbLumieresRestantes = GameObject.Find ("GameManager").GetComponent<GamaManagerScript> ().nbLumieres--;
			nbLumieresRestantes--;
			if (nbLumieresRestantes > 0) {
				cs.ajouterMessageImportant ("Plus que " + nbLumieresRestantes + " !", ConsoleScript.TypeText.ALLY_TEXT, 2);
			} else {
				cs.ajouterMessage ("ON LES A TOUTES !", ConsoleScript.TypeText.ALLY_TEXT);
				cs.ajouterMessageImportant ("FAUT SE BARRER MAINTENANT !!!", ConsoleScript.TypeText.ALLY_TEXT, 2);
			}
			Destroy (this.gameObject);

			// Et on certifie qu'on a bien attrapé une orbe
			cs.updateLastOrbeAttrapee();
		}
	}
}
