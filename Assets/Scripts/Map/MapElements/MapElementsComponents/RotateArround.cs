using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateArround : MonoBehaviour {

    public GameObject target;
    public bool useAxe = false;
    public Vector3 axeToUse;
    public float vitesse = 3.0f;

    protected Vector3 axe;
    protected CharacterController controller;
    protected Vector3 localVirtualPoint;

    public void Start() {
        if (useAxe && axeToUse != Vector3.zero)
            axe = axeToUse;
        else {
            ComputeRandomAxe();
        }
        controller = GetComponent<CharacterController>();
        localVirtualPoint = transform.position - target.transform.position;
    }

    protected void ComputeRandomAxe() {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Vector3 up = (direction == Vector3.up) ? Vector3.right : Vector3.up;
        Vector3 crossProduct = Vector3.Cross(direction, up);
        float angle = Random.Range(0.0f, 360.0f);
        Quaternion rotation = Quaternion.AngleAxis(angle, direction);
        axe = rotation * crossProduct;
    }

    protected Vector3 RotateAround(Vector3 position, Vector3 rotatePoint, Vector3 axis, float angle) {
        return (Quaternion.AngleAxis(angle, axis) * (position - rotatePoint)) + rotatePoint;
    }

    public void Update() {
        float angle = 100 * vitesse * Time.deltaTime;
        if(controller == null)
            transform.RotateAround(target.transform.position, axe, angle);
        else {
            localVirtualPoint = RotateAround(localVirtualPoint, Vector3.zero, axe, angle);
            Vector3 virtualPoint = localVirtualPoint + target.transform.position;
            //transform.position = (virtualPoint + transform.position) / 2.0f;
            Vector3 move = virtualPoint - transform.position;
            controller.Move(move);
        }
    }
}
