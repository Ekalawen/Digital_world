using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BouncyCube : Cube {

    public float distancePoussee = 3.0f;
    public float dureePoussee = 0.3f;
    public float dammageOnHit = 0.0f;
    public Transform quadFolder;

    protected Timer timerAddPoussee;

    protected override void Start() {
        base.Start();
        GameManager gm = GameManager.Instance;
        Vector3 playerPos = gm.player.transform.position;
        timerAddPoussee = new Timer(0.01f);
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
        if (Vector3.Dot(direction, gm.gravityManager.Up()) > 0) {
            player.SetCarefulJumping(Player.EtatPersonnage.AU_SOL);
        } else {
            player.PlayJumpSound();
        }
        if(dammageOnHit > 0.0f) {
            gm.timerManager.AddTime(-dammageOnHit);
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

    public override void AddColor(Color addedColor) {
        base.AddColor(addedColor);
        foreach(Transform quad in quadFolder) {
            quad.GetComponent<Renderer>().material.color += addedColor;
        }
    }

    public override void SetColor(Color newColor) {
        base.SetColor(newColor);
        foreach(Transform quad in quadFolder) {
            quad.GetComponent<Renderer>().material.color = newColor;
        }
    }
}
