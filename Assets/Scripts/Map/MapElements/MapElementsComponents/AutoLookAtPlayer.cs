using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLookAtPlayer : MonoBehaviour {

    protected Transform playerTransform;

    void Start() {
        playerTransform = GameManager.Instance.player.transform;
    }

    void Update() {
        transform.LookAt(playerTransform);
    }
}
