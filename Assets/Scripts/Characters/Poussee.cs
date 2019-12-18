using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poussee {
    public Vector3 direction;
    public float duree;
    public float distance;

    protected float debut;
    protected float vitesse;
    protected float lastTimeApplied;

    public Poussee(Vector3 direction, float duree, float distance) {
        this.direction = direction.normalized;
        this.duree = duree;
        this.distance = distance;
        this.debut = Time.timeSinceLevelLoad;
        this.vitesse = distance / duree;
        this.lastTimeApplied = debut;
    }

    public bool IsOver() {
        return Time.timeSinceLevelLoad - debut > duree;
    }

    public void ApplyPoussee(CharacterController controller) {
        float dureeCourante = Time.timeSinceLevelLoad - lastTimeApplied;
        float distanceCourante = dureeCourante * vitesse;
        controller.Move(direction * distanceCourante);

        lastTimeApplied = Time.timeSinceLevelLoad;
    }

    public void Refresh() {
        debut = Time.timeSinceLevelLoad;
        lastTimeApplied = debut;
    }

    public void Stop() {
        debut = Time.timeSinceLevelLoad - duree;
        lastTimeApplied = debut;
    }
}
