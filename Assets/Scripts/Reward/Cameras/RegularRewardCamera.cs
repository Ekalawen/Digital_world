using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularRewardCamera : RewardCamera {

    public float thetaSpeed = 1.0f;

    protected Vector3 center;
    protected float distance;

    public override void Initialize() {
        base.Initialize();

        center = hm.mapCenter;

        Vector3Int tailleMap = hm.mapSize;
        distance = (tailleMap.x + tailleMap.y + tailleMap.z) / 3.0f;
        transform.position = new Vector3(distance * Mathf.Sqrt(3), hm.mapSize.y / 2, 0);
    }

    public override void Update() {
        Vector3 axe = Vector3.up;
        float angle = Time.deltaTime * thetaSpeed * 180.0f / Mathf.PI;
        transform.RotateAround(center, axe, angle);

        transform.LookAt(center, Vector3.up);
    }
}
