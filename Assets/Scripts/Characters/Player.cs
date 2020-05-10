using System.Collections;
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
	public new Camera camera; // La camera du joueur !
	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	[HideInInspector]
	public GameObject personnage;
	[HideInInspector]
	public Console console;
	protected float xRot, yRot;
    [HideInInspector]
    public bool bSetUpRotation;

	protected EtatPersonnage etat; // l'état du personnage
    protected bool isGrounded;
    protected float debutSaut; // le timing où le personnage a débuté son dernier saut !
	protected Vector3 pointDebutSaut; // le point de départ du saut !
	protected EtatPersonnage origineSaut; // Permet de savoir depuis où (le sol ou un mur) le personnage a sauté !
	protected Vector3 normaleOrigineSaut; // La normale au plan du mur duquel le personnage a sauté
    protected EtatPersonnage jumpedFrom;
	protected float hauteurMaxSaut; // La hauteur maximale d'un saut !
	protected float debutMur; // le timing où le personnage a commencé à s'accrocher au mur !
	protected Vector3 normaleMur; // la normale au mur sur lequel le personnage est accroché !
	protected Vector3 pointMur; // un point du mur sur lequel le personnage est accroché ! En effet, la normale ne suffit pas :p
    protected Vector3 normaleSol; // La normale au sol lorsque l'on est au sol et que l'on essaye de slider vers le bas.
    protected float dureeMurRestante; // Le temps qu'il nous reste à être accroché au mur (utile pour les shifts qui peuvent nous décrocher)
    protected int nbDoublesSautsMax = 0; // Nombre de doubles sauts
    protected int nbDoublesSautsCourrant = 0; // Le nombre de doubles sauts déjà utilisés
    protected float slideLimit; // La limite à partir de laquelle on va slider sur une surface.

    protected IPouvoir pouvoirA; // Le pouvoir de la touche A (souvent la détection)
    protected IPouvoir pouvoirE; // Le pouvoir de la touche E (souvent la localisation)
    protected IPouvoir pouvoirLeftBouton; // Le pouvoir du bouton gauche de la souris
    protected IPouvoir pouvoirRightBouton; // Le pouvoir du bouton droit de la souris
    protected bool bCanUseLocalisation = true;
    [HideInInspector]
    public bool bIsStun = false;

    [HideInInspector]
	public float lastNotContactEnnemy; // Le dernier temps où il ne touchait pas d'ennemi, utilisé pour la fin du jeu

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
        gm = FindObjectOfType<GameManager>();
        sensibilite = PlayerPrefs.GetFloat(MenuOptions.MOUSE_SPEED_KEY);
        bSetUpRotation = true;

        transform.position = position;

        // On regarde là où on nous dit de regarder
        Vector3 up = gm.gravityManager.Up();
        Vector3 cameraRight = camera.transform.right;
        camera.transform.RotateAround(camera.transform.position, cameraRight, orientationXY.x);
        camera.transform.RotateAround(camera.transform.position, up, orientationXY.y);

		pointDebutSaut = transform.position;
        slideLimit = controller.slopeLimit;

        // On veut maintenant activer la caméra du playerPrefabs !
        personnage.GetComponentInChildren<Camera>().enabled = true;

        // On empêche la souris de sortir de l'écran !
        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

        console = GameObject.FindObjectOfType<Console>();

        InitPouvoirs();
    }

    protected void InitPouvoirs() {
        if (pouvoirAPrefab != null)
            pouvoirA = Instantiate(pouvoirAPrefab, parent: this.transform).GetComponent<IPouvoir>();
        if (pouvoirEPrefab != null)
            pouvoirE = Instantiate(pouvoirEPrefab, parent: this.transform).GetComponent<IPouvoir>();
        if(pouvoirLeftBoutonPrefab != null)
            pouvoirLeftBouton = Instantiate(pouvoirLeftBoutonPrefab, parent: this.transform).GetComponent<IPouvoir>();
        if(pouvoirRightBoutonPrefab != null)
            pouvoirRightBouton = Instantiate(pouvoirRightBoutonPrefab, parent: this.transform).GetComponent<IPouvoir>();
    }

    public void SetPouvoir(GameObject pouvoirPrefab, PouvoirGiverItem.PouvoirBinding pouvoirBinding) {
        switch(pouvoirBinding) {
            case PouvoirGiverItem.PouvoirBinding.A:
                pouvoirA = Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                break;
            case PouvoirGiverItem.PouvoirBinding.E:
                pouvoirE = Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                break;
            case PouvoirGiverItem.PouvoirBinding.LEFT_CLICK:
                pouvoirLeftBouton = Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                break;
            case PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK:
                pouvoirRightBouton = Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                break;
        }
        gm.console.InitPouvoirsDisplays();
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
            if (bSetUpRotation)
                camera.transform.LookAt(camera.transform.position + camera.transform.forward, up);
        } else {
            Debug.Log("PING !");
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
        if(GameManager.Instance.IsTimeFreezed()) {
            return;
        }

        // On récupère le mouvement dans le sens de l'orientation du personnage
        Vector3 move = Vector3.zero;

        if (!bIsStun) {
            move = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
            move = camera.transform.TransformDirection(move);
            float magnitude = move.magnitude;

            // On va à l'horizontale si il y a de la gravité, sinon on peut "nager"
            if (gm.gravityManager.HasGravity()) {
                move = Vector3.ProjectOnPlane(move, gm.gravityManager.Up());
                move = move.normalized * magnitude;
            } else {
                // On peut descendre avec Shift
                if (Input.GetKey(KeyCode.LeftShift)) {
                    move += gm.gravityManager.Down();
                }

                // Et monter avec space
                if (Input.GetKey(KeyCode.Space)) {
                    move += gm.gravityManager.Up();
                }
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
        if (!bIsStun)
        {
            switch (etat)
            {
                case EtatPersonnage.AU_SOL:
                    if (Input.GetButton("Jump") && gm.gravityManager.HasGravity()) {
                        Jump(from: EtatPersonnage.AU_SOL);
                        move = ApplyJumpMouvement(move);
                    }
                    ResetAuSol();
                    break;

                case EtatPersonnage.EN_SAUT:
                    if (Input.GetButtonDown("Jump") && gm.gravityManager.HasGravity() && CanDoubleJump()) {
                        AddDoubleJump();
                        Jump(from: origineSaut);
                    }
                    move = ApplyJumpMouvement(move);
                    break;

                case EtatPersonnage.EN_CHUTE:
                    if (Input.GetButtonDown("Jump") && gm.gravityManager.HasGravity() && CanDoubleJump()) {
                        AddDoubleJump();
                        Jump(from: origineSaut);
                        move = ApplyJumpMouvement(move);
                    }
                    break;

                case EtatPersonnage.AU_MUR:
                    // On peut se décrocher du mur en appuyant sur shift
                    ResetDoubleJump();
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        etat = EtatPersonnage.EN_CHUTE;
                        pointDebutSaut = transform.position;
                        origineSaut = EtatPersonnage.AU_SOL;
                        normaleOrigineSaut = normaleMur;
                        dureeMurRestante = dureeMurRestante - (Time.timeSinceLevelLoad - debutMur);
                        // On peut encore sauter quand on est au mur ! 
                    }
                    else if (Input.GetButtonDown("Jump") && gm.gravityManager.HasGravity())
                    { // Mais il faut appuyer à nouveau !
                        Jump(from: EtatPersonnage.AU_MUR);
                        move = ApplyJumpMouvement(move);
                        dureeMurRestante = dureeMur;
                    }
                    else if (Input.GetButton("Jump"))
                    { // On a le droit de terminer son saut lorsqu'on touche un mur
                        move = ApplyJumpMouvement(move);
                    }
                    else
                    {
                        move = gm.gravityManager.CounterGravity(move);
                    }
                    // Si ça fait trop longtemps qu'on est sur le mur
                    // Ou que l'on s'éloigne trop du mur on tombe
                    float distanceMur = ((transform.position - pointMur) - Vector3.ProjectOnPlane((transform.position - pointMur), normaleMur)).magnitude; // pourtant c'est clair non ? Fais un dessins si tu comprends pas <3
                    if ((Time.timeSinceLevelLoad - debutMur) >= dureeMurRestante
                        || distanceMur >= distanceMurMax)
                    {
                        etat = EtatPersonnage.EN_CHUTE;
                        pointDebutSaut = transform.position;
                        origineSaut = EtatPersonnage.AU_MUR;
                        normaleOrigineSaut = normaleMur;
                        dureeMurRestante = dureeMur;
                    }

                    // Et on veut aussi vérifier que le mur continue encore à nos cotés !
                    // Pour ça on va lancer un rayon ! <3
                    Ray ray = new Ray(transform.position, -normaleMur);
                    RaycastHit hit; // là où l'on récupère l'information du ray !
                    if (!Physics.Raycast(ray, out hit, distanceMurMax) || hit.collider.tag != "Cube")
                    {
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
        }

        move = gm.gravityManager.ApplyGravity(move);

        gm.postProcessManager.UpdateGripEffect(etatAvant);

        // On fait slider si on peut ! :)
        move = SlideBottomIfImportantSlope(move);

        controller.Move(move * Time.deltaTime);
    }

    protected void AddDoubleJump() {
        nbDoublesSautsCourrant++;
    }

    protected bool CanDoubleJump() {
        return nbDoublesSautsCourrant < nbDoublesSautsMax;
    }

    protected void ResetAuSol() {
        ResetDoubleJump();
        ResetDureeMur();
    }

    protected void ResetDoubleJump() {
        nbDoublesSautsCourrant = 0;
    }

    protected void ResetDureeMur() {
        dureeMurRestante = dureeMur;
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
             || hit.collider.gameObject.tag == "Objectif"
             || hit.collider.gameObject.tag == "Trigger")
                continue;
            Vector3 n = hit.normal;
            float angle = Vector3.Angle(n, up);
            if(gm.gravityManager.Down() == Vector3.down) {
                normaleSol = n;
            }
            if (Mathf.Abs(angle) <= slideLimit) {
                return true;
            }
        }
        return false;
    }

    protected Vector3 GetNormaleToDirection(Vector3 move) {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position,
                                                  transform.localScale.x / 2.0f,
                                                  move.normalized,
                                                  controller.skinWidth * 1.1f);
        Vector3 up = gm.gravityManager.Up();
        foreach (RaycastHit hit in hits) {
            if (hit.collider.gameObject.tag == "Player"
             || hit.collider.gameObject.tag == "Objectif"
             || hit.collider.gameObject.tag == "Trigger")
                continue;
            Vector3 n = hit.normal;
            return n;
        }
        return Vector3.zero;
    }

    protected bool IsAxisAligned(Vector3 move) {
        return move != Vector3.zero
            && ( Vector3.Cross(move, Vector3.up) == Vector3.zero
            || Vector3.Cross(move, Vector3.down) == Vector3.zero
            || Vector3.Cross(move, Vector3.forward) == Vector3.zero
            || Vector3.Cross(move, Vector3.back) == Vector3.zero
            || Vector3.Cross(move, Vector3.right) == Vector3.zero
            || Vector3.Cross(move, Vector3.left) == Vector3.zero);
    }

    protected Vector3 SlideBottomIfImportantSlope(Vector3 move) {
        Vector3 normaleToDirection = GetNormaleToDirection(move);
        Vector3 bottomMove = Vector3.Dot(move, gm.gravityManager.Down()) * gm.gravityManager.Down();
        if (!IsAxisAligned(normaleToDirection)
         && Mathf.Abs(Vector3.Angle(-bottomMove, normaleToDirection)) >= slideLimit
         && Vector3.Dot(normaleToDirection, gm.gravityManager.Up()) > 0f) {
            float magnitude = move.magnitude;
            move = Vector3.ProjectOnPlane(move, normaleToDirection);
            move = move.normalized * magnitude;
        }
        return move;
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
                    gm.soundManager.PlayLandClip(transform.position);
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

        //// Si on a touché un cube spécial, on fait une action !
        Cube cube = hit.gameObject.GetComponent<Cube>();
        if (cube != null && DoubleCheckInteractWithCube(cube))
            cube.InteractWithPlayer();

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
					if (nProject != Vector3.zero && Mathf.Abs(Vector3.Angle (n, nProject)) < slideLimit) {
                        /*// Si on détecte une collision avec un mur, on peut s'aggriper si l'angle entre la normale
                        // au mur et le vecteur (pointDepartSaut/mur) est inférieur à slideLimit°
                        Vector3 direction = pointDebutSaut - hit.point;
                        Vector3 directionProject = Vector3.ProjectOnPlane(direction, Vector3.up);
                        if (Mathf.Abs (Vector3.Angle (nProject, directionProject)) < slideLimit) {*/
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
        origineSaut = from;
        if (from == EtatPersonnage.AU_SOL) {
        } else if (from == EtatPersonnage.AU_MUR) {
            normaleOrigineSaut = normaleMur;
        } else {
            Debug.Log("On saute depuis un endroit non autorisé !");
        }
        if (!gm.eventManager.IsGameOver()) {
            gm.soundManager.PlayJumpClip(transform.position);
        }
        StartCoroutine (StopJump (debutSaut));
    }

    protected void TryUsePouvoirs() {
        if (gm.eventManager.IsGameOver())
            return;
        // A
        if(Input.GetKeyDown(KeyCode.A)) {
            if (pouvoirA != null)
                pouvoirA.TryUsePouvoir(KeyCode.A);
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
        // E
        if(Input.GetKeyDown(KeyCode.E)) {
            if (pouvoirE != null)
                pouvoirE.TryUsePouvoir(KeyCode.E);
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
        // Click Gauche
        if(Input.GetMouseButtonDown(0)) {
            if (pouvoirLeftBouton != null)
                pouvoirLeftBouton.TryUsePouvoir(KeyCode.Mouse0);
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
        // Click Droit
        if(Input.GetMouseButtonDown(1)) {
            if (pouvoirRightBouton != null)
                pouvoirRightBouton.TryUsePouvoir(KeyCode.Mouse1);
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
    }

    public void FreezeLocalisation() {
        bCanUseLocalisation = false;
    }

    public void FreezePouvoirs(bool value = true) {
        pouvoirA?.FreezePouvoir(value);
        pouvoirE?.FreezePouvoir(value);
        pouvoirLeftBouton?.FreezePouvoir(value);
        pouvoirRightBouton?.FreezePouvoir(value);
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

    public void ResetGrip() {
        // Permet de s'accrocher à nouveau à un mur !
        origineSaut = EtatPersonnage.AU_SOL;
    }

    public bool DoubleCheckInteractWithCube(Cube cube) {
        return MathTools.AABBSphere(cube.transform.position,
            Vector3.one * cube.transform.localScale.x / 2,
            transform.position,
            controller.radius + controller.skinWidth);
        //Vector3 playerPos = transform.position;
        //Vector3 cubePos = cube.transform.position;
        //float playerExtends = controller.radius + controller.skinWidth;
        //float cubeExtends = cube.transform.localScale.x / 2;

        //// Get the closest point to the sphere by clamping
        //float x = Mathf.Clamp(playerPos.x, cubePos.x - cubeExtends, cubePos.x + cubeExtends);
        //float y = Mathf.Clamp(playerPos.y, cubePos.y - cubeExtends, cubePos.y + cubeExtends);
        //float z = Mathf.Clamp(playerPos.z, cubePos.z - cubeExtends, cubePos.z + cubeExtends);
        //Vector3 closestPoint = new Vector3(x, y, z);

        //float distance = Vector3.Distance(closestPoint, playerPos);
        //return distance <= playerExtends;
    }

    public int GetNbDoubleSautsMax() {
        return nbDoublesSautsMax;
    }

    public void AddDoubleJump(int nbToAdd) {
        nbDoublesSautsMax += nbToAdd;
    }

    public void SetNbDoubleJumps(int nbDoubleJumps) {
        nbDoublesSautsMax = nbDoubleJumps;
    }

    public IPouvoir GetPouvoirA() {
        return pouvoirA;
    }
    public IPouvoir GetPouvoirE() {
        return pouvoirE;
    }
    public IPouvoir GetPouvoirLeftClick() {
        return pouvoirLeftBouton;
    }
    public IPouvoir GetPouvoirRightClick() {
        return pouvoirRightBouton;
    }
}


