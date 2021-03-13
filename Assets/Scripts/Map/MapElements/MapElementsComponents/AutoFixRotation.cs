using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFixRotation : MonoBehaviour {

    public Quaternion rotation = Quaternion.identity;

    void Update() {
        transform.rotation = rotation;
    }
}
