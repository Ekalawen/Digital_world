using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IZone : MonoBehaviour {

    protected GameManager gm;

    protected virtual void Start() {
        gm = GameManager.Instance;
    }

    protected virtual void Initialize() {
        Start();
    }

    public virtual void Resize(Vector3 center, Vector3 halfExtents) {
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
