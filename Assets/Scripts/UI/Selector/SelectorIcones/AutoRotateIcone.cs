using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotateIcone : MonoBehaviour {

    public Vector3 axis = Vector3.up;
    public float nbTours = 1;
    public float durationRotating = 1.0f;
    public float durationIdle = 1.0f;

    void Start() {
        StartCoroutine(CRotatePeriodically());
    }

    protected IEnumerator CRotatePeriodically() {
        while (true) {
            yield return Rotate();
            yield return new WaitForSeconds(durationIdle);
        }
    }


    public IEnumerator Rotate() {
        Timer timer = new Timer(durationRotating);
        Quaternion initialRotation = transform.localRotation;
        while(!timer.IsOver()) {
            float angle = nbTours * 360 * timer.GetAvancement();
            transform.localRotation = Quaternion.AngleAxis(angle, axis) * initialRotation;
            yield return null;
        }
        transform.localRotation = Quaternion.AngleAxis(nbTours * 360, axis) * initialRotation;
    }
}
