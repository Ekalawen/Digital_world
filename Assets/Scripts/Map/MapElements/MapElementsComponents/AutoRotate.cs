using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour {

    public Vector3 axeToUse = Vector3.up;
    public bool useAxe = true;
    public float vitesse = 3.0f;

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
        transform.RotateAround(transform.position, axe, angle);
    }

    protected void ComputeRandomAxe() {
        Vector3 randomPoint = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        axe = randomPoint.normalized;
    }

}
