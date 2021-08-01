using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierFarFromCenter : GetHelperModifier {

    public enum DistanceType { L2, CUBE, L_INFINI };

    public float minDistance = 3.0f;
    public bool useCustomPosition = false;
    [ConditionalHide("useCustomPosition")]
    public Vector3 customPosition;
    public DistanceType distanceType = DistanceType.L2;

    public override bool IsInArea(Vector3 position) {
        Vector3 center = useCustomPosition ? customPosition : transform.position;
        return IfFarEnought(center, position);
    }

    protected bool IfFarEnought(Vector3 pos1, Vector3 pos2) {
        switch (distanceType) {
            case DistanceType.L2:
                return Vector3.SqrMagnitude(pos1 - pos2) >= minDistance * minDistance;
            case DistanceType.CUBE:
                return MathTools.CubeDistance(pos1, pos2) >= minDistance;
            case DistanceType.L_INFINI:
                return MathTools.DistanceLInfini(pos1, pos2) >= minDistance;
            default:
                Debug.LogError("Mauvais type de distance dans GetHelperModifierFarFromDistance! :p", this);
                return false;
        }
    }
}
