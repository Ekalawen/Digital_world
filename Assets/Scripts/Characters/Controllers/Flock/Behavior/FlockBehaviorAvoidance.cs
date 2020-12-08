using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlockBehaviorAvoidance : FlockBehavior {

    public float radius = 0.5f;

    public override Vector3 CalculateMove(IController flockController) {
        List<IController> others = flockManager.GetOtherAgentsInRadius(flockController, radius);
        if (others.Count == 0)
            return Vector3.zero;
        Vector3 barycentre = others.Aggregate(Vector3.zero, (acc, o) => {
            float distance = Vector3.Distance(o.transform.position, flockController.transform.position);
            float inverseDistance = MathCurves.Quadratic(0, radius, radius - distance);
            return acc - (o.transform.position - flockController.transform.position).normalized * inverseDistance;
        }) / others.Count;
        return InterpolateWithForward(barycentre, flockController);
    }
}
