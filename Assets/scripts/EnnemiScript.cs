using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemiScript : MonoBehaviour {

	public enum EtatEnnemi {WAITING, TRACKING, DEFENDING, RUSHING};

	public float vitesseMin; // On veut une vitesse aléatoire comprise
	public float vitesseMax; // entre min et max !
	public float puissancePoussee; // La force avec laquelle on va pousser le joueur en cas de contact !
	public float tempsPousee; // Le temps pendant lequel le personnage est poussé !
	public float distanceDeDetection; // La distance à partir de laquelle le probe peut pourchasser l'ennemi
	public float coefficiantDeRushVitesse; // Le multiplicateur de vitesse lorsque les drones rushs
	public float coefficiantDeRushDistanceDeDetection; // Le multiplicateur de la portée de détection quand on est en rush

	private GameObject player;
	private Vector3 lastPositionSeen; // La dernière position à laquelle le joueur a été vu !
	private EtatEnnemi etat;
	private CharacterController controller;
	private DataBaseScript dataBase; // La dataBase qui envoie les ordres
	private float vitesse;

	// Use this for initialization
	void Start () {
		name = "Sonde_" + Random.Range (0, 9999);
		player = GameObject.Find ("Joueur");
		dataBase = GameObject.Find ("DataBase").GetComponent<DataBaseScript> ();
		lastPositionSeen = transform.position;
		etat = EtatEnnemi.WAITING;
		vitesse = Mathf.Exp(Random.Range (Mathf.Log(vitesseMin), Mathf.Log(vitesseMax)));
		controller = this.GetComponent<CharacterController> ();
	}
	
	// Update is called once per frame
	void Update () {
		ConsoleScript cs = GameObject.Find ("Console").GetComponent<ConsoleScript> ();

		// On regarde si il reste des lumières
		// Si il n'en reste plus, on adopte une stratégie défensive pour empécher le joueur de sortir !
		if (GameObject.Find ("GameManager").GetComponent<GamaManagerScript> ().nbLumieres <= 0) {
			Debug.Log ("nb lumières = " + GameObject.Find ("GameManager").GetComponent<GamaManagerScript> ().nbLumieres);

			// On se réfère à une autre entité qui décidera du mouvement COMMUN de tous les drones !
			etat = dataBase.demanderOrdre();

		} else {
			// Si l'ennemie est suffisament proche et qu'il est visible ! Et que le jeu a commencé depuis au moins 5 secondes
			Ray ray = new Ray (transform.position, player.transform.position - transform.position);
			RaycastHit hit;
			if (Time.time >= 10f && Physics.Raycast (ray, out hit, distanceDeDetection) && hit.collider.name == "Joueur") {
				// Si la sonde vient juste de le repérer
				if (etat == EtatEnnemi.WAITING) {
					// On l'anonce !
					cs.ajouterMessage (name + " vous a détecté, je sais où vous êtes.", ConsoleScript.TypeText.ENNEMI_TEXT);
					player.GetComponent<personnageScript> ().vu = true;
				}
				etat = EtatEnnemi.TRACKING;

			} else {
				// Si la sonde vient juste de perdre sa trace
				if (etat == EtatEnnemi.TRACKING) {
					// On l'anonce !
					cs.ajouterMessage ("On a déconnecté " + name + ".", ConsoleScript.TypeText.ALLY_TEXT);
				}
				etat = EtatEnnemi.WAITING;
			}
		}

		// Tant que l'ennemi n'est pas visible et/ou trop proche, on reste sur place
		// Sinon on le pourchasse !
		Vector3 direction;
		switch (etat) {
		case EtatEnnemi.WAITING:
			// On va là où on a vu le joueur pour la dernière fois !
			direction = lastPositionSeen - transform.position;
			direction.Normalize ();

			// Si c'est trop long, on ajuste
			if (Vector3.Magnitude(direction * vitesse * Time.deltaTime) > Vector3.Magnitude(lastPositionSeen - transform.position)) {
				controller.Move (lastPositionSeen - transform.position);
			} else {
				controller.Move (direction * vitesse * Time.deltaTime);
			}
			break;

		case EtatEnnemi.TRACKING:
			// Les ennemis se déplacent toujours vers le joueur de façon tout à fait linéaire !		
			direction = player.transform.position - transform.position;
			direction.Normalize ();

			controller.Move (direction* vitesse * Time.deltaTime);
			// Et on retient la position actuelle du joueur !
			lastPositionSeen = player.transform.position;
			break;

		case EtatEnnemi.DEFENDING:
			// On va tout en haut pour empêcher le joueur de sortir !
			Vector3 cible = transform.position;
			cible.y = GameObject.Find ("GameManager").GetComponent<GamaManagerScript> ().tailleMap + 1;
			direction = cible;
			direction.Normalize ();
			direction *= coefficiantDeRushVitesse;

			// Si c'est trop long, on ajuste
			if (Vector3.Magnitude(direction * vitesse * Time.deltaTime) > Vector3.Magnitude(cible - transform.position)) {
				controller.Move (cible - transform.position);
			} else {
				controller.Move (direction * vitesse * Time.deltaTime);
			}
			break;

		case EtatEnnemi.RUSHING:
			// Les ennemis se déplacent toujours vers le joueur de façon tout à fait linéaire !		
			direction = player.transform.position - transform.position;
			direction.Normalize ();
			direction*= coefficiantDeRushVitesse;

			controller.Move (direction* vitesse * Time.deltaTime);
			// Et on retient la position actuelle du joueur !
			lastPositionSeen = player.transform.position;
			break;
		}
	}

	// Permet de savoir so me drpone est en mouvement
	public bool isMoving() {
		return (etat == EtatEnnemi.TRACKING || etat == EtatEnnemi.RUSHING) || Vector3.Distance(lastPositionSeen, transform.position) > 1f;
	}

	// On vérifie si on a touché le joueur !!!
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.collider.name == "Joueur") {
			// Si c'est le cas, on l'envoie ballader !
			Vector3 directionPoussee = hit.collider.transform.position - transform.position;
			directionPoussee.Normalize ();
			hit.collider.GetComponent<personnageScript> ().etrePoussee (directionPoussee * puissancePoussee, tempsPousee);

			// Et on affiche un message dans la console !
			if (!GameObject.Find ("GameManager").GetComponent<GamaManagerScript> ().partieDejaTerminee) {
				ConsoleScript cs = GameObject.Find ("Console").GetComponent<ConsoleScript> ();
				cs.ajouterMessageImportant ("TOUCHÉ ! Je vais vous avoir !", ConsoleScript.TypeText.ENNEMI_TEXT, 1);
			}
		}
	}
		

}
