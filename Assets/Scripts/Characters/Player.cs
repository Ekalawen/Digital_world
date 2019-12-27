﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;

public class Player : Character {

    //////////////////////////////////////////////////////////////////////////////////////
    // ENUMERATION
    //////////////////////////////////////////////////////////////////////////////////////

    public enum EtatPersonnage { AU_SOL, EN_SAUT, EN_CHUTE, AU_MUR, AU_POTEAU };

    //////////////////////////////////////////////////////////////////////////////////////
    // ATTRIBUTS PUBLIQUES
    /////////////////////////////////////////////////////////////////////////////////////

    public static Player _instance;
	public float vitesseDeplacement; // la vitesse de déplacement horizontale
	public float vitesseSaut; // la vitesse d'élévation du saut
	public float dureeSaut; // la durée totale d'un saut
	public float dureeEfficaciteSaut; // le pourcentage de temps où le saut nous surélève
	public float gravite; // la force de la gravité
	public float sensibilite; // la sensibilité de la souris
	public float dureeMur; // le temps que l'on peut rester accroché au mur
	public float distanceMurMax; // la distance maximale de laquelle on peut s'éloigner du mur

    public GameObject pouvoirAPrefab; // Le pouvoir de la touche A (souvent la détection)
    public GameObject pouvoirEPrefab; // Le pouvoir de la touche E (souvent la localisation)
    public GameObject pouvoirLeftBoutonPrefab; // Le pouvoir du bouton gauche de la souris
    public GameObject pouvoirRightBoutonPrefab; // Le pouvoir du bouton droit de la souris
	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	[HideInInspector]
	public GameObject personnage;
	[HideInInspector]
	public Console console;
	[HideInInspector]
	public new Camera camera;
	protected float xRot, yRot;
    [HideInInspector]
    public bool bSetUpRotation;

	protected EtatPersonnage etat; // l'état du personnage
    protected bool isGrounded;
    protected float debutSaut; // le timing où le personnage a débuté son dernier saut !
	protected Vector3 pointDebutSaut; // le point de départ du saut !
	protected EtatPersonnage origineSaut; // Permet de savoir depuis où (le sol ou un mur) le personnage a sauté !
	protected Vector3 normaleOrigineSaut; // La normale au plan du mur duquel le personnage a sauté
	protected float hauteurMaxSaut; // La hauteur maximale d'un saut !
	protected float debutMur; // le timing où le personnage a commencé à s'accrocher au mur !
	protected Vector3 normaleMur; // la normale au mur sur lequel le personnage est accroché !
	protected Vector3 pointMur; // un point du mur sur lequel le personnage est accroché ! En effet, la normale ne suffit pas :p
    protected float dureeMurRestante; // Le temps qu'il nous reste à être accroché au mur (utile pour les shifts qui peuvent nous décrocher)

    protected IPouvoir pouvoirA; // Le pouvoir de la touche A (souvent la détection)
    protected IPouvoir pouvoirE; // Le pouvoir de la touche E (souvent la localisation)
    protected IPouvoir pouvoirLeftBouton; // Le pouvoir du bouton gauche de la souris
    protected IPouvoir pouvoirRightBouton; // Le pouvoir du bouton droit de la souris
    protected bool bCanUseLocalisation = true;

    protected Quaternion originaleRotation;

    [HideInInspector]
	public float lastNotContactEnnemy; // Le dernier temps où il ne touchait pas d'ennemi, utilisé pour la fin du jeu

    [HideInInspector]
    public AudioSource audioSource;
    protected GameManager gm;


    //////////////////////////////////////////////////////////////////////////////////////
    // METHODES
    //////////////////////////////////////////////////////////////////////////////////////

    public override void Start() {
        base.Start();
    }

    public void Initialize(Vector3 position, Vector2 orientationXY) {
        // On récupère le personnage
        personnage = gameObject;
        personnage.name = "Joueur";
		controller = personnage.GetComponent<CharacterController> ();
		camera = personnage.transform.GetChild(0).GetComponent<Camera>() as Camera;
        audioSource = GetComponentInChildren<AudioSource>();
        gm = FindObjectOfType<GameManager>();
        sensibilite = PlayerPrefs.GetFloat(MenuOptions.MOUSE_SPEED_KEY);
        originaleRotation = camera.transform.localRotation;

        //ChoseStartingPosition();
        transform.position = position;

        // On regarde là où on nous dit de regarder
        xRot = orientationXY.x;
        yRot = orientationXY.y;

		pointDebutSaut = transform.position;

        // On veut maintenant activer la caméra du playerPrefabs !
        personnage.GetComponentInChildren<Camera>().enabled = true;

        // On empêche la souris de sortir de l'écran !
        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

        console = GameObject.FindObjectOfType<Console>();

        if(pouvoirAPrefab != null)
            pouvoirA = Instantiate(pouvoirAPrefab, parent: this.transform).GetComponent<IPouvoir>();
        if(pouvoirEPrefab != null)
            pouvoirE = Instantiate(pouvoirEPrefab, parent: this.transform).GetComponent<IPouvoir>();
        if(pouvoirLeftBoutonPrefab != null)
            pouvoirLeftBouton = Instantiate(pouvoirLeftBoutonPrefab, parent: this.transform).GetComponent<IPouvoir>();
        if(pouvoirRightBoutonPrefab != null)
            pouvoirRightBouton = Instantiate(pouvoirRightBoutonPrefab, parent: this.transform).GetComponent<IPouvoir>();
    }

