using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoUpLookAtPlayer : MonoBehaviour {

    protected Transform playerTransform;
    protected GameManager gm;

    void Start() {
        gm = GameManager.Instance;
    }

    void Update() {
        Vector3 currentAxe = transform.forward;
        Vector3 playerDirection = gm.player.transform.position - transform.position;
        transform.LookAt(transform.position + currentAxe, playerDirection);
    }
}
