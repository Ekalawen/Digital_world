using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCubesOnContactComponent : MonoBehaviour {
    void OnControllerColliderHit(ControllerColliderHit hit) {
        Cube cube = hit.gameObject.GetComponent<Cube>();
        if (cube != null) {
            cube.Explode();
        }
    }
}
