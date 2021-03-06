﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class BouncyCube : Cube {

    public float distancePoussee = 3.0f;
    public float dureePoussee = 0.3f;
    public float dammageOnHit = 0.0f;

    protected Timer timerAddPoussee;

    public override void Initialize() {
        base.Initialize();
        GameManager gm = GameManager.Instance;
        Vector3 playerPos = gm.player.transform.position;
        timerAddPoussee = new Timer(0.1f);
        timerAddPoussee.SetOver();
        if(gm.player.DoubleCheckInteractWithCube(this)) {
            InteractWithPlayer();
        }
    }

    public override void InteractWithPlayer() {
        if (!timerAddPoussee.IsOver())
            return;
        Player player = gm.player;
        Vector3 direction = GetDirectionPoussee(player.transform.position);
        player.AddPoussee(new Poussee(direction, dureePoussee, distancePoussee));
        player.ResetGrip();
        if (Vector3.Dot(direction, gm.gravityManager.Up()) > 0 && InputManager.Instance.GetJump()) {
            player.SetCarefulJumping(Player.EtatPersonnage.AU_SOL);
        } else {
            player.RemoveGravityEffectFor(dureePoussee); // La gravité est déjà artificiellement annulée lors d'un saut :)
        }
        gm.soundManager.PlayBounceClip();
        if (dammageOnHit > 0.0f) {
            gm.timerManager.RemoveTime(dammageOnHit, EventManager.DeathReason.TOUCHED_BOUNCY_CUBE);
        }
        timerAddPoussee.Reset();
    }

    protected Vector3 GetDirectionPoussee(Vector3 position) {
        Vector3 direction = (position - transform.position).normalized;
        Vector3 directionPoussee = MathTools.GetClosestToNormals(transform, direction);
        return directionPoussee;
    }
}
