using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlockBehaviorCohesion : FlockBehavior {

    public float radius = 3f;

    public override Vector3 CalculateMove(IController flockController) {
        List<IController> others = flockManager.GetOtherAgentsInRadius(flockController, radius);
        if (others.Count == 0)
            return Vector3.zero;
        Vector3 barycentre = others.Aggregate(Vector3.zero, (acc, o) => acc + o.transform.position) / others.Count;
        Vector3 direction = barycentre - flockController.transform.position; // Ne pas normaliser !
        return InterpolateWithForward(direction, flockController).normalized;
    }
}
