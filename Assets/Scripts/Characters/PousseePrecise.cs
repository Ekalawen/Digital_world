using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PousseePrecise : Poussee {

    protected Vector3 target;
    protected float adjustmentDuree;
    protected Timer adjustmentTimer;
    public UnityEvent onPousseeFinish;

    public PousseePrecise(Vector3 target, Vector3 source, float adjustmentDuree, float vitesse, UnityAction callback = null)
        : base(Vector3.zero, 0, 0) { // Because it's mandatory
        this.direction = (target - source).normalized;
        this.distance = (target - source).magnitude;
        this.vitesse = vitesse;
        this.duree = distance / vitesse;

        this.dureeTimer = new Timer(duree);
        this.target = target;
        this.adjustmentDuree = adjustmentDuree;
        this.adjustmentTimer = new Timer(duree + adjustmentDuree);
        onPousseeFinish = new UnityEvent();
        if(callback != null) {
            onPousseeFinish.AddListener(callback);
        }
    }

    public override Vector3 ComputePoussee(CharacterController controller) {
        if (!dureeTimer.IsOver()) {
            adjustmentTimer.GetNewAvancementUpTo1(); // To keep track of newAvancement also in this timer
            Vector3 mouvement = base.ComputePoussee(controller);
            return mouvement;
        }
        if(!adjustmentTimer.IsOver()) {
            float distanceCourante = Time.deltaTime * vitesse;
            float distanceToTarget = (target - controller.transform.position).magnitude;
            if(distanceCourante >= distanceToTarget) {
                distanceCourante = distanceToTarget;
                FinishPoussee();
            }
            Vector3 directionCourante = (target - controller.transform.position).normalized;
            Vector3 mouvement = directionCourante * distanceCourante;
            return mouvement;
        }
        return Vector3.zero;
    }

    protected void FinishPoussee() {
        adjustmentTimer.SetOver();
        onPousseeFinish.Invoke();
    }

    public override bool IsOver() {
        return adjustmentTimer.IsOver();
    }
}
