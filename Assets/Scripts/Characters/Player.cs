using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;

public class Player : Character {

    public enum EtatPersonnage { AU_SOL, EN_SAUT, EN_CHUTE, AU_MUR };

    public static Player _instance;

    [Header("Deplacements")]
	public float vitesseDeplacement; // la vitesse de déplacement horizontale
	public float hauteurCulminanteSaut; // la hauteur culminante du saut
    public float dureeAscensionSaut;
    public float dureeHorizontalSaut;
	public float dureeMur; // le temps que l'on peut rester accroché au mur
	public float distanceMurMax; // la distance maximale de laquelle on peut s'éloigner du mur
    public float sensibilite; // la sensibilité de la souris

    [Header("Pouvoirs")]
    public GameObject pouvoirAPrefab; // Le pouvoir de la touche A (souvent la détection)
    public GameObject pouvoirEPrefab; // Le pouvoir de la touche E (souvent la localisation)
    public GameObject pouvoirLeftBoutonPrefab; // Le pouvoir du bouton gauche de la souris
    public GameObject pouvoirRightBoutonPrefab; // Le pouvoir du bouton droit de la souris

    [Header("Camera")]
	public new Camera camera; // La camera du joueur !

	[HideInInspector]
	public GameObject personnage;
	[HideInInspector]
	public Console console;
	protected float xRot, yRot;
    [HideInInspector]
    public bool bSetUpRotation;

	protected EtatPersonnage etat; // l'état du personnage
	protected EtatPersonnage etatAvant; // l'état du personnage avant la frame
    protected bool isGrounded;
    protected Timer sautTimer;
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
    protected float skinWidthCoef = 1.1f;
    protected float lastAvancementSaut;

    protected IPouvoir pouvoirA; // Le pouvoir de la touche A (souvent la détection)
    protected IPouvoir pouvoirE; // Le pouvoir de la touche E (souvent la localisation)
    protected IPouvoir pouvoirLeftBouton; // Le pouvoir du bouton gauche de la souris
    protected IPouvoir pouvoirRightBouton; // Le pouvoir du bouton droit de la souris
    protected bool bCanUseLocalisation = true;
    [HideInInspector]
    public bool bIsStun = false;

    [HideInInspector]
	public Timer ennemiCaptureTimer; // Le temps depuis lequel le joueur est en contact avec un ennemi

