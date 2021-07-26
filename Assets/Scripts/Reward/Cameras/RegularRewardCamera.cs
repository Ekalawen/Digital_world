using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularRewardCamera : RewardCamera {

    public float thetaSpeed = 1.0f;
    public float coefDistance = 1.0f;

    protected Vector3 center;
    protected float distance;

    public override void Initialize()
    {
        base.Initialize();

        center = hm.mapCenter;

        SetDistance();
    }

    public override void Update() {
        Vector3 axe = Vector3.up;
        float angle = Time.deltaTime * thetaSpeed * 180.0f / Mathf.PI;
        transform.RotateAround(center, axe, angle);
        SetDistance();

        transform.LookAt(center, Vector3.up);
    }

    protected void SetDistance() {
        Vector3Int tailleMap = hm.mapSize;
        distance = (tailleMap.x + tailleMap.y + tailleMap.z) / 3.0f;
        Vector3 direction = (transform.position - center).normalized;
        Vector3 newPosition = center + direction * (distance * coefDistance * Mathf.Sqrt(3));
        newPosition.y = hm.mapSize.y / 2;
        transform.position = newPosition;
    }
}