    void ChoseStartingPosition() {
    }

    void Update () {
        // On met à jour la caméra
        UpdateCamera();

        // Puis on met à jour la position du joueur
        UpdateMouvement();

        // Test pour savoir si on s'est fait capturé par un ennemi !
        UpdateLastNotContactEnnemi();

        // On vérifie si le joueur a utilisé l'un de ses pouvoirs ! :)
        TryUsePouvoirs();
	}

    // Utilisé pour gérer la caméra
    void UpdateCamera() {
        // On mesure la rotation que l'on veut faire
        xRot = -Input.GetAxis("Mouse Y") * sensibilite;
        yRot = Input.GetAxis("Mouse X") * sensibilite;
        Vector3 currentRotation = new Vector3(xRot, yRot, 0);

        // On précalcul les principaux vecteurs
        Vector3 up = gm.gravityManager.Up();
        Vector3 right = gm.gravityManager.Right();
        Vector3 cameraRight = camera.transform.right;

        // On retient l'orientation que l'on avait avant
        Quaternion rotationAvant = camera.transform.rotation;
        Vector3 forwardAvant = camera.transform.forward;

        // On tourne
        camera.transform.RotateAround(camera.transform.position, cameraRight, currentRotation.x);
        camera.transform.RotateAround(camera.transform.position, up, currentRotation.y);

        // Si on a dépassé le up avec la rotation, alors la rotation est remise "en arrière"
        float tresholdAngle = 0.01f; // degrées, et le plus petit possible pour que l'on ne s'en rende pas compte :3
        float dot = Vector3.Dot(camera.transform.forward, up);
        Vector3 cross1 = Vector3.Cross(forwardAvant, up);
        Vector3 cross2 = Vector3.Cross(camera.transform.forward, up);
        if (Vector3.Dot(cross1, cross2) < 0) { // Si ils ne sont pas dans le même sens !
            float angle = Vector3.Angle(up * Mathf.Sign(dot), camera.transform.forward);
            float halfAngle = tresholdAngle / 2.0f;
            camera.transform.forward = Quaternion.AngleAxis(Mathf.Sign(dot) * (angle + halfAngle), cross2) * camera.transform.forward;
        }

        // On remet le up dans le bon sens si on peut, sinon on cap le vecteur au treshold !
        Vector3 tresholdVector = Quaternion.AngleAxis(tresholdAngle, right) * up;
        float tresholdDot = Vector3.Dot(tresholdVector, up);
        if (Mathf.Abs(dot) <= Mathf.Abs(tresholdDot)) {
            if(bSetUpRotation)
                camera.transform.LookAt(camera.transform.position + camera.transform.forward, up);
        } else {
            Vector3 axe = Vector3.Cross(up * Mathf.Sign(dot), camera.transform.forward);
            Vector3 recherche = Quaternion.AngleAxis(tresholdAngle, axe) * up * Mathf.Sign(dot);
            if (bSetUpRotation)
                camera.transform.LookAt(camera.transform.position + recherche, up);
            else
                camera.transform.forward = camera.transform.position + recherche;
        }
    }

