using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class personnageScript : MonoBehaviour {

	public enum EtatPersonnage {AU_SOL, EN_SAUT, EN_CHUTE, AU_MUR, AU_POTEAU};

	public float vitesseDeplacement; // la vitesse de déplacement horizontale
	public float vitesseSaut; // la vitesse d'élévation du saut
	public float dureeSaut; // la durée totale d'un saut
	public float dureeEfficaciteSaut; // le pourcentage de temps où le saut nous surélève
	public float gravite; // la force de la gravité
	public float sensibilite; // la sensibilité de la souris

	private GameObject personnage;
	private CharacterController controller;
	private Camera camera;
	private float xRot, yRot;
	private float currentRotationX, currentRotationY;
	private float xRotV, yRotV;
	private float lookSmoothDamp = 0.0f;

	private EtatPersonnage etat; // l'état du personnage
	private float debutSaut; // le timing où le personnage a débuté son dernier saut !
	private Vector3 pointDebutSaut; // le point de départ du saut !

	// Use this for initialization
	void Start () {
		// On positionne la souris au centre de l'écran !
		xRot -= Input.mousePosition.y;
		yRot = Input.mousePosition.x;

		// On empêche la souris de sortir de l'écran !
		//Cursor.lockState = CursorLockMode.Locked;
		//Cursor.visible = false;

		// On récupère le personnage
		personnage = GameObject.Find("Joueur");
		controller = personnage.GetComponent<CharacterController> ();
		camera = personnage.transform.GetChild(0).GetComponent<Camera>() as Camera;
	}

	// Update is called once per frame
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

		// On trouve l'état du personnage
		if (etat != EtatPersonnage.AU_MUR && controller.isGrounded) {
			etat = EtatPersonnage.AU_SOL;
		} else {
			if (etat != EtatPersonnage.EN_SAUT) {
				etat = EtatPersonnage.EN_CHUTE;
			}
		}

		Debug.Log ("Etat = " + etat);

		// En fonction de l'état du personnage, on applique le mouvement correspondant !
		switch (etat) {
		case EtatPersonnage.AU_SOL:
			if (Input.GetButton ("Jump")) {
				etat = EtatPersonnage.EN_SAUT;
				debutSaut = Time.time;
				pointDebutSaut = transform.position;
				StartCoroutine (stopJump (debutSaut));
			}
			break;
		case EtatPersonnage.EN_SAUT:
			float percentSaut = (Time.time - debutSaut) / dureeSaut;
			if (percentSaut <= dureeEfficaciteSaut) {
				move.y = vitesseSaut;
			}

			break;
		case EtatPersonnage.EN_CHUTE:
			move.y -= gravite * Time.deltaTime;
			break;
		case EtatPersonnage.AU_MUR:
			// On peut encore sauter quand on est au mur ! 
			Debug.Log("On est au mur !");
			/*if (Input.GetButton ("Jump")) {
				etat = EtatPersonnage.EN_SAUT;
				debutSaut = Time.time;
				pointDebutSaut = transform.position;
				StartCoroutine (stopJump (debutSaut));
			}*/
			break;
		case EtatPersonnage.AU_POTEAU:
			break;
		}

		controller.Move (move * Time.deltaTime);
	}

	IEnumerator stopJump(float debut) {
		while (Time.time - debut < dureeSaut && Input.GetButton ("Jump")) {
			yield return null;
		}
		etat = EtatPersonnage.EN_CHUTE;
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {

		// On regarde si le personnage s'accroche à un mur !
		if (etat == EtatPersonnage.EN_SAUT) {
			// Si la normale est au moins un peu à l'horizontale !
			Vector3 n = hit.normal;
			Vector3 nProject = Vector3.ProjectOnPlane (n, Vector3.up);
			if (Mathf.Abs (Vector3.Angle (n, nProject)) < 45f) {

				// Si on détecte une collision avec un mur, on peut s'aggriper si l'angle entre la normale
				// au mur et le vecteur (pointDepartSaut/mur) est inférieur à 45°
				Vector3 direction = pointDebutSaut - hit.transform.position;
				Vector3 directionProject = Vector3.ProjectOnPlane(direction, Vector3.up);

				if (Mathf.Abs (Vector3.Angle (nProject, directionProject)) < 45f) {
					etat = EtatPersonnage.AU_MUR; // YEAH !!!
				}
			}
		}
	}
}
