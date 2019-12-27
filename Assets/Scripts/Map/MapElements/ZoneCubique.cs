using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ZoneCubique : MonoBehaviour {

    public void Resize(Vector3 center, Vector3 halfExtents) {
        transform.localScale = halfExtents * 2;
        transform.position = center;
    }

    private void OnTriggerEnter(Collider other) {
        OnEnter(other);
    }
    private void OnTriggerExit(Collider other) {
        OnExit(other);
    }

    protected abstract void OnEnter(Collider other);
    protected abstract void OnExit(Collider other);
}
