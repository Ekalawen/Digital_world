using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearCurve : Curve {

    public override Vector3 GetAvancement(int indStartPoint, float t) {
        Vector3 p1 = points[indStartPoint];
        Vector3 p2 = (indStartPoint < points.Count - 1) ? points[indStartPoint + 1] : p1;
        Vector3 res = p1 + (p2 - p1) * t;
        return res;
    }

}
