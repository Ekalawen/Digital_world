using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poussee {
    public Vector3 direction;
    public float duree;
    public float distance;
    public bool isNegative = false;

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

    public virtual bool IsOver() {
        return dureeTimer.IsOver();
    }

    public virtual void ApplyPoussee(CharacterController controller) {
        controller.Move(ComputePoussee(controller));
    }

    public void ApplyPousseeOnEnnenmiController(EnnemiController controller) {
        controller.Move(ComputePoussee(null)); // Pas terrible du tout ça ! x)
    }

    public virtual Vector3 ComputePoussee(CharacterController controller) {
        float dureeCourante = dureeTimer.GetNewAvancementUpTo1() * duree;
        float distanceCourante = dureeCourante * vitesse;
        return direction * distanceCourante;
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
