﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flird : Ennemi {

	public override void Start () {
        base.Start();
		name = "Flird_" + UnityEngine.Random.Range (0, 9999);
	}
	
	public override void UpdateSpecific () {
        // Pas besoin pour le moment
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.collider.name == "Joueur") {
            HitPlayer();
		} else {
            Cube cube = hit.gameObject.GetComponent<Cube>();
            if (cube != null) {
                Debug.Log($"J'ai pris un mur m'sieu' ! ({gm.ennemiManager.ennemis.Count})");
                Suicide();
            }
        }
	}

    protected override void HitPlayerSpecific() {
        // Effet de vignette rouge
        gm.postProcessManager.UpdateHitEffect();
        Suicide();
    }

    protected void Suicide() {
        DestroyEnnemi();
    }

    protected override void HitContinuousPlayerSpecific() {
        // Pas besoin pour le moment
    }

    public override EventManager.DeathReason GetDeathReason() {
        return EventManager.DeathReason.FLIRD_HIT;
    }
}
