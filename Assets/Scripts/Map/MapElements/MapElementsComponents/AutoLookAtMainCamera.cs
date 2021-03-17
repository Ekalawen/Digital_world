using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLookAtMainCamera : MonoBehaviour {

    protected Transform cameraTransform;

    void Start() {
        cameraTransform = Camera.main.transform;
    }

    void Update() {
        transform.LookAt(cameraTransform);
    }
}
