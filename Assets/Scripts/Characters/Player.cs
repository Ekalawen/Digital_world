using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.VFX;
using EZCameraShake;

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
    public float dureeCanJumpAfterFalling = 0.1f; // La durée pendant laquelle on peut encore sauter si on vient de tomber depuis l'état AU_SOL

    [Header("Pouvoirs")]
    public GameObject pouvoirAPrefab; // Le pouvoir de la touche A (souvent la détection)
    public GameObject pouvoirEPrefab; // Le pouvoir de la touche E (souvent la localisation)
    public GameObject pouvoirLeftBoutonPrefab; // Le pouvoir du bouton gauche de la souris
    public GameObject pouvoirRightBoutonPrefab; // Le pouvoir du bouton droit de la souris

    [Header("Camera")]
	public new Camera camera; // La camera du joueur !
    public VisualEffect dashVfx;
    public VisualEffect shiftVfx;
    public VisualEffect wallVfx;
    public VisualEffect jumpVfx;
    public CameraShaker cameraShaker;

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
	protected Vector3 pointMurSecondary; // le deuxième point sur lequel le personnage est accroché : utile pour les coins ! 
    protected Vector3 normaleSol; // La normale au sol lorsque l'on est au sol et que l'on essaye de slider vers le bas.
    protected Timer dureeMurTimer; // Le temps qu'il nous reste à être accroché au mur (utile pour les shifts qui peuvent nous décrocher)
    protected float dureeMurRestante = 0; // Le temps qu'il nous reste à être accroché au mur après s'en être détaché via SHIFT ! :)
    protected Timer timerLastTimeAuMur; // La dernière fois que l'on était accroché au mur.
    protected Timer timerLastTimeNotAuMur; // La dernière fois que l'on n'était PAS accroché au mur.
    protected int nbDoublesSautsMax = 0; // Nombre de doubles sauts
    protected int nbDoublesSautsCourrant = 0; // Le nombre de doubles sauts déjà utilisés
    protected float slideLimit; // La limite à partir de laquelle on va slider sur une surface.
    protected float skinWidthCoef = 1.1f;
    protected float lastAvancementSaut;
    protected Timer timerLastTimeAuSol;
    protected bool isGravityEffectRemoved = false;
    protected Timer gravityEffectRemovedForTimer = null;
    protected Coroutine gravityEffectRemovedForCoroutine = null;
    protected float cameraShakerInitialHeight;

    protected IPouvoir pouvoirA; // Le pouvoir de la touche A (souvent la détection)
    protected IPouvoir pouvoirE; // Le pouvoir de la touche E (souvent la localisation)
    protected IPouvoir pouvoirLeftBouton; // Le pouvoir du bouton gauche de la souris
    protected IPouvoir pouvoirRightBouton; // Le pouvoir du bouton droit de la souris
    protected bool bCanUseLocalisation = true;
    [HideInInspector]
    public bool bIsStun = false;
    protected UnityEvent onHitEvent;

    protected GameManager gm;
    protected InputManager inputManager;


    public override void Start() {
        base.Start();
    }

    public void Initialize(Vector3 position, Vector2 orientationXY)
    {
        // On récupère le personnage
        personnage = gameObject;
        personnage.name = "Joueur";
        controller = personnage.GetComponent<CharacterController>();
        gm = GameManager.Instance;
        inputManager = InputManager.Instance;
        GetPlayerSensitivity();
        bSetUpRotation = true;
        sautTimer = new Timer(GetDureeTotaleSaut());
        sautTimer.SetOver();
        onHitEvent = new UnityEvent();
        timerLastTimeAuSol = new Timer(dureeCanJumpAfterFalling);
        timerLastTimeAuSol.SetOver();
        dureeMurTimer = new Timer(dureeMur);
        dureeMurTimer.SetOver();
        timerLastTimeAuMur = new Timer();
        timerLastTimeNotAuMur = new Timer();
        cameraShakerInitialHeight = cameraShaker.RestPositionOffset.y;

        transform.position = position;

        // On regarde là où on nous dit de regarder
        Vector3 up = gm.gravityManager.Up();
        Vector3 cameraRight = camera.transform.right;
        camera.transform.RotateAround(camera.transform.position, cameraRight, orientationXY.x);
        camera.transform.RotateAround(camera.transform.position, up, orientationXY.y);

        pointDebutSaut = transform.position;
        slideLimit = controller.slopeLimit;
        ResetAuSol();
        ResetGrip();

        // On veut maintenant activer la caméra du playerPrefabs !
        personnage.GetComponentInChildren<Camera>().enabled = true;

        // On empêche la souris de sortir de l'écran !
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        console = GameObject.FindObjectOfType<Console>();

        InitPouvoirs();
    }

    internal void SetPouvoir(object pouvoirDash333Prefab, PouvoirGiverItem.PouvoirBinding pouvoirBinding)
    {
        throw new NotImplementedException();
    }

    public void GetPlayerSensitivity() {
        sensibilite = PrefsManager.GetFloat(PrefsManager.MOUSE_SPEED_KEY, MenuOptions.defaultMouseSpeed);
    }

    protected void InitPouvoirs() {
        if (pouvoirAPrefab != null) {
            pouvoirA = Instantiate(pouvoirAPrefab, parent: this.transform).GetComponent<IPouvoir>();
            pouvoirA.Initialize();
        }
        if (pouvoirEPrefab != null) {
            pouvoirE = Instantiate(pouvoirEPrefab, parent: this.transform).GetComponent<IPouvoir>();
            pouvoirE.Initialize();
        }
        if(pouvoirLeftBoutonPrefab != null) {
            pouvoirLeftBouton = Instantiate(pouvoirLeftBoutonPrefab, parent: this.transform).GetComponent<IPouvoir>();
            pouvoirLeftBouton.Initialize();
        }
        if (pouvoirRightBoutonPrefab != null) {
            pouvoirRightBouton = Instantiate(pouvoirRightBoutonPrefab, parent: this.transform).GetComponent<IPouvoir>();
            pouvoirRightBouton.Initialize();
        }
    }

    public void SetPouvoir(GameObject pouvoirPrefab, PouvoirGiverItem.PouvoirBinding pouvoirBinding) {
        switch (pouvoirBinding) {
            case PouvoirGiverItem.PouvoirBinding.A:
                pouvoirA = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                if(pouvoirA != null) {
                    pouvoirA.Initialize();
                }
                break;
            case PouvoirGiverItem.PouvoirBinding.E:
                pouvoirE = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                if(pouvoirE != null) {
                    pouvoirE.Initialize();
                }
                break;
            case PouvoirGiverItem.PouvoirBinding.LEFT_CLICK:
                pouvoirLeftBouton = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                if(pouvoirLeftBouton != null) {
                    pouvoirLeftBouton.Initialize();
                }
                break;
            case PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK:
                pouvoirRightBouton = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                if(pouvoirRightBouton != null) {
                    pouvoirRightBouton.Initialize();
                }
                break;
        }
        gm.console.InitPouvoirsDisplays();
    }

    public void SetPouvoirsCooldownZeroSwap() {
        List<IPouvoir> pouvoirs = new List<IPouvoir>() { pouvoirA, pouvoirE, pouvoirLeftBouton, pouvoirRightBouton };
        if (pouvoirs.FindAll(p => p != null).Select(p => p.GetCooldown().cooldown).Contains(0.0f)) {
            InitPouvoirs();
            gm.console.InitPouvoirsDisplays();
        } else {
            foreach (IPouvoir pouvoir in pouvoirs) {
                if (pouvoir != null) {
                    pouvoir.SetCooldownDuration(0.0f);
                    pouvoir.SetTimerMalus(0.0f);
                }
            }
        }
    }

    void Update ()
    {
        // On met à jour la caméra
        UpdateCamera();

        // Puis on met à jour la position du joueur
        UpdateMouvement();

        // Update post process effects
        UpdatePostProcessEffects();

        // Pour détecter si le joueur a fait un grand saut
        DetecterGrandSaut(etatAvant);

        // On vérifie si le joueur a utilisé l'un de ses pouvoirs ! :)
        TryUsePouvoirs();
    }

    private void UpdatePostProcessEffects() {
        if (gm.IsPaused())
            return;

        // Add post process effect when we are gripped to the wall
        gm.postProcessManager.UpdateWallEffect(etatAvant);

        // Add post process effect when we are pressing the shift key :)
        gm.postProcessManager.UpdateShiftEffect();
    }

    // Utilisé pour gérer la caméra
    void UpdateCamera() {
        if (gm.IsPaused())
            return;

        // On mesure la rotation que l'on veut faire
        Vector2 currentRotation = inputManager.GetCameraMouvement() * sensibilite;

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
            if (inputManager.GetJump() && !sautTimer.IsOver()) {
                etat = EtatPersonnage.EN_SAUT;
            } else if (isGrounded) {
                SetAuSol();
            }
            else {
                if (etat != EtatPersonnage.EN_SAUT) {
                    etat = EtatPersonnage.EN_CHUTE;
                    sautTimer.SetOver();
                }
            }
        }
	}

    public void SetAuSol(bool useSound = true) {
        etat = EtatPersonnage.AU_SOL;
        timerLastTimeAuSol.Reset();
        if (useSound && etat != etatAvant) {
            gm.soundManager.PlayLandClip(transform.position);
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

        move = ApplyGravity(move);

        move = SlideBottomIfImportantSlope(move);

        controller.Move(move * Time.deltaTime);

        ApplyPoussees();
    }

    protected Vector3 UpdateJumpMouvement(Vector3 move) {
        if (!bIsStun) {
            switch (etat) {
                case EtatPersonnage.AU_SOL:
                    if (inputManager.GetJump() && gm.gravityManager.HasGravity()) {
                        Jump(from: EtatPersonnage.AU_SOL);
                        move = ApplyJumpMouvement(move);
                    }
                    ResetAuSol();
                    break;

                case EtatPersonnage.EN_SAUT:
                    if (inputManager.GetJumpDown() && gm.gravityManager.HasGravity() && CanDoubleJump()) {
                        AddDoubleJump();
                        Jump(from: origineSaut);
                    }
                    if(sautTimer.IsOver() || inputManager.GetJumpUp()) {
                        etat = EtatPersonnage.EN_CHUTE;
                    } else {
                        move = ApplyJumpMouvement(move);
                    }
                    break;

                case EtatPersonnage.EN_CHUTE:
                    if (inputManager.GetJumpDown()) {
                        if(!timerLastTimeAuSol.IsOver()) { // Permet de sauter quelques frames après être tombé d'une plateforme !
                            timerLastTimeAuSol.SetOver();
                            Jump(from: EtatPersonnage.AU_SOL);
                            move = ApplyJumpMouvement(move);
                        } else if (gm.gravityManager.HasGravity() && CanDoubleJump()) {
                            AddDoubleJump();
                            Jump(from: origineSaut);
                            move = ApplyJumpMouvement(move);
                        }
                    }
                    break;

                case EtatPersonnage.AU_MUR:
                    ResetDoubleJump();
                    if (inputManager.GetShift()) {
                        // On peut se décrocher du mur en appuyant sur shift
                        etat = EtatPersonnage.EN_CHUTE;
                        pointDebutSaut = transform.position;
                        origineSaut = EtatPersonnage.AU_MUR;
                        normaleOrigineSaut = normaleMur;
                        dureeMurRestante = dureeMurTimer.GetRemainingTime();
                        timerLastTimeAuMur.Reset();

                    } else if (inputManager.GetJumpDown() && gm.gravityManager.HasGravity()) {
                        // On peut encore sauter quand on est au mur ! 
                        // Mais il faut appuyer à nouveau !
                        Jump(from: EtatPersonnage.AU_MUR);
                        move = ApplyJumpMouvement(move);

                    } else if (inputManager.GetJump()) {
                        // On a le droit de terminer son saut lorsqu'on touche un mur
                        move = ApplyJumpMouvement(move);

                    } else {
                        // Si on ne fait rien, alors on ne chute pas.
                        if (!isGravityEffectRemoved) {
                            move = gm.gravityManager.CounterGravity(move);
                        }
                    }

                    // Si ça fait trop longtemps qu'on est sur le mur
                    // Ou que l'on s'éloigne trop du mur on tombe
                    Vector3 pos2mur = transform.position - pointMur;
                    Vector3 posOnMur = Vector3.ProjectOnPlane(pos2mur, normaleMur) + pointMur;
                    float distanceMur = (posOnMur - transform.position).magnitude; // pourtant c'est clair non ? Fais un dessins si tu comprends pas <3
                    if (dureeMurTimer.IsOver() || distanceMur >= distanceMurMax) {
                        FallFromWall();
                    }

                    // Et on veut aussi vérifier que le mur continue encore à nos cotés !
                    // Pour ça on va lancer un rayon ! <3
                    Ray ray = new Ray(transform.position, -normaleMur);
                    RaycastHit[] hits = Physics.SphereCastAll(ray, GetSizeRadius(), distanceMurMax);
                    List<string> hitTags = hits.Select(h => h.collider.tag).ToList();
                    if (hits.Length == 0 || (!hitTags.Contains("Cube") && !hitTags.Contains("Ennemi"))) { // Pour pouvoir s'accrocher sur les Tracers inactifs !
                        FallFromWall();
                    }
                    break;
                default: break;
            }
        }

        return move;
    }

    public Vector3 GetCurrentPosOnMur() {
        if (pointMurSecondary == Vector3.zero) {
            Vector3 pos2mur = transform.position - pointMur;
            return Vector3.ProjectOnPlane(pos2mur, normaleMur) + pointMur;
        } else {
            Vector3 mergedPointMur = Vector3.ProjectOnPlane(pointMurSecondary - pointMur, normaleMur) + pointMur;
            Vector3 mergedNormalMur = (normaleMur.normalized + (pointMur - mergedPointMur).normalized).normalized;
            Vector3 pos2mur = transform.position - mergedPointMur;
            Vector3 posOnMur = Vector3.ProjectOnPlane(pos2mur, mergedNormalMur) + mergedPointMur;
            return posOnMur;
        }
    }

    protected void FallFromWall() {
        etat = EtatPersonnage.EN_CHUTE;
        pointDebutSaut = transform.position;
        origineSaut = EtatPersonnage.AU_MUR;
        normaleOrigineSaut = normaleMur;
        dureeMurRestante = 0;
        timerLastTimeAuMur.Reset();
    }

    protected Vector3 UpdateHorizontalMouvement(Vector3 move) {
        if (!bIsStun) {
            move = inputManager.GetHorizontalMouvement();
            move = camera.transform.TransformDirection(move);
            float magnitude = move.magnitude;

            // On va à l'horizontale si il y a de la gravité, sinon on peut "nager"
            if (gm.gravityManager.HasGravity())
            {
                move = Vector3.ProjectOnPlane(move, gm.gravityManager.Up());
                move = move.normalized * magnitude;
            }
            else
            {
                if (inputManager.GetShift())
                {
                    move += gm.gravityManager.Down();
                }
                if (inputManager.GetJump()) // Was Input.GetKey(KeyCode.Space) before InputManager refactor !
                {
                    move += gm.gravityManager.Up();
                }
            }
        }
        move *= vitesseDeplacement;
        return move;
    }

    protected Vector3 ApplyGravity(Vector3 move) {
        if(!isGravityEffectRemoved) {
            move = gm.gravityManager.ApplyGravity(move);
        }
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
        ResetOrigineSaut();
    }

    protected void ResetDoubleJump() {
        nbDoublesSautsCourrant = 0;
    }

    protected void ResetDureeMur() {
        dureeMurTimer.Reset();
    }

    protected void ResetOrigineSaut() {
        normaleOrigineSaut = Vector3.zero;
        origineSaut = EtatPersonnage.AU_SOL;
    }

    // Permet de s'accrocher à nouveau à un mur !
    public void ResetGrip() {
        //dureeMurRestante = 0;
        ResetOrigineSaut();
        ResetDureeMur();
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
             || hit.collider.gameObject.tag == "Trigger") {
                continue;
            }
            if (hit.collider.gameObject.GetComponent<BouncyCube>() != null) { // Pour empêcher de pouvoir sauter sur eux avant qu'ils nous bouncent !
                continue;
            }
            Cube cube = hit.collider.gameObject.GetComponent<Cube>();
            if (cube != null) {
                cube.InteractWithPlayer(); // Pour être sur qu'on ne saute pas sur un Cube sans le trigger ! :3
            }
            Vector3 n = hit.normal;
            float angle = Vector3.Angle(n, up);
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
            float hauteurSaut = hauteurMaxSaut - gm.gravityManager.GetHeightInMap(transform.position);
			if(hauteurSaut > 7) {
				console.GrandSaut(hauteurSaut);
			}
		}
        MajHauteurMaxSaut();
	}

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //// Si on a touché un cube spécial, on fait une action !
        Cube cube = hit.gameObject.GetComponent<Cube>();
        if (cube != null && DoubleCheckInteractWithCube(cube)) {
            cube.InteractWithPlayer();
        }

        // On regarde si le personnage s'accroche à un mur !
        // Pour ça il doit être dans les airs !
        // Et il ne doit PAS être en train d'appuyer sur SHIFT
        if (CanGrip(cube)) {
            // Si on vient d'un mur, on vérifie que la normale du mur précédent est suffisamment différente de la normale actuelle !
            Vector3 wallNormal = GetWallNormalFromHit(hit);
            if (CanGripToWall(wallNormal)) {
                // Si la normale est au moins un peu à l'horizontale !
                Vector3 up = gm.gravityManager.Up();
                Vector3 nProject = Vector3.ProjectOnPlane(wallNormal, up);
                if (nProject != Vector3.zero && Mathf.Abs(Vector3.Angle(wallNormal, nProject)) < slideLimit) {
                    GripOn(hit, wallNormal);
                }
            }
        }
    }

    protected Vector3 GetWallNormalFromHit(ControllerColliderHit hit) {
        // Il est possible de supprimer cette fonctionnalité pour permettre au joueur d'être plus sticky sur les murs :) (et notemment les angles !)
        BoxCollider box = hit.gameObject.GetComponent<BoxCollider>();
        if(box != null && box.transform.rotation == Quaternion.identity) {
            Vector3 normalizedNormal = MathTools.GetClosestToNormals(box.transform, hit.normal);
            return normalizedNormal;
        } else {
            return hit.normal;
        }
    }

    protected bool CanGrip(Cube cube) {
        // AU_MUR pour pouvoir s'accrocher à un mur depuis un autre mur ! :)
        return (etat == EtatPersonnage.EN_SAUT || etat == EtatPersonnage.EN_CHUTE || etat == EtatPersonnage.AU_MUR)
            && !inputManager.GetShift()
            && (cube == null || cube.gameObject.GetComponent<BouncyCube>() == null); // On ne veut pas s'accrocher sur les bouncy cubes ! :)
    }

    protected bool CanGripToWall(Vector3 wallNormal) {
        return origineSaut == EtatPersonnage.AU_SOL
           || (origineSaut == EtatPersonnage.AU_MUR && AreWallsNormalsDifferent(normaleOrigineSaut, wallNormal))
           || (origineSaut == EtatPersonnage.AU_MUR && etat == EtatPersonnage.EN_CHUTE && dureeMurRestante > 0);
    }

    public static bool AreWallsNormalsDifferent(Vector3 normalMur1, Vector3 normalMur2) {
        return Vector3.Angle(normalMur1, normalMur2) > 10;
    }

    protected void GripOn(ControllerColliderHit hit, Vector3 wallNormal) {
        EtatPersonnage previousEtat = etat;
        etat = EtatPersonnage.AU_MUR; // YEAH !!!
        if(debutMur == Time.timeSinceLevelLoad) {
            if(!MathTools.IsInPlane(pointMur, hit.point, wallNormal)) {
                pointMurSecondary = pointMur;
            }
        } else {
            pointMurSecondary = Vector3.zero;
        }
        pointMur = hit.point;
        debutMur = Time.timeSinceLevelLoad;
        normaleMur = wallNormal;
        if(normaleOrigineSaut == Vector3.zero || AreWallsNormalsDifferent(normaleOrigineSaut, normaleMur)) {
            ResetDureeMur();
            dureeMurRestante = 0;
        } else {
            dureeMurTimer.SetRemainingTime(dureeMurRestante);
        }
        Cube cube = hit.gameObject.GetComponent<Cube>();
        if(cube != null) {
            cube.InteractWithPlayer();
        }
        if(previousEtat != EtatPersonnage.AU_MUR) {
            timerLastTimeNotAuMur.Reset();
        }

        // Pour pouvoir s'accrocher à un autre mur depuis ce mur-ci !
        origineSaut = EtatPersonnage.AU_MUR;
        normaleOrigineSaut = normaleMur;

        //if (etat != previousEtat)
        //    gm.soundManager.PlayGripClip(transform.position);
    }

	public void MajHauteurMaxSaut() {
		if(etat == EtatPersonnage.EN_SAUT || etat == EtatPersonnage.EN_CHUTE) {
			if(gm.gravityManager.GetHeightInMap(transform.position) > hauteurMaxSaut) {
                hauteurMaxSaut = gm.gravityManager.GetHeightInMap(transform.position);
			}
		} else {
            hauteurMaxSaut = gm.gravityManager.GetHeightInMap(transform.position);
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
        if (!isGravityEffectRemoved) {
            move = gm.gravityManager.CounterGravity(move);
        }
        lastAvancementSaut = avancementSaut;
        return move;
    }

    protected float ComputeVitesseSaut() {
        float sizePlayer = GetSizeRadius();
        float vitesseSaut = (hauteurCulminanteSaut - sizePlayer) / dureeAscensionSaut;
        return vitesseSaut;
    }

    public float GetSizeRadius() {
        return transform.localScale.y / 2 + controller.skinWidth;
    }

    protected void Jump(EtatPersonnage from) {
        etat = EtatPersonnage.EN_SAUT;
        sautTimer = new Timer(GetDureeTotaleSaut());
        sautTimer.AdvanceTimerBy(Time.deltaTime);// Car on sait qu'on applique le jump tout de suite ! Comme ça on gagne une frame !
        pointDebutSaut = transform.position;
        origineSaut = from;
        lastAvancementSaut = 0f;
        timerLastTimeAuSol.SetOver(); // On ne veut pas pouvoir double sauter à cause de ça !
        if (from == EtatPersonnage.AU_SOL) {
        } else if (from == EtatPersonnage.AU_MUR) {
            normaleOrigineSaut = normaleMur;
            timerLastTimeAuMur.Reset();
        } else {
            Debug.Log("On saute depuis un endroit non autorisé !");
        }
        PlayJumpSound();
        gm.postProcessManager.StartJumpEffect();
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
        if (gm.eventManager.IsGameOver() || gm.IsPaused())
            return;
        // A
        if(inputManager.GetPouvoirADown()) {
            if (pouvoirA != null)
                pouvoirA.TryUsePouvoir(inputManager.GetPouvoirAKeyCode());
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
        // E
        if(inputManager.GetPouvoirEDown()) {
            if (pouvoirE != null)
                pouvoirE.TryUsePouvoir(inputManager.GetPouvoirEKeyCode());
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
        // Click Gauche
        if(inputManager.GetPouvoirLeftClickDown()) {
            if (pouvoirLeftBouton != null)
                pouvoirLeftBouton.TryUsePouvoir(inputManager.GetPouvoirLeftClickKeyCode());
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
        // Click Droit
        if(inputManager.GetPouvoirRightClickDown()) {
            if (pouvoirRightBouton != null)
                pouvoirRightBouton.TryUsePouvoir(inputManager.GetPouvoirRightClickKeyCode());
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

    public bool DoubleCheckInteractWithCube(Cube cube) {
        if (controller == null) {
            return false;
        }
        if (cube.transform.rotation == Quaternion.identity) {
            return MathTools.AABBSphere(cube.transform.position,
                Vector3.one * cube.transform.localScale.x / 2,
                transform.position,
                transform.localScale.x / 2 + controller.skinWidth * skinWidthCoef);
        } else {
            return MathTools.OBBSphere(cube.transform.position,
                Vector3.one * cube.transform.localScale.x / 2,
                cube.transform.rotation,
                transform.position,
                transform.localScale.x / 2 + controller.skinWidth * skinWidthCoef);
        }
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

    public void OnHit() {
        onHitEvent.Invoke();
    }

    public void RegisterOnHit(UnityAction call) {
        onHitEvent.AddListener(call);
    }

    public float GetDureeRestanteMur() {
        return dureeMurTimer.GetRemainingTime();
    }

    public void RemoveGravityEffect() {
        isGravityEffectRemoved = true;
    }

    public void UnremoveGravityEffect() {
        isGravityEffectRemoved = false;
    }

    public void RemoveGravityEffectFor(float duree) {
        Timer timer = gravityEffectRemovedForTimer;
        if(timer == null || timer.IsOver()) {
            timer = new Timer(duree);
            gravityEffectRemovedForCoroutine = StartCoroutine(CRemoveGravityEffectFor(duree));
        } else {
            if(timer.GetRemainingTime() < duree) {
                StopCoroutine(gravityEffectRemovedForCoroutine);
                timer = new Timer(duree);
                gravityEffectRemovedForCoroutine = StartCoroutine(CRemoveGravityEffectFor(duree));
            }
        }
    }
    protected IEnumerator CRemoveGravityEffectFor(float duree) {
        RemoveGravityEffect();
        gravityEffectRemovedForTimer = new Timer(duree);
        yield return new WaitForSeconds(duree);
        UnremoveGravityEffect();
    }

    public void Stun() {
        bIsStun = true;
        FreezePouvoirs(true);
    }

    public void UnStun() {
        bIsStun = false;
        FreezePouvoirs(false);
    }

    public Timer GetSautTimer() {
        return sautTimer;
    }

    public Timer GetDureeMurRestanteTimer() {
        return dureeMurTimer;
    }

    public float GetDureeDejaAuMur() {
        return dureeMurTimer.GetElapsedTime();
    }

    public Timer GetTimerLastTimeAuMur() {
        return timerLastTimeAuMur;
    }

    public Timer GetTimerLastTimeNotAuMur() {
        return timerLastTimeNotAuMur;
    }

    public float GetCameraShakerHeight() {
        return gm.gravityManager.GetHeightAbsolute(cameraShaker.RestPositionOffset);
    }

    public void SetCameraShakerHeight(float newHeight) {
        Vector3 offset = cameraShaker.RestPositionOffset;
        int heightIndice = gm.gravityManager.GetHeightIndice();
        float sign = gm.gravityManager.GetHeightSign();
        for(int i = 0; i < 3; i++) {
            if (i == heightIndice) {
                offset[i] = sign * newHeight;
            } else {
                if(offset[i] != 0) {
                    offset[i] = Mathf.Sign(offset[i]) * (cameraShakerInitialHeight - newHeight);
                }
            }
        }
        cameraShaker.RestPositionOffset = offset;
    }

    public float GetCameraShakerInitialHeight() {
        return cameraShakerInitialHeight;
    }
}
