using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeoData {

    public enum GeoPointType { CONTINUE, IMPACT };

    [ColorUsageAttribute(true, true)]
    public Color color;
    public GeoPointType type = GeoPointType.IMPACT;
    [Tooltip("Only for Impact")]
    public float duration = 1.5f;
    public Transform targetObject = null;

    protected Vector3 targetPosition;

    public GeoData(GeoData other) {
        color = other.color;
        type = other.type;
        duration = other.duration;
        targetObject = other.targetObject;
        targetPosition = other.targetPosition;
    }

    public Vector3 GetTargetAndSaveIt() {
        if(targetObject != null) {
            targetPosition = targetObject.transform.position;
            return targetObject.transform.position;
        }
        return targetPosition;
    }

    public float GetDuration() {
        return type == GeoPointType.IMPACT ? duration : float.PositiveInfinity;
    }

    public void SetTargetPosition(Vector3 target) {
        targetPosition = target;
    }

    public Vector3 GetTargetPosition() {
        return targetPosition;
    }
}
