using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoUpLookAtCamera : MonoBehaviour {

    protected Camera cam;

    void Start() {
        cam = Camera.main;
    }

    void Update() {
        Vector3 currentAxe = transform.forward;
        Vector3 cameraDirection = cam.transform.position - transform.position;
        transform.LookAt(transform.position + currentAxe, cameraDirection);
    }
}
