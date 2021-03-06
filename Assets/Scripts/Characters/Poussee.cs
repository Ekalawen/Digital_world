﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poussee {
    public Vector3 direction;
    public float duree;
    public float distance;

    protected float vitesse;
    protected Timer dureeTimer;

    public Poussee(Vector3 direction, float duree, float distance) {
        this.direction = direction.normalized;
        this.distance = distance;
        this.vitesse = distance / duree;

        this.duree = duree;
        this.dureeTimer = new Timer(duree);
    }

    public static Poussee CreatePoussee(Vector3 direction, float duree, float puissance) {
        return new Poussee(direction, duree, puissance * duree);
    }

    public bool IsOver() {
        return dureeTimer.IsOver();
    }

    public virtual void ApplyPoussee(CharacterController controller) {
        float dureeCourante = dureeTimer.GetNewAvancement() * duree;
        float distanceCourante = dureeCourante * vitesse;
        controller.Move(direction * distanceCourante);
    }

    public void Reset() {
        dureeTimer.Reset();
    }

    public void Stop() {
        dureeTimer.SetOver();
    }

    public void Redirect(Vector3 direction) {
        this.direction = direction.normalized;
    }
}