    // On met à jour le mouvement du joueur
    void UpdateMouvement() {
        // Si le temps est freeze, on ne se déplace pas !
        if(GameManager.Instance.timeFreezed) {
            return;
        }

		// On récupère le mouvement dans le sens de l'orientation du personnage
		Vector3 move = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
        move = camera.transform.TransformDirection (move);
        float magnitude = move.magnitude;

        // On va à l'horizontale si il y a de la gravité, sinon on peut "nager"
        if (gm.gravityManager.HasGravity()) {
            move = Vector3.ProjectOnPlane(move, gm.gravityManager.Up());
            move = move.normalized * magnitude;
        } else {
            // On peut descendre avec Shift
            if(Input.GetKey(KeyCode.LeftShift)) {
                move += gm.gravityManager.Down();
            }

            // Et monter avec space
            if(Input.GetKey(KeyCode.Space)) {
                move += gm.gravityManager.Up();
            }
        }

        // On applique la vitesse au déplacement
        move *= vitesseDeplacement;

        // On applique les poussées !
        ApplyPoussees();

		// On retient l'état d'avant
		EtatPersonnage etatAvant = etat;

		// On trouve l'état du personnage
		GetEtatPersonnage();

		// Si on a fait un grand saut, on le dit
		DetecterGrandSaut(etatAvant);
		MajHauteurMaxSaut();

		// En fonction de l'état du personnage, on applique le mouvement correspondant !
		switch (etat) {
		case EtatPersonnage.AU_SOL:
			if (Input.GetButton ("Jump") && gm.gravityManager.HasGravity()) {
                Jump(from: EtatPersonnage.AU_SOL);
                move = ApplyJumpMouvement(move);
			}
            dureeMurRestante = dureeMur;
			break;

		case EtatPersonnage.EN_SAUT:
            move = ApplyJumpMouvement(move);
			break;

		case EtatPersonnage.EN_CHUTE:
			//move.y -= gravite;
			break;

		case EtatPersonnage.AU_MUR:
			// On peut se décrocher du mur en appuyant sur shift
			if (Input.GetKey (KeyCode.LeftShift)) {
				etat = EtatPersonnage.EN_CHUTE;
				pointDebutSaut = transform.position;
				origineSaut = EtatPersonnage.AU_SOL;
				normaleOrigineSaut = normaleMur;
                dureeMurRestante = dureeMurRestante - (Time.timeSinceLevelLoad - debutMur);
			// On peut encore sauter quand on est au mur ! 
			} else if (Input.GetButtonDown ("Jump") && gm.gravityManager.HasGravity()) { // Mais il faut appuyer à nouveau !
                Jump(from: EtatPersonnage.AU_MUR);
                move = ApplyJumpMouvement(move);
                dureeMurRestante = dureeMur;
			} else if (Input.GetButton ("Jump")) { // On a le droit de terminer son saut lorsqu'on touche un mur
                move = ApplyJumpMouvement(move);
			} else {
                move = gm.gravityManager.CounterGravity(move);
            }
			// Si ça fait trop longtemps qu'on est sur le mur
			// Ou que l'on s'éloigne trop du mur on tombe
			float distanceMur = ((transform.position - pointMur) - Vector3.ProjectOnPlane ((transform.position - pointMur), normaleMur)).magnitude; // pourtant c'est clair non ? Fais un dessins si tu comprends pas <3
			if ((Time.timeSinceLevelLoad - debutMur) >= dureeMurRestante
			    || distanceMur >= distanceMurMax) {
				etat = EtatPersonnage.EN_CHUTE;
				pointDebutSaut = transform.position;
				origineSaut = EtatPersonnage.AU_MUR;
				normaleOrigineSaut = normaleMur;
                dureeMurRestante = dureeMur;
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
                dureeMurRestante = dureeMur;
			}

			break;
		case EtatPersonnage.AU_POTEAU:
			// C'est possible de rajouter ça quand on voudra =)
			break;
		}

        move = gm.gravityManager.ApplyGravity(move);

        gm.postProcessManager.UpdateGripEffect(etatAvant);

		controller.Move (move * Time.deltaTime);
    }

    // Permet de savoir la dernière fois qu'il a été en contact avec un ennemi !
    void UpdateLastNotContactEnnemi() {
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
    }

