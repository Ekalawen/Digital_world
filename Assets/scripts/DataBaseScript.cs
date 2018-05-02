using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBaseScript : MonoBehaviour {

	private EnnemiScript[] drones; // Elle connait tous les drones !
	private EnnemiScript.EtatEnnemi etatDrones; // Permet de donner des ordres aux drones
	private GameObject player; // Le joueur
	private bool plusDeLumieres;
	private float timingPlusDeLumieres;

	// Use this for initialization
	void Start () {
		name = "DataBase";
		GameObject[] go = GameObject.FindGameObjectsWithTag ("Ennemi");
		drones = new EnnemiScript[go.Length];
		for (int i = 0; i < go.Length; i++) {
			drones [i] = go [i].GetComponent<EnnemiScript> ();
		}
		player = GameObject.Find ("Joueur");
		plusDeLumieres = false;
	}
	
	// Update is called once per frame
	void Update () {
		// On regarde si il reste des lumières
		if (GameObject.Find ("GameManager").GetComponent<GamaManagerScript> ().nbLumieres <= 0) {
			Debug.Log ("Database : nbLumières = " + GameObject.Find ("GameManager").GetComponent<GamaManagerScript> ().nbLumieres);
			if (!plusDeLumieres) {
				plusDeLumieres = true;
				timingPlusDeLumieres = Time.time;
			}
			// Alors on décide de la stratégie à adopter pour tous les drones !
			// Il suffit qu'un seul drône puisse voir le joueur pour que TOUS le trackent !
			bool joueurVisible = false;
			Ray ray;
			RaycastHit hit;
			foreach (EnnemiScript es in drones) {
				ray = new Ray (es.gameObject.transform.position, player.transform.position - es.gameObject.transform.position);
				if (Physics.Raycast (ray, out hit, es.distanceDeDetection * es.coefficiantDeRushDistanceDeDetection) && hit.collider.name == "Joueur") {
					// VU
					joueurVisible = true;
					break;
				}
			}
			if (joueurVisible && Time.time - timingPlusDeLumieres > 5f) {
				etatDrones = EnnemiScript.EtatEnnemi.RUSHING;
			} else {
				etatDrones = EnnemiScript.EtatEnnemi.DEFENDING;
			}
		}
	}

	public EnnemiScript.EtatEnnemi demanderOrdre() {
		return etatDrones;
	}
}
