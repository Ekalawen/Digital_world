using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonde : Ennemi {

	public enum EtatSonde {WAITING, TRACKING, DEFENDING, RUSHING};

	public float distancePoussee; // La distance sur laquelle on va pousser le personnage !
	public float tempsPoussee; // Le temps pendant lequel le personnage est poussé !
	public float coefficiantDeRushVitesse; // Le multiplicateur de vitesse lorsque les drones rushs
	public float coefficiantDeRushDistanceDeDetection; // Le multiplicateur de la portée de détection quand on est en rush

	protected EtatSonde etat;
	protected Vector3 lastPositionSeen; // La dernière position à laquelle le joueur a été vu !
    protected Poussee pousseeCurrent;

	public override void Start () {
        base.Start();
		// Initialisation
		name = "Sonde_" + Random.Range (0, 9999);
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        player = gm.player;
		etat = EtatSonde.WAITING;
		lastPositionSeen = transform.position;
	}
	
	// Update is called once per frame
	public override void UpdateSpecific () {
        GetEtat();

        // Tant que l'ennemi n'est pas visible et/ou trop proche, on reste sur place
        // Sinon on le pourchasse !
        switch (etat) {
            case EtatSonde.WAITING:
                // On va là où on a vu le joueur pour la dernière fois !
                Vector3 move = Move(lastPositionSeen);

                // Si le mouvement est trop petit, c'est que l'on est bloqué, donc on arrête le mouvement en lui ordonnant d'aller sur place
                if (Vector3.Magnitude(move) <= 0.001f && Vector3.Magnitude(move) != 0f)
                {
                    lastPositionSeen = transform.position;
                }
                break;

            case EtatSonde.TRACKING:
                // Les ennemis se déplacent toujours vers le joueur de façon tout à fait linéaire !		
                Move(player.transform.position);

                // Et on retient la position actuelle du joueur !
                lastPositionSeen = player.transform.position;
                break;
        }
	}

    // On récupère l'état dans lequel doit être notre sonde
    void GetEtat() {
        if(IsPlayerVisible()) {
            EtatSonde previousEtat = etat;
            etat = EtatSonde.TRACKING;
            // Si la sonde vient juste de le repérer, on l'annonce
            if(etat != previousEtat)
                gm.console.JoueurDetecte(name);
        } else {
            EtatSonde previousEtat = etat;
            etat = EtatSonde.WAITING;
            // Si la sonde vient juste de perdre sa trace, on l'annonce
            if (etat != previousEtat)
                gm.console.JoueurPerduDeVue(name);
        }
    }

	// Permet de savoir si le drone est en mouvement
	public bool IsMoving() {
		return (etat == EtatSonde.TRACKING || etat == EtatSonde.RUSHING) || Vector3.Distance(lastPositionSeen, transform.position) > 1f;
	}

	// On vérifie si on a touché le joueur !!!
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.collider.name == "Joueur") {
            HitPlayer();
		}
	}

    protected void HitPlayer() {
        // Si c'est le cas, on l'envoie ballader !
        if (pousseeCurrent != null && !pousseeCurrent.IsOver()) {
            pousseeCurrent.Stop();
        }
        Vector3 directionPoussee = player.transform.position - transform.position;
        directionPoussee.Normalize();
        pousseeCurrent = new Poussee(directionPoussee, tempsPoussee, distancePoussee);
        player.AddPoussee(pousseeCurrent);

        // Le son
        gm.soundManager.PlayHitClip(GetComponentInChildren<AudioSource>());

        // Effet de vignette rouge
        gm.postProcessManager.UpdateHitEffect();

        // Et on affiche un message dans la console !
        if (!gm.partieDejaTerminee) {
            gm.console.JoueurTouche();
        }
    }

}
