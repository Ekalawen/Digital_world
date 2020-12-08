using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SondeController : EnnemiController {

	public enum EtatSonde {WAITING, TRACKING, DEFENDING, RUSHING, WANDERING};

	protected EtatSonde etat;
	protected Vector3 lastPositionSeen; // La dernière position à laquelle le joueur a été vu !
    protected Vector3 lastPosition;

    public override void Start() {
        base.Start();
		etat = EtatSonde.WAITING;
		lastPositionSeen = transform.position;
    }

    protected override void UpdateSpecific() {
        GetEtat();

        // Tant que l'ennemi n'est pas visible et/ou trop proche, on reste sur place
        // Sinon on le pourchasse !
        switch (etat) {
            case EtatSonde.WAITING:
                Wait();
                break;
            case EtatSonde.TRACKING:
                Track();
                break;
        }
        lastPosition = transform.position;
    }

    // On récupère l'état dans lequel doit être notre sonde
    protected virtual void GetEtat() {
        if(IsPlayerVisible()) {
            EtatSonde previousEtat = etat;
            etat = EtatSonde.TRACKING;
            // Si la sonde vient juste de le repérer, on l'annonce
            if (etat != previousEtat && lastPositionSeen == transform.position) {
                gm.soundManager.PlayDetectionClip(transform.position, transform);
            }
        } else {
            EtatSonde previousEtat = etat;
            etat = EtatSonde.WAITING;
            // Si la sonde vient juste de perdre sa trace, on l'annonce
            if (etat != previousEtat)
                gm.console.JoueurPerduDeVue(name);
        }
    }

	// Permet de savoir si le drone est en mouvement
	public override bool IsMoving() {
		return (etat == EtatSonde.TRACKING || etat == EtatSonde.RUSHING) || Vector3.Distance(lastPositionSeen, transform.position) > 1f;
	}

	// Permet de savoir si le drone est inactif
    public override bool IsInactive() {
        return etat == EtatSonde.WAITING && lastPositionSeen == transform.position;
    }

    protected virtual void Wait() {
        // On va là où on a vu le joueur pour la dernière fois !
        Vector3 move = MoveToTarget(lastPositionSeen);

        // Si le mouvement est trop petit, c'est que l'on est arrivé, donc on arrête le mouvement en lui ordonnant d'aller sur place
        if (Vector3.Magnitude(move) <= 0.001f && Vector3.Magnitude(move) != 0f) {
            lastPositionSeen = transform.position;
        }
        // Si on arrive plus à bouger, on s'arrête :)
        if(Vector3.Distance(transform.position, lastPosition) <= 0.001f) {
            lastPositionSeen = transform.position;
        }
    }

    protected virtual void Track() {
        // Les ennemis se déplacent toujours vers le joueur de façon tout à fait linéaire !		
        MoveToTarget(player.transform.position);

        // Et on retient la position actuelle du joueur !
        lastPositionSeen = player.transform.position;
    }
}
