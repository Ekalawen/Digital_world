using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTransform : MonoBehaviour {

    public Transform transformToLookAt;

    void Update() {
        if(transformToLookAt != null)
            transform.LookAt(transformToLookAt, Vector3.up);
    }
}
