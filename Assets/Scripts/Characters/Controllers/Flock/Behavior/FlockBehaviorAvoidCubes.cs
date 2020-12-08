using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlockBehaviorAvoidCubes : FlockBehavior {

    public float radius = 0.5f;

    public override Vector3 CalculateMove(IController flockController) {
        List<Cube> cubes = gm.map.GetRegularCubesInSphere(flockController.transform.position, radius);
        if (cubes.Count == 0)
            return Vector3.zero;
        Vector3 barycentre = cubes.Aggregate(Vector3.zero, (acc, o) => {
            float distance = Vector3.Distance(o.transform.position, flockController.transform.position);
            float inverseDistance = radius - distance;
            return acc - (o.transform.position - flockController.transform.position).normalized * inverseDistance;
        }) / cubes.Count;
        return barycentre;
    }
}
