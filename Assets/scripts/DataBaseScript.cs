using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Le but de la DataBase est de gérer le comportement de tout ce qui est entrave le joueur.
// Cela va de la coordination des Drones, à la génération d'évenements néffastes.
public class DataBaseScript : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	[HideInInspector]
	public EnnemiScript[] drones; // Elle connait tous les drones !
	[HideInInspector]
	public EnnemiScript.EtatEnnemi etatDrones; // Permet de donner des ordres aux drones
	[HideInInspector]
	public GameObject player; // Le joueur
	[HideInInspector]
	public ConsoleScript console; // La console
	[HideInInspector]
	public MapManagerScript mapManager; // la map
	private bool plusDeLumieres;
	private float timingPlusDeLumieres;
	private bool isJoueurSuivi; // Permet de savoir si le joueur est suivi

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// Initialisation
		name = "DataBase";
		GameObject[] go = GameObject.FindGameObjectsWithTag ("Ennemi");
		drones = new EnnemiScript[go.Length];
		for (int i = 0; i < go.Length; i++) {
			drones [i] = go [i].GetComponent<EnnemiScript> ();
		}
		player = GameObject.Find ("Joueur");
		mapManager = GameObject.Find("MapManager").GetComponent<MapManagerScript>();
		plusDeLumieres = false;
		isJoueurSuivi = false;
	}
	
	void Update () {
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

		// On regarde si il reste des lumières
		if (mapManager.nbLumieres <= 0) {
			if (!plusDeLumieres) {
				plusDeLumieres = true;
				timingPlusDeLumieres = Time.timeSinceLevelLoad;
			}

			if (joueurVisible && Time.timeSinceLevelLoad - timingPlusDeLumieres > 5f) {
				etatDrones = EnnemiScript.EtatEnnemi.RUSHING;
			} else {
				etatDrones = EnnemiScript.EtatEnnemi.DEFENDING;
			}
		}

		// Si le joueur était visible avant mais qu'on le perd de vu, alors on le signal ! =)
		Debug.Log("suivi = " + isJoueurSuivi);
		if (Time.timeSinceLevelLoad >= 10 && isJoueurSuivi && !joueurSuivi()) {
			console.semerSondes();
			isJoueurSuivi = false;
		}
		if (Time.timeSinceLevelLoad >= 10 && !isJoueurSuivi && joueurSuivi()) {
			console.joueurRepere();
			isJoueurSuivi = true;
		}
	}

	public EnnemiScript.EtatEnnemi demanderOrdre() {
		return etatDrones;
	}

	// Permet de savoir si le joueur est actuellement suivi
	public bool joueurSuivi() {
		bool suivi = false;
		foreach (EnnemiScript drone in drones) {
			if (drone.isMoving()) {
				suivi = true;
				break;
			}
		}
		return suivi;
	}
}
