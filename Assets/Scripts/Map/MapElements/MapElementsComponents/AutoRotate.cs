using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour {

    public bool useAxe = true;
    [ConditionalHide("useAxe")]
    public Vector3 axeToUse = Vector3.up;
    public float vitesse = 3.0f;
    public bool usePivot = false;
    [ConditionalHide("usePivot")]
    public Vector3 pivot;

    protected Vector3 axe;

    void Start() {
        if (useAxe && axeToUse != Vector3.zero)
            axe = axeToUse;
        else {
            ComputeRandomAxe();
        }
    }

    void Update() {
        float angle = 100 * vitesse * Time.deltaTime;
        if(usePivot) {
            transform.position += transform.rotation * pivot;
        }
        transform.RotateAround(transform.position, axe, angle);
        if(usePivot) {
            transform.position -= transform.rotation * pivot;
        }
    }

    protected void ComputeRandomAxe() {
        Vector3 randomPoint = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        axe = randomPoint.normalized;
    }

}
