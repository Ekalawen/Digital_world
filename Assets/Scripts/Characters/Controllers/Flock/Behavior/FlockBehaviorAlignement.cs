using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlockBehaviorAlignement : FlockBehavior {

    public float radius = 3f;

    public override Vector3 CalculateMove(IController flockController) {
        List<IController> others = flockManager.GetOtherAgentsInRadius(flockController, radius);
        if (others.Count == 0)
            return flockController.transform.forward;
        Vector3 meanDirection = others.Aggregate(Vector3.zero, (acc, o) => acc + o.transform.forward) / others.Count;
        return meanDirection.normalized;
    }
}
