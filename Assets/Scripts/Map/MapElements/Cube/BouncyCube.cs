using System;
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

    public override void Start() {
        base.Start();
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
        if (Vector3.Dot(direction, gm.gravityManager.Up()) > 0 && Input.GetKey(KeyCode.Space)) {
            player.SetCarefulJumping(Player.EtatPersonnage.AU_SOL);
        } else {
            player.RemoveGravityEffectFor(dureePoussee); // La gravité est déjà artificiellement annulée lors d'un saut :)
            player.PlayJumpSound(); // Juste ici car il est déjà joué dans la simulation du saut ! :)
        }
        if (dammageOnHit > 0.0f) {
            gm.timerManager.RemoveTime(dammageOnHit, EventManager.DeathReason.TOUCHED_BOUNCY_CUBE);
        }
        timerAddPoussee.Reset();
    }

    protected Vector3 GetDirectionPoussee(Vector3 position) {
        List<Vector3> normals = GetAllNormals();
        Vector3 direction = (position - transform.position).normalized;
        return normals.OrderBy(n => Vector3.Dot(direction, n)).Last();
    }

    protected List<Vector3> GetAllNormals() {
        return new List<Vector3>() {
            transform.forward,
            - transform.forward,
            transform.right,
            - transform.right,
            transform.up,
            - transform.up,
        };
    }
}
