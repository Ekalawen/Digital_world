using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierFarFromCenter : GetHelperModifier {

    public float minDistance = 3.0f;
    public bool useCustomPosition = false;
    [ConditionalHide("useCustomPosition")]
    public Vector3 customPosition;

    public override bool IsInArea(Vector3 position) {
        Vector3 center = useCustomPosition ? customPosition : transform.position;
        return Vector3.SqrMagnitude(center - position) >= minDistance * minDistance;
    }
}