    protected bool IsGrounded() {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position,
            transform.localScale.x / 2.0f,
            gm.gravityManager.Down(), 
            controller.skinWidth * 1.1f);
        Vector3 up = gm.gravityManager.Up();
        foreach(RaycastHit hit in hits) {
            if (hit.collider.gameObject.tag == "Player"
             || hit.collider.gameObject.tag == "Trigger")
                continue;
            Vector3 n = hit.normal;
            float angle = Vector3.Angle(n, up);
            if (Mathf.Abs(angle) <= 45f) {
                return true;
            }
        }
        return false;
    }

    // Pour mettre à jour l'état du personnage !
    void GetEtatPersonnage() {
        isGrounded = IsGrounded();
		if (etat != EtatPersonnage.AU_MUR) {
            int currentFrame = Time.frameCount;
			if (isGrounded) {
                EtatPersonnage previousEtat = etat;
				etat = EtatPersonnage.AU_SOL;
                if (etat != previousEtat)
                    gm.soundManager.PlayLandClip(audioSource);
			} else {
				if (etat != EtatPersonnage.EN_SAUT) {
					etat = EtatPersonnage.EN_CHUTE;
				}
			}
		}
	}

	IEnumerator StopJump(float debut) {
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

	void DetecterGrandSaut(EtatPersonnage etatAvant) {
		if((etatAvant == EtatPersonnage.EN_CHUTE || etatAvant == EtatPersonnage.EN_SAUT || etatAvant == EtatPersonnage.AU_MUR)
		&& (etat == EtatPersonnage.AU_SOL || etat == EtatPersonnage.AU_MUR)) {
            float hauteurSaut = hauteurMaxSaut - gm.gravityManager.GetHigh(transform.position);
			if(hauteurSaut > 7) {
				console.GrandSaut(hauteurSaut);
			}
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {

        if(hit.gameObject.GetComponent<DeathCube>() != null) {
            Debug.Log("Looooooooooooooooose ! :'(");
            gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
        }

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
                    Vector3 up = gm.gravityManager.Up();
					Vector3 nProject = Vector3.ProjectOnPlane (n, up);
					if (nProject != Vector3.zero && Mathf.Abs(Vector3.Angle (n, nProject)) < 45f) {
                        /*// Si on détecte une collision avec un mur, on peut s'aggriper si l'angle entre la normale
                        // au mur et le vecteur (pointDepartSaut/mur) est inférieur à 45°
                        Vector3 direction = pointDebutSaut - hit.point;
                        Vector3 directionProject = Vector3.ProjectOnPlane(direction, Vector3.up);
                        if (Mathf.Abs (Vector3.Angle (nProject, directionProject)) < 45f) {*/
                        GripOn(hit);
					}
				}
		}
	}

    protected void GripOn(ControllerColliderHit hit) {
        EtatPersonnage previousEtat = etat;
        etat = EtatPersonnage.AU_MUR; // YEAH !!!
        debutMur = Time.timeSinceLevelLoad;
        normaleMur = hit.normal;
        pointMur = hit.point;
        gm.postProcessManager.UpdateGripEffect(previousEtat);
        //if(etat != previousEtat)
        //    gm.soundManager.PlayGripClip(audioSource);
    }

	public void MajHauteurMaxSaut() {
		if(etat == EtatPersonnage.EN_SAUT || etat == EtatPersonnage.EN_CHUTE) {
			if(gm.gravityManager.GetHigh(transform.position) > hauteurMaxSaut) {
                hauteurMaxSaut = gm.gravityManager.GetHigh(transform.position);
			}
		} else {
            hauteurMaxSaut = gm.gravityManager.GetHigh(transform.position);
		}
	}

    protected Vector3 ApplyJumpMouvement(Vector3 move) {
        float percentSaut = (Time.timeSinceLevelLoad - debutSaut) / dureeSaut;
        if (percentSaut <= dureeEfficaciteSaut) {
            move = gm.gravityManager.MoveOppositeDirectionOfGravity(move, vitesseSaut);
            //move.y += vitesseSaut;
        } else {
            move = gm.gravityManager.MoveOppositeDirectionOfGravity(move, gm.gravityManager.gravityIntensity);
        }
        //move = gm.gravityManager.CounterGravity(move);
        return move;
    }

    protected void Jump(EtatPersonnage from) {
        etat = EtatPersonnage.EN_SAUT;
        debutSaut = Time.timeSinceLevelLoad;
        pointDebutSaut = transform.position;
        if (from == EtatPersonnage.AU_SOL) {
            origineSaut = EtatPersonnage.AU_SOL;
        } else if (from == EtatPersonnage.AU_MUR) {
            origineSaut = EtatPersonnage.AU_MUR;
            normaleOrigineSaut = normaleMur;
        } else {
            Debug.Log("On saute depuis un endroit non autorisé !");
        }
        if (!gm.eventManager.IsGameOver()) {
            gm.soundManager.PlayJumpClip(audioSource);
        }
        StartCoroutine (StopJump (debutSaut));
    }

    protected void TryUsePouvoirs() {
        if (gm.eventManager.IsGameOver())
            return;
        // A
        if(Input.GetKeyDown(KeyCode.A))
            pouvoirA?.TryUsePouvoir();
        // E
        if(Input.GetKeyDown(KeyCode.E))
            pouvoirE?.TryUsePouvoir();
        // Click Gauche
        if(Input.GetMouseButtonDown(0))
            pouvoirLeftBouton?.TryUsePouvoir();
        // Click Droit
        if(Input.GetMouseButtonDown(1))
            pouvoirRightBouton?.TryUsePouvoir();
    }

    public void FreezeLocalisation() {
        bCanUseLocalisation = false;
    }

    public void FreezePouvoirs() {
        pouvoirA?.FreezePouvoir();
        pouvoirE?.FreezePouvoir();
        pouvoirLeftBouton?.FreezePouvoir();
        pouvoirRightBouton?.FreezePouvoir();
    }

    public bool CanUseLocalisation() {
        return bCanUseLocalisation;
    }

    public EtatPersonnage GetEtat() {
        return etat;
    }

    public override void ApplyPoussees() {
        base.ApplyPoussees();
        gm.postProcessManager.SetBlur(poussees.Count > 0);
    }
}