    protected GameManager gm;


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
        sautTimer = new Timer(GetDureeTotaleSaut());
        sautTimer.SetOver();
        ennemiCaptureTimer = new Timer(5);

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
        switch (pouvoirBinding) {
            case PouvoirGiverItem.PouvoirBinding.A:
                pouvoirA = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                break;
            case PouvoirGiverItem.PouvoirBinding.E:
                pouvoirE = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                break;
            case PouvoirGiverItem.PouvoirBinding.LEFT_CLICK:
                pouvoirLeftBouton = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                break;
            case PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK:
                pouvoirRightBouton = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
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

        // Add post process effect when we are gripped to the wall
        gm.postProcessManager.UpdateGripEffect(etatAvant);

        // Pour détecter si le joueur a fait un grand saut
        DetecterGrandSaut(etatAvant);

        // Test pour savoir si on s'est fait capturé par un ennemi !
        UpdateCapturedByEnnemiTest();

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

    void GetEtatPersonnage() {
        etatAvant = etat;
        isGrounded = IsGrounded();
		if (etat != EtatPersonnage.AU_MUR) {
            if(Input.GetButton("Jump") && !sautTimer.IsOver()) {
                etat = EtatPersonnage.EN_SAUT;
            } else if (isGrounded) {
				etat = EtatPersonnage.AU_SOL;
                if (etat != etatAvant) {
                    gm.soundManager.PlayLandClip(transform.position);
                }
			} else {
                if (etat != EtatPersonnage.EN_SAUT) {
                    etat = EtatPersonnage.EN_CHUTE;
                }
            }
		}
	}

    // On met à jour le mouvement du joueur
    void UpdateMouvement() {
        if (GameManager.Instance.IsTimeFreezed()) {
            return;
        }

        GetEtatPersonnage();

        Vector3 move = Vector3.zero;

        move = UpdateHorizontalMouvement(move);

        move = UpdateJumpMouvement(move);

        move = gm.gravityManager.ApplyGravity(move);

        move = SlideBottomIfImportantSlope(move);

        controller.Move(move * Time.deltaTime);

        ApplyPoussees();
    }

    protected Vector3 UpdateJumpMouvement(Vector3 move) {
        if (!bIsStun) {
            switch (etat) {
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
                    if(sautTimer.IsOver() || Input.GetButtonUp("Jump")) {
                        etat = EtatPersonnage.EN_CHUTE;
                    } else {
                        move = ApplyJumpMouvement(move);
                    }
                    break;

                case EtatPersonnage.EN_CHUTE:
                    if (Input.GetButtonDown("Jump") && gm.gravityManager.HasGravity() && CanDoubleJump()) {
                        AddDoubleJump();
                        Jump(from: origineSaut);
                        move = ApplyJumpMouvement(move);
                    }
                    break;

                case EtatPersonnage.AU_MUR:
                    ResetDoubleJump();
                    if (Input.GetKey(KeyCode.LeftShift)) {
                        // On peut se décrocher du mur en appuyant sur shift
                        etat = EtatPersonnage.EN_CHUTE;
                        pointDebutSaut = transform.position;
                        origineSaut = EtatPersonnage.AU_SOL;
                        normaleOrigineSaut = normaleMur;
                        dureeMurRestante = dureeMurRestante - (Time.timeSinceLevelLoad - debutMur);

                    } else if (Input.GetButtonDown("Jump") && gm.gravityManager.HasGravity()) {
                        // On peut encore sauter quand on est au mur ! 
                        // Mais il faut appuyer à nouveau !
                        Jump(from: EtatPersonnage.AU_MUR);
                        move = ApplyJumpMouvement(move);
                        dureeMurRestante = dureeMur;

                    } else if (Input.GetButton("Jump")) {
                        // On a le droit de terminer son saut lorsqu'on touche un mur
                        move = ApplyJumpMouvement(move);
                    } else {
                        // Si on ne fait rien, alors on ne chute pas.
                        move = gm.gravityManager.CounterGravity(move);
                    }

                    // Si ça fait trop longtemps qu'on est sur le mur
                    // Ou que l'on s'éloigne trop du mur on tombe
                    Vector3 pos2mur = transform.position - pointMur;
                    float distanceMur = (pos2mur - Vector3.ProjectOnPlane(pos2mur, normaleMur)).magnitude; // pourtant c'est clair non ? Fais un dessins si tu comprends pas <3
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
                    RaycastHit hit;
                    if (!Physics.Raycast(ray, out hit, distanceMurMax) || hit.collider.tag != "Cube") {
                        etat = EtatPersonnage.EN_CHUTE;
                        pointDebutSaut = transform.position;
                        origineSaut = EtatPersonnage.AU_MUR;
                        normaleOrigineSaut = normaleMur;
                        dureeMurRestante = dureeMur;
                    }
                    break;
                default: break;
            }
        }

        return move;
    }

    protected Vector3 UpdateHorizontalMouvement(Vector3 move) {
        if (!bIsStun) {
            move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            move = camera.transform.TransformDirection(move);
            float magnitude = move.magnitude;

            // On va à l'horizontale si il y a de la gravité, sinon on peut "nager"
            if (gm.gravityManager.HasGravity()) {
                move = Vector3.ProjectOnPlane(move, gm.gravityManager.Up());
                move = move.normalized * magnitude;
            } else {
                if (Input.GetKey(KeyCode.LeftShift)) {
                    move += gm.gravityManager.Down();
                }
                if (Input.GetKey(KeyCode.Space)) {
                    move += gm.gravityManager.Up();
                }
            }
        }
        move *= vitesseDeplacement;
        return move;
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

    void UpdateCapturedByEnnemiTest() {
		Collider[] colliders = Physics.OverlapSphere (transform.position, 1f);
		foreach (Collider collider in colliders) {
			if (collider.tag == "Ennemi") {
                return;
			}
		}
        ennemiCaptureTimer.Reset();
    }

    protected bool IsGrounded() {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position,
            transform.localScale.x / 2.0f,
            gm.gravityManager.Down(),
            controller.skinWidth * skinWidthCoef);
        Vector3 up = gm.gravityManager.Up();
        foreach(RaycastHit hit in hits) {
            if (hit.collider.gameObject.tag == "Player"
             || hit.collider.gameObject.tag == "Objectif"
             || hit.collider.gameObject.tag == "Trigger")
                continue;
            Vector3 n = hit.normal;
            float angle = Vector3.Angle(n, up);
            //if(gm.gravityManager.Down() == Vector3.down) {
            //    normaleSol = n;
            //}
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
                                                  controller.skinWidth * skinWidthCoef);
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
        // On fait slider si on peut ! :)
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

    public float GetDureeTotaleSaut() {
        return dureeAscensionSaut + dureeHorizontalSaut;
    }

	void DetecterGrandSaut(EtatPersonnage etatAvant) {
		if((etatAvant == EtatPersonnage.EN_CHUTE || etatAvant == EtatPersonnage.EN_SAUT || etatAvant == EtatPersonnage.AU_MUR)
		&& (etat == EtatPersonnage.AU_SOL || etat == EtatPersonnage.AU_MUR)) {
            float hauteurSaut = hauteurMaxSaut - gm.gravityManager.GetHigh(transform.position);
			if(hauteurSaut > 7) {
				console.GrandSaut(hauteurSaut);
			}
		}
        MajHauteurMaxSaut();
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
            // Si on vient d'un mur, on vérifie que la normale du mur précédent est suffisamment différente de la normale actuelle !
            Vector3 n = hit.normal;
            if (origineSaut == EtatPersonnage.AU_SOL
            || (origineSaut == EtatPersonnage.AU_MUR && Vector3.Angle(normaleOrigineSaut, n) > 10)) {
                // Si la normale est au moins un peu à l'horizontale !
                Vector3 up = gm.gravityManager.Up();
                Vector3 nProject = Vector3.ProjectOnPlane(n, up);
                if (nProject != Vector3.zero && Mathf.Abs(Vector3.Angle(n, nProject)) < slideLimit) {
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
        //if (etat != previousEtat)
        //    gm.soundManager.PlayGripClip(transform.position);
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
        float proportionAscension = dureeAscensionSaut / GetDureeTotaleSaut();
        float avancementSaut = sautTimer.GetAvancement();
        float vitesseSaut = ComputeVitesseSaut();
        if (avancementSaut <= proportionAscension) {
            move += gm.gravityManager.Up() * vitesseSaut;
        } else if (lastAvancementSaut < proportionAscension) {
            float avancementRestant = (proportionAscension - lastAvancementSaut) / (avancementSaut - lastAvancementSaut);
            move += gm.gravityManager.Up() * vitesseSaut * avancementRestant;
        }
        move += gm.gravityManager.Up() * gm.gravityManager.gravityIntensity; // Pour contrer la gravité !
        lastAvancementSaut = avancementSaut;
        return move;
    }

    protected float ComputeVitesseSaut() {
        float sizePlayer = transform.localScale.y / 2 + controller.skinWidth;
        float vitesseSaut = (hauteurCulminanteSaut - sizePlayer) / dureeAscensionSaut;
        return vitesseSaut;
    }

    protected void Jump(EtatPersonnage from) {
        etat = EtatPersonnage.EN_SAUT;
        sautTimer = new Timer(GetDureeTotaleSaut());
        sautTimer.AdvanceTimerBy(Time.deltaTime);// Car on sait qu'on applique le jump tout de suite ! Comme ça on gagne une frame !
        pointDebutSaut = transform.position;
        origineSaut = from;
        lastAvancementSaut = 0f;
        if (from == EtatPersonnage.AU_SOL) {
        } else if (from == EtatPersonnage.AU_MUR) {
            normaleOrigineSaut = normaleMur;
        } else {
            Debug.Log("On saute depuis un endroit non autorisé !");
        }
        PlayJumpSound();
    }

    public void PlayJumpSound() {
        if (!gm.eventManager.IsGameOver()) {
            gm.soundManager.PlayJumpClip(transform.position);
        }
    }

    public void SetCarefulJumping(EtatPersonnage from) {
        Jump(from);
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

    public void RemoveAllPouvoirs() {
        SetPouvoir(null, PouvoirGiverItem.PouvoirBinding.A);
        SetPouvoir(null, PouvoirGiverItem.PouvoirBinding.E);
        SetPouvoir(null, PouvoirGiverItem.PouvoirBinding.LEFT_CLICK);
        SetPouvoir(null, PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK);
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
            transform.localScale.x / 2 + controller.skinWidth);
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


