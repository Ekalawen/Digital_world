using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PersonnageScript : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ENUMERATION
	//////////////////////////////////////////////////////////////////////////////////////

	public enum EtatPersonnage {AU_SOL, EN_SAUT, EN_CHUTE, AU_MUR, AU_POTEAU};

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public float vitesseDeplacement; // la vitesse de déplacement horizontale
	public float vitesseSaut; // la vitesse d'élévation du saut
	public float dureeSaut; // la durée totale d'un saut
	public float dureeEfficaciteSaut; // le pourcentage de temps où le saut nous surélève
	public float gravite; // la force de la gravité
	public float sensibilite; // la sensibilité de la souris
	public float dureeMur; // le temps que l'on peut rester accroché au mur
	public float distanceMurMax; // la distance maximale de laquelle on peut s'éloigner du mur
	public GameObject trail; // Les trails à tracer quand le personnage est perdu !

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	[HideInInspector]
	public GameObject personnage;
	[HideInInspector]
	public CharacterController controller;
	[HideInInspector]
	public ConsoleScript console;
	private Camera camera;
	private float xRot, yRot;
	private float currentRotationX, currentRotationY;
	private float xRotV, yRotV;
	private float lookSmoothDamp = 0.0f;

	private EtatPersonnage etat; // l'état du personnage
	private float debutSaut; // le timing où le personnage a débuté son dernier saut !
	private Vector3 pointDebutSaut; // le point de départ du saut !
	private EtatPersonnage origineSaut; // Permet de savoir depuis où (le sol ou un mur) le personnage a sauté !
	private Vector3 normaleOrigineSaut; // La normale au plan du mur duquel le personnage a sauté
	private float debutMur; // le timing où le personnage a commencé à s'accrocher au mur !
	private Vector3 normaleMur; // la normale au mur sur lequel le personnage est accroché !
	private Vector3 pointMur; // un point du mur sur lequel le personnage est accroché ! En effet, la normale ne suffit pas :p

	private Vector3 pousee; // Lorsque le personnage est poussé
	private float debutPousee; // Le début de la poussée
	private float tempsPousee; // Le temps pendant lequel le personnage reçoit cette poussée

	[HideInInspector]
	public float lastNotContactEnnemy; // Le dernier temps où il ne touchait pas d'ennemi, utilisé pour la fin du jeu
	[HideInInspector]
	public bool vu = false; // permet de savoir si le joueur est vu par des ennemis ou pas

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// On récupère le personnage
		personnage = GameObject.Find("Joueur");
		controller = personnage.GetComponent<CharacterController> ();
		camera = personnage.transform.GetChild(0).GetComponent<Camera>() as Camera;
		pointDebutSaut = transform.position;

		// On regarde en bas quand on commence !
		xRot = 180; 
		yRot = 0;

		// On empêche la souris de sortir de l'écran !
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update () {
		// On oriente notre personnage dans dans le sens de la souris
		xRot -= Input.GetAxisRaw ("Mouse Y") * sensibilite; // mouvement caméra haut-bad
		yRot += Input.GetAxisRaw ("Mouse X") * sensibilite; // mouvement personnage axe 
		xRot = Mathf.Clamp(xRot, -90, 90);
		currentRotationX = Mathf.SmoothDamp (currentRotationX, xRot, ref xRotV, lookSmoothDamp);
		currentRotationY = Mathf.SmoothDamp (currentRotationY, yRot, ref yRotV, lookSmoothDamp);
		camera.transform.rotation = Quaternion.Euler (currentRotationX, currentRotationY, 0);// mouvement caméra haut-bas

		// On récupère le mouvement dans le sens de l'orientation du personnage
		Vector3 move = Vector3.zero;
		move = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
		camera.transform.rotation = Quaternion.Euler (0, currentRotationY, 0);// mouvement caméra haut-bas
		move = camera.transform.TransformDirection (move);
		camera.transform.rotation = Quaternion.Euler (currentRotationX, currentRotationY, 0);// mouvement caméra haut-bas

		// On applique la vitesse au déplacement
		move *= vitesseDeplacement;

		// On applique la poussee si le personnage en a une !
		if (Time.timeSinceLevelLoad - debutPousee < tempsPousee) {
			move += pousee;
		}

		// On retient l'état d'avant
		EtatPersonnage etatAvant = etat;

		// On trouve l'état du personnage
		getEtatPersonnage();

		// Si on a fait un grand saut, on le dit
		detecterGrandSaut(etatAvant);

		// En fonction de l'état du personnage, on applique le mouvement correspondant !
		switch (etat) {
		case EtatPersonnage.AU_SOL:
			if (Input.GetButton ("Jump")) {
				etat = EtatPersonnage.EN_SAUT;
				debutSaut = Time.timeSinceLevelLoad;
				pointDebutSaut = transform.position;
				origineSaut = EtatPersonnage.AU_SOL;
				StartCoroutine (stopJump (debutSaut));
			} else {
				// Petit débuggage pour empêcher l'alternance entre AU_SOL et EN_CHUTE !
				move.y -= gravite * Time.deltaTime;
			}
			break;

		case EtatPersonnage.EN_SAUT:
			float percentSaut = (Time.timeSinceLevelLoad - debutSaut) / dureeSaut;
			if (percentSaut <= dureeEfficaciteSaut) {
				move.y += vitesseSaut;
			}
			break;

		case EtatPersonnage.EN_CHUTE:
			move.y -= gravite * Time.deltaTime;
			break;

		case EtatPersonnage.AU_MUR:
			// On peut se décrocher du mur en appuyant sur shift
			if (Input.GetKey (KeyCode.LeftShift)) {
				etat = EtatPersonnage.EN_CHUTE;
				pointDebutSaut = transform.position;
				origineSaut = EtatPersonnage.AU_SOL;
				normaleOrigineSaut = normaleMur;
			// On peut encore sauter quand on est au mur ! 
			} else if (Input.GetButtonDown ("Jump")) { // Mais il faut appuyer à nouveau !
				etat = EtatPersonnage.EN_SAUT;
				debutSaut = Time.timeSinceLevelLoad;
				pointDebutSaut = transform.position;
				origineSaut = EtatPersonnage.AU_MUR;
				normaleOrigineSaut = normaleMur;
				StartCoroutine (stopJump (debutSaut));
			} else if (Input.GetButton ("Jump")) { // On a le droit de terminer son saut lorsqu'on touche un mur
				float pourcentageSaut = (Time.timeSinceLevelLoad - debutSaut) / dureeSaut;
				if (pourcentageSaut <= dureeEfficaciteSaut) {
					move.y += vitesseSaut;
				}
			}
			// Si ça fait trop longtemps qu'on est sur le mur
			// Ou que l'on s'éloigne trop du mur on tombe
			float distanceMur = ((transform.position - pointMur) - Vector3.ProjectOnPlane ((transform.position - pointMur), normaleMur)).magnitude; // pourtant c'est clair non ? Fais un dessins si tu comprends pas <3
			if ((Time.timeSinceLevelLoad - debutMur) >= dureeMur
			    || distanceMur >= distanceMurMax) {
				etat = EtatPersonnage.EN_CHUTE;
				pointDebutSaut = transform.position;
				origineSaut = EtatPersonnage.AU_MUR;
				normaleOrigineSaut = normaleMur;
			}
			// Et on veut aussi vérifier que le mur continue encore à nos cotés !
			// Pour ça on va lancer un rayon ! <3
			Ray ray = new Ray (transform.position, -normaleMur);
			RaycastHit hit; // là où l'on récupère l'information du ray !
			if (!Physics.Raycast (ray, out hit, distanceMurMax) || hit.collider.tag != "Cube") {
				// En fait il faudrait passer en mode AU_POTEAU ici !
				etat = EtatPersonnage.EN_CHUTE;
				pointDebutSaut = transform.position;
				origineSaut = EtatPersonnage.AU_MUR;
				normaleOrigineSaut = normaleMur;
			}

			break;
		case EtatPersonnage.AU_POTEAU:
			// C'est possible de rajouter ça quand on voudra =)
			break;
		}

		controller.Move (move * Time.deltaTime);

		// On vérifie qu'il n'est pas en contact avec un ennemy !
		Collider[] colliders = Physics.OverlapSphere (transform.position, 1f);
		bool pasTouchee = true;
		foreach (Collider collider in colliders) {
			if (collider.tag == "Ennemi") {
				pasTouchee = false;
			}
		}
		if (pasTouchee) {
			lastNotContactEnnemy = Time.timeSinceLevelLoad;
		}


		// On regarde si le joueur a appuyé sur E
		if (Input.GetKeyDown (KeyCode.E)) {
			// On trace les rayons ! =)
			GameObject[] lumieres = GameObject.FindGameObjectsWithTag ("Objectif");
			for (int i = 0; i < lumieres.Length; i++) {
				//Vector3 departRayons = transform.position - 0.5f * camera.transform.forward + 0.5f * Vector3.up;
				Vector3 departRayons = transform.position + 0.5f * Vector3.up;
				GameObject tr = Instantiate (trail, departRayons, Quaternion.identity) as GameObject;
				tr.GetComponent<TrailScript> ().setTarget (lumieres [i].transform.position);
			}

			// Un petit message
			console.envoyerTrails();

			// Et on certifie qu'on a appuyé sur E
			console.updateLastOrbeAttrapee();
		}
	}

	// Pour mettre à jour l'état du personnage !
	void getEtatPersonnage() {
		if (etat != EtatPersonnage.AU_MUR) {
			if (controller.isGrounded) {
				//if((controller.collisionFlags & CollisionFlags.Below) != 0) {
				etat = EtatPersonnage.AU_SOL;
			} else {
				if (etat != EtatPersonnage.EN_SAUT) {
					etat = EtatPersonnage.EN_CHUTE;
				}
			}
		}
	}

	IEnumerator stopJump(float debut) {
		while (Time.timeSinceLevelLoad - debut < dureeSaut && Input.GetButton ("Jump")) {
			yield return null;
		}
		if (etat != EtatPersonnage.AU_MUR) {
			etat = EtatPersonnage.EN_CHUTE;
			// On choisit de ne pas changer le point de debut volontairement !
			// Sinon ça écraserait le point de départ du saut !
			//pointDebutSaut = transform.position;
		}
	}

	void detecterGrandSaut(EtatPersonnage etatAvant) {
		if((etatAvant == EtatPersonnage.EN_CHUTE || etatAvant == EtatPersonnage.EN_SAUT)
		&& (etat == EtatPersonnage.AU_SOL || etat == EtatPersonnage.AU_MUR)) {
			float hauteurSaut = pointDebutSaut.y - transform.position.y;
			if(hauteurSaut > 7) {
				console.grandSaut(hauteurSaut);
			}
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {

		// On regarde si le personnage s'accroche à un mur !
		// Pour ça il doit être dans les airs !
		// Et il ne doit PAS être en train d'appuyer sur SHIFT
		if ((etat == EtatPersonnage.EN_SAUT || etat == EtatPersonnage.EN_CHUTE) && !Input.GetKey(KeyCode.LeftShift)) {

			/*// On ne peut pas s'aggriper si on a une distance horizontale trop petite !
			Vector3 pointDepart = pointDebutSaut;
			Vector3 pointDepartProject = Vector3.ProjectOnPlane (pointDepart, Vector3.up);
			if(true) {//Vector3.Distance(pointDepartProject, Vector3.ProjectOnPlane(hit.point, Vector3.up)) >= 0.5f) {*/

				// Si on vient d'un mur, on vérifie que la normale du mur précédent est suffisamment différente de la normale actuelle !
				Vector3 n = hit.normal;
				if(origineSaut == EtatPersonnage.AU_SOL
					|| (origineSaut == EtatPersonnage.AU_MUR && Vector3.Angle(normaleOrigineSaut, n) > 10)) {

					// Si la normale est au moins un peu à l'horizontale !
					Vector3 nProject = Vector3.ProjectOnPlane (n, Vector3.up);
					if (Mathf.Abs (Vector3.Angle (n, nProject)) < 45f) {

						/*// Si on détecte une collision avec un mur, on peut s'aggriper si l'angle entre la normale
						// au mur et le vecteur (pointDepartSaut/mur) est inférieur à 45°
						Vector3 direction = pointDebutSaut - hit.point;
						Vector3 directionProject = Vector3.ProjectOnPlane(direction, Vector3.up);
						if (Mathf.Abs (Vector3.Angle (nProject, directionProject)) < 45f) {*/
							etat = EtatPersonnage.AU_MUR; // YEAH !!!
							debutMur = Time.timeSinceLevelLoad;
							normaleMur = n;
							pointMur = hit.point;
					}
				}
		}
	}

	public void etrePoussee(Vector3 directionPoussee, float tempsDeLaPousee) {
		pousee = directionPoussee;
		tempsPousee = tempsDeLaPousee;
		debutPousee = Time.timeSinceLevelLoad;
	}
}

