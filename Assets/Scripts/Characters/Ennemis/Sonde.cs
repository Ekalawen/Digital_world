using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonde : Ennemi {

    [Header("Poussee")]
	public float distancePoussee; // La distance sur laquelle on va pousser le personnage !
	public float tempsPoussee; // Le temps pendant lequel le personnage est poussé !

    protected Poussee pousseeCurrent;

	public override void Start () {
        base.Start();
		// Initialisation
		name = "Sonde_" + Random.Range (0, 9999);
        gm = GameManager.Instance;
        player = gm.player;
	}
	
	// Update is called once per frame
	public override void UpdateSpecific () {
	}

	// On vérifie si on a touché le joueur !!!
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.collider.name == "Joueur") {
            HitPlayer();
		}
	}

    protected override void HitPlayerSpecific() {
        // Si c'est le cas, on l'envoie ballader !
        if (pousseeCurrent != null && !pousseeCurrent.IsOver()) {
            pousseeCurrent.Stop();
        }
        Vector3 directionPoussee = player.transform.position - transform.position;
        directionPoussee.Normalize();
        pousseeCurrent = new Poussee(directionPoussee, tempsPoussee, distancePoussee);
        player.AddPoussee(pousseeCurrent);
        player.ResetGrip(); // Pour que le joueur puisse à nouveau s'accrocher aux murs !

        // Effet de vignette rouge
        gm.postProcessManager.UpdateHitEffect();
    }

    protected override void HitContinuousPlayerSpecific() {
    }
}
