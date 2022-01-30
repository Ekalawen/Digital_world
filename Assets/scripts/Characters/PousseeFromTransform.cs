using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PousseeFromTransform : Poussee {

    protected Transform originTransform;

    public PousseeFromTransform(Transform originTransform, float duree, float distance)
    : base(Vector3.zero, duree, distance) {
        this.originTransform = originTransform;
    }

    public override Vector3 ComputePoussee(CharacterController controller) {
        float dureeCourante = dureeTimer.GetNewAvancementUpTo1() * duree;
        float distanceCourante = dureeCourante * vitesse;
        Vector3 directionCourante = (controller.transform.position - originTransform.transform.position).normalized;
        return directionCourante * distanceCourante;
    }

}
