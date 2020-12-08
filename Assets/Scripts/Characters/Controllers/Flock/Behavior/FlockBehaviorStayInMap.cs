using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlockBehaviorStayInMap : FlockBehavior {

    public float radiusRatio = 1.0f;

    protected Vector3 mapCenter;
    protected float radius;

    public override void Initialize() {
        base.Initialize();
        mapCenter = gm.map.GetCenter();
        radius = (gm.map.tailleMap.x + gm.map.tailleMap.y + gm.map.tailleMap.z) / 3.0f * radiusRatio;
    }

    public override Vector3 CalculateMove(IController flockController) {
        Vector3 position = flockController.transform.position;
        float distance = Vector3.Distance(mapCenter, position);
        if(distance >= radius) {
            float avancement = (distance / radius) - 1;
            return (mapCenter - position) * avancement * avancement;
        } else {
            return Vector3.zero;
        }
    }
}
