using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PousseeFromTransformWithTreshold : PousseeFromTransform {

    protected float tresholdStraight;
    protected Vector3 lastDirection;

    public PousseeFromTransformWithTreshold(Transform originTransform, float duree, float distance, float tresholdStraight)
    : base(originTransform, duree, distance) {
        this.tresholdStraight = tresholdStraight;
        this.lastDirection = Vector3.zero;
    }

    public override Vector3 ComputePoussee(CharacterController controller) {
        float dureeCourante = dureeTimer.GetNewAvancement() * duree;
        float distanceCourante = dureeCourante * vitesse;
        Vector3 directionCourante;
        if (Vector3.Distance(controller.transform.position, originTransform.transform.position) <= tresholdStraight) {
            directionCourante = (controller.transform.position - originTransform.transform.position).normalized;
            lastDirection = directionCourante;
        } else {
            directionCourante = lastDirection;
        }
        return directionCourante * distanceCourante;
    }

}
