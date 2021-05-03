using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoOrientLightning : MonoBehaviour {

    protected Vector3 startingAxe;
    protected Transform playerTransform;
    protected GameManager gm;

    void Start() {
        gm = GameManager.Instance;
        startingAxe = transform.forward;
    }

    void Update() {
        //Vector3 cameraProjectedPos = Vector3.ProjectOnPlane(cameraTransform.position, startingAxe);
        Vector3 playerDirection = gm.player.transform.position - transform.position;
        transform.LookAt(transform.position + startingAxe, playerDirection);
    }
}
