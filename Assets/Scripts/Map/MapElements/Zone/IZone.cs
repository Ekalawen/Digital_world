using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IZone : MonoBehaviour {

    protected GameManager gm;
    protected Bounds boundingBox;

    protected virtual void Start() {
        gm = GameManager.Instance;
        boundingBox = new Bounds(transform.position, transform.localScale);
    }

    public virtual void Initialize() {
        Start();
    }

    public virtual void Resize(Vector3 center, Vector3 halfExtents) {
        transform.localScale = halfExtents * 2;
        transform.position = center;
        boundingBox = new Bounds(transform.position, transform.localScale);
    }

    private void OnTriggerEnter(Collider other) {
        OnEnter(other);
    }
    private void OnTriggerExit(Collider other) {
        OnExit(other);
    }

    protected abstract void OnEnter(Collider other);
    protected abstract void OnExit(Collider other);

    public Vector3 GetRandomPosition() {
        return new Vector3(
            Random.Range(Mathf.RoundToInt(boundingBox.min.x), Mathf.RoundToInt(boundingBox.max.x) + 1),
            Random.Range(Mathf.RoundToInt(boundingBox.min.y), Mathf.RoundToInt(boundingBox.max.y) + 1),
            Random.Range(Mathf.RoundToInt(boundingBox.min.z), Mathf.RoundToInt(boundingBox.max.z) + 1));
    }
}
