﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCamera : MonoBehaviour {

    public float thetaSpeed = 1.0f;

    protected HistoryManager hm;
    protected Vector3 center;
    protected float distance;

    private void Start() {
        hm = HistoryManager.Instance;

        center = Vector3.zero;
        foreach (TimedVector3 tpos in hm.GetPlayerHistory().positions) {
            center += tpos.position;
        }
        center /= hm.GetPlayerHistory().positions.Count;

        Vector3Int tailleMap = hm.mapSize;
        distance = (tailleMap.x + tailleMap.y + tailleMap.z) / 3.0f;
        transform.position = new Vector3(distance * Mathf.Sqrt(3), distance / 2, 0);
    }

    private void Update() {
        Vector3 axe = Vector3.up;
        float angle = Time.deltaTime * thetaSpeed * 180.0f / Mathf.PI;
        transform.RotateAround(center, axe, angle);

        transform.LookAt(center, Vector3.up);
    }

    public Vector3 GetCenter() {
        return center;
    }
}
