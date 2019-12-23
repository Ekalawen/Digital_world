using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour {

    public Vector3 initialGravityDirection = Vector3.down;
    public float initialGravityIntensity = 5; // 5 est la gravity par défautl !

    protected Vector3 gravityDirection;
    protected float gravityIntensity;

    public void Initialize() {
        SetGravity(initialGravityDirection, initialGravityIntensity);
    }

    public Vector3 ApplyGravity(Vector3 initialMovement) {
        return initialMovement + gravityDirection * gravityIntensity;
    }

    public Vector3 CounterGravity(Vector3 initialMovement) {
        return initialMovement - gravityDirection * gravityIntensity;
    }

    public Vector3 MoveOppositeDirectionOfGravity(Vector3 initialMovement, float intensityMovement) {
        return initialMovement - gravityDirection * intensityMovement;
    }

    public void SetGravity(Vector3 gravityDirection, float gravityIntensity) {
        this.gravityDirection = gravityDirection.normalized;
        this.gravityIntensity = gravityIntensity;
    }

}
